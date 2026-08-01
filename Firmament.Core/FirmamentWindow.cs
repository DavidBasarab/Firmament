using System.Drawing;
using System.Runtime.CompilerServices;
using Firmament.Core.Extensions;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Firmament.Core;

public unsafe class FirmamentWindow : IDisposable
{
	private const double ReportIntervalSeconds = 1.0;

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

	private int colorIndex;

	private D3D11 d3d11;
	private ComPtr<ID3D11Device> device;
	private ComPtr<ID3D11DeviceContext> deviceContext;

	private DXGI dxgi;

	private double elapsed;
	private double peakRenderSeconds;
	private ComPtr<ID3D11RenderTargetView> renderTargetView;
	private double rendersSinceLastReport;

	private double secondsSinceLastReport;
	private ComPtr<IDXGISwapChain1> swapChain;
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
	}

	public void Dispose()
	{
		renderTargetView.Dispose();
		swapChain.Dispose();
		deviceContext.Dispose();
		device.Dispose();
		window.Dispose();
	}

	public void Run()
	{
		window.Run();
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

		ComPtr<IDXGIFactory2> factory = default;

		SilkMarshal.ThrowHResult(dxgi.CreateDXGIFactory2(0, out factory));

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

		ComPtr<ID3D11Texture2D> backBuffer = default;

		SilkMarshal.ThrowHResult(swapChain.GetBuffer(0, out backBuffer));

		SilkMarshal.ThrowHResult(device.CreateRenderTargetView(backBuffer, null, ref renderTargetView));
		backBuffer.Dispose();
	}

	private void OnRender(double delta)
	{
		elapsed += delta;

		var clearColor = colors[colorIndex].ToArray();

		deviceContext.OMSetRenderTargets(1, ref renderTargetView, (ComPtr<ID3D11DepthStencilView>)default);
		deviceContext.ClearRenderTargetView(renderTargetView, ref clearColor[0]);

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

		colorIndex = GetNextColorIndex();

		ReportPerformance();
		ResetWindow();
	}

	private int GetNextColorIndex()
	{
		return (colorIndex + 1) % colors.Count;
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

		window.Title =
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliseconds:F2} ms avg | {peakMilliseconds:F2} ms peak | {updatesSinceLastReport} updates";
	}

	private void ResetWindow()
	{
		secondsSinceLastReport = 0.0;
		updatesSinceLastReport = 0;
		rendersSinceLastReport = 0;
		peakRenderSeconds = 0.0;
	}
}
