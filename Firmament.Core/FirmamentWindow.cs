using System.Runtime.CompilerServices;
using System.Text;
using FatCat.Toolkit.Console;
using Firmament.Core.Extensions;
using Firmament.Core.Shaders;
using Firmament.Core.Types;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Feature = Silk.NET.DXGI.Feature;

namespace Firmament.Core;

public unsafe class FirmamentWindow : IDisposable
{
	private const double ReportIntervalSeconds = 0.1;

	private readonly List<nint> unmanagedSemanticNames = [];

	private bool allowTearingSupported;
	private uint backBufferHeight;
	private uint backBufferWidth;

	private float[] clearColor = [0.0f, 0.0f, 0.0f, 1.0f];

	private D3D11 d3D11;
	private ComPtr<ID3D11Device> device;
	private ComPtr<ID3D11DeviceContext> deviceContext;
	private DXGI dxgi;
	private IGamepad gamepad;
	private IInputContext input;
	private ComPtr<ID3D11InputLayout> inputLayout;
	private IKeyboard keyboard;
	private bool pauseBackgroundSwitch;
	private double peakRenderSeconds;
	private ComPtr<ID3D11PixelShader> pixelShader;
	private uint presentSyncInterval = 1;
	private ComPtr<ID3D11RenderTargetView> renderTargetView;
	private double rendersSinceLastReport;
	private int resizeCount;
	private double secondsSinceLastReport;

	private ComPtr<IDXGISwapChain1> swapChain;
	private uint swapChainFlags;

	private int updatesSinceLastReport;

	private ComPtr<ID3D11Buffer> vertexBuffer;
	private InputElementDesc[] vertexLayoutDescription;

	private ComPtr<ID3D11VertexShader> vertexShader;

	private IWindow Window { get; }

	public FirmamentWindow(int width, int height, string title)
	{
		var options = WindowOptions.Default with
		{
			API = GraphicsAPI.None,
			Size = new Vector2D<int>(width, height),
			Title = title,
		};

		Window = Silk.NET.Windowing.Window.Create(options);

		Window.Load += OnLoad;
		Window.Update += OnUpdate;
		Window.Render += OnRender;
		Window.FramebufferResize += OnFrameBufferResize;
	}

	public event Action Load;

	public event Action<double> Render;

	public event Action<double> Update;

	public event Action CleanUp;

	public void Dispose()
	{
		CleanUp?.Invoke();
		renderTargetView.Dispose();
		swapChain.Dispose();
		deviceContext.Dispose();
		device.Dispose();
		input.Dispose();
		Window.Dispose();

		vertexBuffer.Dispose();

		foreach (var semanticName in unmanagedSemanticNames)
		{
			SilkMarshal.Free(semanticName);
		}

		inputLayout.Dispose();
		pixelShader.Dispose();
		vertexShader.Dispose();
	}

	public void Run()
	{
		Window.Run();
	}

	public void SetClearColor(float[] color)
	{
		clearColor = color;
	}

	private byte* AllocatedSemanticName(string semanticName)
	{
		var pointer = SilkMarshal.StringToPtr(semanticName);

		unmanagedSemanticNames.Add(pointer);

		return (byte*)pointer;
	}

	private void BindVertexBuffer()
	{
		var stride = (uint)sizeof(Vertex);
		var offset = 0u;

		deviceContext.IASetVertexBuffers(0, 1, ref vertexBuffer, ref stride, ref offset);
	}

	private ComPtr<ID3D10Blob> CompileShader(D3DCompiler compiler, string source, string entryPoint, string target)
	{
		ConsoleLog.WriteCyan($"Compiling shader `{entryPoint}` for target `{target}`...");

		ComPtr<ID3D10Blob> byteCode = default;
		ComPtr<ID3D10Blob> errors = default;

		try
		{
			var sourceBytes = Encoding.ASCII.GetBytes(source);

			ConsoleLog.WriteDarkYellow($"Shader source length: {sourceBytes.Length} bytes");

			fixed (byte* sourcePointer = sourceBytes)
			{
				var result = compiler.Compile(
					sourcePointer,
					(nuint)sourceBytes.Length,
					(byte*)null,
					null,
					ref Unsafe.NullRef<ID3DInclude>(),
					entryPoint,
					target,
					0,
					0,
					ref byteCode,
					ref errors
				);

				if (result < 0)
				{
					ConsoleLog.WriteDarkRed($"Shader compilation failed for entry point `{entryPoint}` and target `{target}`");

					throw new InvalidOperationException(DescribeCompileFailure(entryPoint, result, errors));
				}
			}
		}
		finally
		{
			errors.Dispose();
		}

		return byteCode;
	}

