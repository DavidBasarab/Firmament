using System.Drawing;
using System.Runtime.CompilerServices;
using Firmament.Core.Extensions;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Firmament.Core;

public unsafe class FirmamentWindow : IDisposable
{
	private const double ColorTransitionSeconds = 5.0;
	private const double ReportIntervalSeconds = 0.1;

	private readonly List<Color> colors =
	[
		Color.BlueViolet,
		Color.Crimson,
		Color.DarkOrange,
		Color.DeepSkyBlue,
		Color.ForestGreen,
		Color.Gold,
		Color.HotPink,
		Color.Indigo,
		Color.LimeGreen,
		Color.MediumOrchid,
	];

	private readonly IWindow window;
	private uint backBufferHeight;

	private uint backBufferWidth;

	private int colorIndex;
	private double colorTransitionProgress;

	private float[] currentClearColor;

	private D3D11 d3d11;
	private ComPtr<ID3D11Device> device;
	private ComPtr<ID3D11DeviceContext> deviceContext;

	private DXGI dxgi;
	private IGamepad gamepad;

	private IInputContext input;
	private IKeyboard keyboard;

	private bool pauseBackgroundSwitch;

	private double peakRenderSeconds;
	private ComPtr<ID3D11RenderTargetView> renderTargetView;
	private double rendersSinceLastReport;

	private int resizeCount;

	private double secondsSinceLastReport;
	private float[] startColor;
	private ComPtr<IDXGISwapChain1> swapChain;
	private float[] targetColor;
	private int updatesSinceLastReport;

	public FirmamentWindow(int width, int height, string title)
	{
		var options = WindowOptions.Default with
		{
			API = GraphicsAPI.None,
			Size = new Vector2D<int>(width, height),
			Title = title,
		};

		window = Window.Create(options);

		window.Load += OnLoad;
		window.Update += OnUpdate;
		window.Render += OnRender;
		window.FramebufferResize += OnFrameBufferResize;
	}

	public void Dispose()
	{
		renderTargetView.Dispose();
		swapChain.Dispose();
		deviceContext.Dispose();
		device.Dispose();
		input.Dispose();
		window.Dispose();
	}

	public void Run()
	{
		window.Run();
	}

	private void AdvanceColorTransition(double delta)
	{
		colorTransitionProgress += delta / ColorTransitionSeconds;

		while (colorTransitionProgress >= 1.0)
		{
			colorTransitionProgress -= 1.0;
			MoveToNextColor();
		}

		InterpolateClearColor();
	}

	private void CreateRenderTargetView()
	{
		SilkMarshal.ThrowHResult(swapChain.GetBuffer(0, out ComPtr<ID3D11Texture2D> backBuffer));
		SilkMarshal.ThrowHResult(device.CreateRenderTargetView(backBuffer, null, ref renderTargetView));
		backBuffer.Dispose();
	}

	private void GetGamepad()
	{
		gamepad = input.Gamepads.FirstOrDefault();

		if (gamepad != null)
		{
			gamepad.ButtonDown += OnGamepadButtonDown;
		}
	}

	private void GetKeyboard()
	{
		keyboard = input.Keyboards.FirstOrDefault();

		if (keyboard != null)
		{
			keyboard.KeyDown += OnKeyDown;
		}
	}

	private int GetNextColorIndex()
	{
		return (colorIndex + 1) % colors.Count;
	}

	private void InitializeColorTransition()
	{
		startColor = colors[colorIndex].ToArray();
		targetColor = colors[GetNextColorIndex()].ToArray();
		currentClearColor = [0f, 0f, 0f, 0f];
		colorTransitionProgress = 0.0;
	}

	private void InterpolateClearColor()
	{
		var t = (float)colorTransitionProgress;

		for (var channel = 0; channel < currentClearColor.Length; channel++)
		{
			currentClearColor[channel] = startColor[channel] + (targetColor[channel] - startColor[channel]) * t;
		}
	}

	private void MoveToNextColor()
	{
		colorIndex = GetNextColorIndex();
		startColor = targetColor;
		targetColor = colors[GetNextColorIndex()].ToArray();
	}

	private void OnFrameBufferResize(Vector2D<int> size)
	{
		if (size.X <= 0 || size.Y <= 0)
		{
			return;
		}

		resizeCount++;

		ResizeSwapChain((uint)size.X, (uint)size.Y);
	}

