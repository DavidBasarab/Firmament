using System.Runtime.CompilerServices;
using System.Text;
using FatCat.Toolkit.Console;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace Firmament.Core.Shaders;

public unsafe class Shader(FirmamentWindow window, IShaderLoader loader, string shaderName) : IDisposable
{
	private ComPtr<ID3D11InputLayout> inputLayout;
	private bool isDisposed;
	private ComPtr<ID3D11PixelShader> pixelShader;
	private string source;
	private ComPtr<ID3D11VertexShader> vertexShader;

	public string PixelEntryPoint { get; set; } = "pixel_main";

	public string PixelTarget { get; set; } = "ps_5_0";

	public string VertexEntryPoint { get; set; } = "vertex_main";

	public string VertexTarget { get; set; } = "vs_5_0";

	public void Bind()
	{
		window.DeviceContext.IASetInputLayout(inputLayout);
		window.DeviceContext.VSSetShader(vertexShader, Nulls.D3D11Class, 0);
		window.DeviceContext.PSSetShader(pixelShader, Nulls.D3D11Class, 0);
	}

	public void Dispose()
	{
		if (isDisposed)
		{
			return;
		}

		isDisposed = true;

		inputLayout.Dispose();
		pixelShader.Dispose();
		vertexShader.Dispose();
	}

	public void Load(InputElementDesc[] vertexLayout)
	{
		var complier = D3DCompiler.GetApi();

		source = loader.GetShaderSource(shaderName);

		try
		{
			LoadVertexStage(complier, vertexLayout);
			LoadPixelStage(complier);
		}
		finally
		{
			complier.Dispose();
		}
	}

	private ComPtr<ID3D10Blob> CompileShader(D3DCompiler compiler, string entryPoint, string target)
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

	private string DescribeCompileFailure(string entryPoint, int result, ComPtr<ID3D10Blob> errors)
	{
		if (errors.Handle is null)
		{
			return $"Compiling `{entryPoint}` failed with HRESULT 0x{result:x8} and produced no error text.";
		}

		var message = SilkMarshal.PtrToString((nint)errors.GetBufferPointer());

		return $"Compiling `{entryPoint}` failed: {message}";
	}

	private void LoadPixelStage(D3DCompiler compiler)
	{
		var byteCode = CompileShader(compiler, PixelEntryPoint, PixelTarget);

		try
		{
			SilkMarshal.ThrowHResult(
				window.Device.CreatePixelShader(
					byteCode.GetBufferPointer(),
					byteCode.GetBufferSize(),
					ref Unsafe.NullRef<ID3D11ClassLinkage>(),
					ref pixelShader
				)
			);
		}
		finally
		{
			byteCode.Dispose();
		}
	}

	private void LoadVertexStage(D3DCompiler compiler, InputElementDesc[] vertexLayout)
	{
		var byteCode = CompileShader(compiler, VertexEntryPoint, VertexTarget);

		try
		{
			SilkMarshal.ThrowHResult(
				window.Device.CreateVertexShader(
					byteCode.GetBufferPointer(),
					byteCode.GetBufferSize(),
					ref Unsafe.NullRef<ID3D11ClassLinkage>(),
					ref vertexShader
				)
			);

			SilkMarshal.ThrowHResult(
				window.Device.CreateInputLayout(
					ref vertexLayout[0],
					(uint)vertexLayout.Length,
					byteCode.GetBufferPointer(),
					byteCode.GetBufferSize(),
					ref inputLayout
				)
			);
		}
		finally
		{
			byteCode.Dispose();
		}
	}
}