	private void CreateInputLayout(ComPtr<ID3D10Blob> vertexByteCode)
	{
		SilkMarshal.ThrowHResult(
			device.CreateInputLayout(
				ref vertexLayoutDescription[0],
				(uint)vertexLayoutDescription.Length,
				vertexByteCode.GetBufferPointer(),
				vertexByteCode.GetBufferSize(),
				ref inputLayout
			)
		);
	}

	private void CreateRenderTargetView()
	{
		SilkMarshal.ThrowHResult(swapChain.GetBuffer(0, out ComPtr<ID3D11Texture2D> backBuffer));
		SilkMarshal.ThrowHResult(device.CreateRenderTargetView(backBuffer, null, ref renderTargetView));
		backBuffer.Dispose();
	}

	private void CreateShaders()
	{
		var loader = Factory.Get<IShaderLoader>();
		var source = loader.GetShaderSource("pass-through");
		var compiler = D3DCompiler.GetApi();

		var vertexByteCode = CompileShader(compiler, source, "vertex_main", "vs_5_0");
		var pixelByteCode = CompileShader(compiler, source, "pixel_main", "ps_5_0");

		SilkMarshal.ThrowHResult(
			device.CreateVertexShader(
				vertexByteCode.GetBufferPointer(),
				vertexByteCode.GetBufferSize(),
				ref Unsafe.NullRef<ID3D11ClassLinkage>(),
				ref vertexShader
			)
		);

		SilkMarshal.ThrowHResult(
			device.CreatePixelShader(
				pixelByteCode.GetBufferPointer(),
				pixelByteCode.GetBufferSize(),
				ref Unsafe.NullRef<ID3D11ClassLinkage>(),
				ref pixelShader
			)
		);

		CreateInputLayout(vertexByteCode);

		vertexByteCode.Dispose();
		pixelByteCode.Dispose();
		compiler.Dispose();
	}