	private void OnGamepadButtonDown(IGamepad gamepad, Button button)
	{
		RunActionForGamepadButtonDown(button, ButtonName.Start, () => window.Close());
		RunActionForGamepadButtonDown(button, ButtonName.A, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
	}

	private void OnKeyDown(IKeyboard source, Key key, int scancode)
	{
		RunActionForKeyDown(key, Key.Space, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
		RunActionForKeyDown(key, Key.Escape, () => window.Close());
	}

	private void OnLoad()
	{
		dxgi = DXGI.GetApi(window);
		d3d11 = D3D11.GetApi(window);

		SilkMarshal.ThrowHResult(
			d3d11.CreateDevice(
				default(ComPtr<IDXGIAdapter>),
				D3DDriverType.Hardware,
				0,
				(uint)CreateDeviceFlag.None,
				null,
				0,
				D3D11.SdkVersion,
				ref device,
				null,
				ref deviceContext
			)
		);

		var swapChainDesc = new SwapChainDesc1
		{
			BufferCount = 2,
			Format = Format.FormatB8G8R8A8Unorm,
			BufferUsage = DXGI.UsageRenderTargetOutput,
			SwapEffect = SwapEffect.FlipDiscard,
			SampleDesc = new SampleDesc(1, 0),
		};

		SilkMarshal.ThrowHResult(dxgi.CreateDXGIFactory2(0, out ComPtr<IDXGIFactory2> factory));

		SilkMarshal.ThrowHResult(
			factory.CreateSwapChainForHwnd(
				device,
				window.Native.DXHandle.Value,
				in swapChainDesc,
				null,
				ref Unsafe.NullRef<IDXGIOutput>(),
				ref swapChain
			)
		);

		factory.Dispose();

		CreateRenderTargetView();
		SetViewPort((uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);

		input = window.CreateInput();

		GetKeyboard();
		GetGamepad();
	}

	private void OnRender(double delta)
	{
		if (currentClearColor is null)
		{
			InitializeColorTransition();
		}

		if (!pauseBackgroundSwitch)
		{
			AdvanceColorTransition(delta);
		}

		deviceContext.OMSetRenderTargets(1, ref renderTargetView, (ComPtr<ID3D11DepthStencilView>)default);
		deviceContext.ClearRenderTargetView(renderTargetView, ref currentClearColor[0]);

		swapChain.Present(0, 0);

		secondsSinceLastReport += delta;
		rendersSinceLastReport++;

		if (delta > peakRenderSeconds)
		{
			peakRenderSeconds = delta;
		}

		if (secondsSinceLastReport < ReportIntervalSeconds)
		{
			return;
		}

		ReportPerformance();
		ResetWindow();
	}

	private void OnUpdate(double delta)
	{
		updatesSinceLastReport++;
	}

	private void ReportPerformance()
	{
		var framesPerSecond = rendersSinceLastReport / secondsSinceLastReport;
		var avgMilliseconds = secondsSinceLastReport / rendersSinceLastReport * 1000.0;
		var peakMilliseconds = peakRenderSeconds * 1000.0;
		var aspectRatio = (float)backBufferWidth / backBufferHeight;

		window.Title =
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliseconds:F2} ms avg | {peakMilliseconds:F2} ms peak | {updatesSinceLastReport} updates | Background Paused {pauseBackgroundSwitch} | {resizeCount} resizes | {backBufferWidth}x{backBufferHeight} @ {aspectRatio:F2}:1";
	}

	private void ResetWindow()
	{
		secondsSinceLastReport = 0.0;
		updatesSinceLastReport = 0;
		rendersSinceLastReport = 0;
		peakRenderSeconds = 0.0;
	}

	private void ResizeSwapChain(uint width, uint height)
	{
		var noRenderTargets = default(ComPtr<ID3D11RenderTargetView>);

		deviceContext.OMSetRenderTargets(0, ref noRenderTargets, (ComPtr<ID3D11DepthStencilView>)default);

		renderTargetView.Dispose();

		SilkMarshal.ThrowHResult(swapChain.ResizeBuffers(0, width, height, Format.FormatUnknown, 0));

		CreateRenderTargetView();
		SetViewPort(width, height);
	}

	private void RunActionForGamepadButtonDown(Button button, ButtonName expectName, Action action)
	{
		if (button.Name == expectName)
		{
			action();
		}
	}

	private void RunActionForKeyDown(Key key, Key expectedValue, Action action)
	{
		if (key == expectedValue)
		{
			action();
		}
	}

	private void SetViewPort(uint width, uint height)
	{
		var viewport = new Viewport
		{
			TopLeftX = 0,
			TopLeftY = 0,
			Width = width,
			Height = height,
			MinDepth = 0.0f,
			MaxDepth = 1.0f,
		};

		deviceContext.RSSetViewports(1, ref viewport);

		backBufferHeight = height;
		backBufferWidth = width;
	}
}
