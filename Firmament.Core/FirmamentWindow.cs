using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Feature = Silk.NET.DXGI.Feature;

namespace Firmament.Core;

public unsafe class FirmamentWindow : IDisposable
{
	private float[] clearColor = [0.0f, 0.0f, 0.0f, 1.0f];

	private D3D11 d3D11;
	private ComPtr<ID3D11Device> device;
	private ComPtr<ID3D11DeviceContext> deviceContext;
	private DXGI dxgi;

	private IInputContext input;

	private ComPtr<ID3D11RenderTargetView> renderTargetView;

	private ComPtr<IDXGISwapChain1> swapChain;
	private uint swapChainFlags;

	public bool AllowTearingSupported { get; private set; }

	public uint BackBufferHeight { get; private set; }

	public uint BackBufferWidth { get; private set; }

	public ComPtr<ID3D11Device> Device => device;

	public ComPtr<ID3D11DeviceContext> DeviceContext => deviceContext;

	public IGamepad Gamepad { get; private set; }

	public IKeyboard Keyboard { get; private set; }

	public uint PresentSyncInterval { get; set; } = 1;

	public int ResizeCount { get; private set; }

	private IWindow SilkWindow { get; }

	public FirmamentWindow(int width, int height, string title)
	{
		var options = WindowOptions.Default with
		{
			API = GraphicsAPI.None,
			Size = new Vector2D<int>(width, height),
			Title = title,
		};

		SilkWindow = Window.Create(options);

		SilkWindow.Load += OnLoad;
		SilkWindow.Update += OnUpdate;
		SilkWindow.Render += OnRender;
		SilkWindow.FramebufferResize += OnFrameBufferResize;
	}

	public event Action CleanUp;

	public event Action<IGamepad, Button> GamepadButtonDown;

	public event Action<IKeyboard, Key, int> KeyDown;

	public event Action Load;

	public event Action<double> Render;

	public event Action<double> Update;

	public void Close()
	{
		SilkWindow.Close();
	}

	public void Dispose()
	{
		CleanUp?.Invoke();
		renderTargetView.Dispose();
		swapChain.Dispose();
		deviceContext.Dispose();
		device.Dispose();
		input.Dispose();
		SilkWindow.Dispose();
	}

	public void Run()
	{
		SilkWindow.Run();
	}

	public void SetClearColor(float[] color)
	{
		clearColor = color;
	}

	public void SetWindowTitle(string title)
	{
		SilkWindow.Title = title;
	}

	private void CreateRenderTargetView()
	{
		SilkMarshal.ThrowHResult(swapChain.GetBuffer(0, out ComPtr<ID3D11Texture2D> backBuffer));
		SilkMarshal.ThrowHResult(device.CreateRenderTargetView(backBuffer, null, ref renderTargetView));
		backBuffer.Dispose();
	}

	private void GetGamepad()
	{
		Gamepad = input.Gamepads.FirstOrDefault();

		if (Gamepad != null && GamepadButtonDown is not null)
		{
			Gamepad.ButtonDown += GamepadButtonDown;
		}
	}

	private void GetKeyboard()
	{
		Keyboard = input.Keyboards.FirstOrDefault();

		if (Keyboard != null && KeyDown is not null)
		{
			Keyboard.KeyDown += KeyDown;
		}
	}

	private uint GetPresentFlags()
	{
		if (PresentSyncInterval == 0 && AllowTearingSupported)
		{
			return DXGI.PresentAllowTearing;
		}

		return 0;
	}

	private bool IsTearingSupported(ComPtr<IDXGIFactory2> factory)
	{
		if (factory.QueryInterface(out ComPtr<IDXGIFactory5> factory5) < 0)
		{
			return false;
		}

		var allowTearing = 0;

		var result = factory5.CheckFeatureSupport(Feature.PresentAllowTearing, ref allowTearing, sizeof(int));

		factory5.Dispose();

		return result >= 0 && allowTearing != 0;
	}

	private void OnFrameBufferResize(Vector2D<int> size)
	{
		if (size.X <= 0 || size.Y <= 0)
		{
			return;
		}

		ResizeCount++;

		ResizeSwapChain((uint)size.X, (uint)size.Y);
	}

	private void OnLoad()
	{
		dxgi = DXGI.GetApi(SilkWindow);
		d3D11 = D3D11.GetApi(SilkWindow);

		SilkMarshal.ThrowHResult(
			d3D11.CreateDevice(
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

		SilkMarshal.ThrowHResult(dxgi.CreateDXGIFactory2(0, out ComPtr<IDXGIFactory2> factory));

		AllowTearingSupported = IsTearingSupported(factory);
		swapChainFlags = AllowTearingSupported ? (uint)SwapChainFlag.AllowTearing : 0;

		var swapChainDesc = new SwapChainDesc1
		{
			BufferCount = 2,
			Format = Format.FormatB8G8R8A8Unorm,
			BufferUsage = DXGI.UsageRenderTargetOutput,
			SwapEffect = SwapEffect.FlipDiscard,
			SampleDesc = new SampleDesc(1, 0),
			Flags = swapChainFlags,
		};

		SilkMarshal.ThrowHResult(
			factory.CreateSwapChainForHwnd(
				device,
				SilkWindow.Native.DXHandle.Value,
				in swapChainDesc,
				null,
				ref Unsafe.NullRef<IDXGIOutput>(),
				ref swapChain
			)
		);

		factory.Dispose();

		CreateRenderTargetView();
		SetViewPort((uint)SilkWindow.FramebufferSize.X, (uint)SilkWindow.FramebufferSize.Y);

		input = SilkWindow.CreateInput();

		GetKeyboard();
		GetGamepad();

		Load?.Invoke();
	}

	private void OnRender(double delta)
	{
		deviceContext.OMSetRenderTargets(1, ref renderTargetView, (ComPtr<ID3D11DepthStencilView>)default);
		deviceContext.ClearRenderTargetView(renderTargetView, ref clearColor[0]);

		Render?.Invoke(delta);

		SilkMarshal.ThrowHResult(swapChain.Present(PresentSyncInterval, GetPresentFlags()));
	}

	private void OnUpdate(double delta)
	{
		Update?.Invoke(delta);
	}

	private void ResizeSwapChain(uint width, uint height)
	{
		var noRenderTargets = default(ComPtr<ID3D11RenderTargetView>);

		deviceContext.OMSetRenderTargets(0, ref noRenderTargets, (ComPtr<ID3D11DepthStencilView>)default);

		renderTargetView.Dispose();

		SilkMarshal.ThrowHResult(swapChain.ResizeBuffers(0, width, height, Format.FormatUnknown, swapChainFlags));

		CreateRenderTargetView();
		SetViewPort(width, height);
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

		BackBufferHeight = height;
		BackBufferWidth = width;
	}
}