	private void CreateVertexBuffer()
	{
		Vertex[] vertices = [new(0.0f, 0.5f, 1f, 0f, 0f), new(0.5f, -0.5f, 0f, 1f, 0f), new(-0.5f, -0.5f, 0f, 0f, 1f)];

		var bufferDescription = new BufferDesc
		{
			ByteWidth = (uint)(sizeof(Vertex) * vertices.Length),
			Usage = Usage.Immutable,
			BindFlags = (uint)BindFlag.VertexBuffer,
		};

		fixed (Vertex* vertexPtr = vertices)
		{
			var initialData = new SubresourceData { PSysMem = vertexPtr };

			SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDescription, in initialData, ref vertexBuffer));
		}
	}

	private void CyclePresentSyncInterval()
	{
		presentSyncInterval = presentSyncInterval switch
		{
			0 => 1,
			1 => 2,
			_ => 0,
		};
	}

	private string DescribeCompileFailure(string entryPoint, int result, ComPtr<ID3D10Blob> errors)
	{
		if (errors.Handle is null)
		{
			return $"Compiling `{entryPoint}` failed with HRESULT 0x{result:x8} and produced no error text.";
		}

		var message = SilkMarshal.PtrToString((nint)errors.GetBufferPointer());

		return $"Compiling `{entryPoint}` failed: {message}";
	}

	private string DescribePresentMode()
	{
		if (presentSyncInterval > 0)
		{
			return $"vsync {presentSyncInterval}";
		}

		if (allowTearingSupported)
		{
			return "tearing";
		}

		return "no-sync";
	}

	private void DescribeVertexLayout()
	{
		vertexLayoutDescription =
		[
			new InputElementDesc
			{
				SemanticName = AllocatedSemanticName("POSITION"),
				SemanticIndex = 0,
				Format = Format.FormatR32G32Float,
				InputSlot = 0,
				AlignedByteOffset = 0,
				InputSlotClass = InputClassification.PerVertexData,
				InstanceDataStepRate = 0,
			},
			new InputElementDesc
			{
				SemanticName = AllocatedSemanticName("COLOR"),
				SemanticIndex = 0,
				Format = Format.FormatR32G32B32Float,
				InputSlot = 0,
				AlignedByteOffset = 8,
				InputSlotClass = InputClassification.PerVertexData,
				InstanceDataStepRate = 0,
			},
		];
	}

	private void DrawTriangle()
	{
		deviceContext.IASetInputLayout(inputLayout);
		deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);

		deviceContext.VSSetShader(vertexShader, null, 0);
		deviceContext.PSSetShader(pixelShader, null, 0);

		deviceContext.Draw(3, 0);
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

	private uint GetPresentFlags()
	{
		if (presentSyncInterval == 0 && allowTearingSupported)
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

		resizeCount++;

		ResizeSwapChain((uint)size.X, (uint)size.Y);
	}

	private void OnGamepadButtonDown(IGamepad gamepad, Button button)
	{
		button.RunAction(ButtonName.Start, () => Window.Close());
		button.RunAction(ButtonName.A, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
	}

	private void OnKeyDown(IKeyboard source, Key key, int scancode)
	{
		key.RunAction(Key.Space, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
		key.RunAction(Key.Escape, () => Window.Close());
		key.RunAction(Key.V, CyclePresentSyncInterval);
	}

	private void OnLoad()
	{
		dxgi = DXGI.GetApi(Window);
		d3D11 = D3D11.GetApi(Window);

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

		allowTearingSupported = IsTearingSupported(factory);
		swapChainFlags = allowTearingSupported ? (uint)SwapChainFlag.AllowTearing : 0;

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
				Window.Native.DXHandle.Value,
				in swapChainDesc,
				null,
				ref Unsafe.NullRef<IDXGIOutput>(),
				ref swapChain
			)
		);

		factory.Dispose();

		CreateRenderTargetView();
		SetViewPort((uint)Window.FramebufferSize.X, (uint)Window.FramebufferSize.Y);

		input = Window.CreateInput();

		GetKeyboard();
		GetGamepad();

		CreateVertexBuffer();
		DescribeVertexLayout();

		PreLoadShaders();
		CreateShaders();

		Load?.Invoke();
	}

	private void OnRender(double delta)
	{
		if (!pauseBackgroundSwitch) { }

		Render?.Invoke(delta);

		deviceContext.OMSetRenderTargets(1, ref renderTargetView, (ComPtr<ID3D11DepthStencilView>)default);
		deviceContext.ClearRenderTargetView(renderTargetView, ref clearColor[0]);

		BindVertexBuffer();

		DrawTriangle();

		SilkMarshal.ThrowHResult(swapChain.Present(presentSyncInterval, GetPresentFlags()));

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

		Update?.Invoke(delta);
	}

	private void PreLoadShaders()
	{
		ConsoleLog.WriteMagenta("Pre-loading shaders...");

		var loader = Factory.Get<IShaderLoader>();

		loader.PreLoadShader("pass-through");

		ConsoleLog.WriteMagenta("Shader pre-loading complete.");
	}

	private void ReportPerformance()
	{
		var framesPerSecond = rendersSinceLastReport / secondsSinceLastReport;
		var avgMilliseconds = secondsSinceLastReport / rendersSinceLastReport * 1000.0;
		var peakMilliseconds = peakRenderSeconds * 1000.0;
		var aspectRatio = (float)backBufferWidth / backBufferHeight;

		Window.Title =
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliseconds:F2} ms avg | {peakMilliseconds:F2} ms peak | {updatesSinceLastReport} updates | Background Paused {pauseBackgroundSwitch} | {resizeCount} resizes | {backBufferWidth}x{backBufferHeight} @ {aspectRatio:F2}:1 | {DescribePresentMode()}";
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

		backBufferHeight = height;
		backBufferWidth = width;
	}
}
