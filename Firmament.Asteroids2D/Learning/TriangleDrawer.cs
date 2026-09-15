using System.Runtime.CompilerServices;
using System.Text;
using FatCat.Toolkit.Console;
using Firmament.Core;
using Firmament.Core.Shaders;
using Firmament.Core.Types;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Firmament.Asteroids2D.Learning;

public unsafe class TriangleDrawer(LearningGame game) : IDisposable
{
	private readonly List<nint> unmanagedSemanticNames = [];

	private ComPtr<ID3D11InputLayout> inputLayout;
	private ComPtr<ID3D11PixelShader> pixelShader;

	private ComPtr<ID3D11Buffer> vertexBuffer;

	private InputElementDesc[] vertexLayoutDescription;

	private ComPtr<ID3D11VertexShader> vertexShader;

	private FirmamentWindow Window { get; } = game.Window;

	public void CleanUp()
	{
		vertexBuffer.Dispose();

		foreach (var semanticName in unmanagedSemanticNames)
		{
			SilkMarshal.Free(semanticName);
		}

		inputLayout.Dispose();
		pixelShader.Dispose();
		vertexShader.Dispose();
	}

	public void Dispose()
	{
		CleanUp();
	}

	public void Load()
	{
		CreateVertexBuffer();
		DescribeVertexLayout();

		PreLoadShaders();
		CreateShaders();
	}

	public void Render(double delta)
	{
		BindVertexBuffer();

		DrawTriangle();
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

		Window.DeviceContext.IASetVertexBuffers(0, 1, ref vertexBuffer, ref stride, ref offset);
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
			Window.Device.CreateInputLayout(
				ref vertexLayoutDescription[0],
				(uint)vertexLayoutDescription.Length,
				vertexByteCode.GetBufferPointer(),
				vertexByteCode.GetBufferSize(),
				ref inputLayout
			)
		);
	}

	private void CreateShaders()
	{
		var loader = Factory.Get<IShaderLoader>();
		var source = loader.GetShaderSource("pass-through");
		var compiler = D3DCompiler.GetApi();

		var vertexByteCode = CompileShader(compiler, source, "vertex_main", "vs_5_0");
		var pixelByteCode = CompileShader(compiler, source, "pixel_main", "ps_5_0");

		SilkMarshal.ThrowHResult(
			Window.Device.CreateVertexShader(
				vertexByteCode.GetBufferPointer(),
				vertexByteCode.GetBufferSize(),
				ref Unsafe.NullRef<ID3D11ClassLinkage>(),
				ref vertexShader
			)
		);

		SilkMarshal.ThrowHResult(
			Window.Device.CreatePixelShader(
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

			SilkMarshal.ThrowHResult(Window.Device.CreateBuffer(in bufferDescription, in initialData, ref vertexBuffer));
		}
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
		Window.DeviceContext.IASetInputLayout(inputLayout);
		Window.DeviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);

		Window.DeviceContext.VSSetShader(vertexShader, null, 0);
		Window.DeviceContext.PSSetShader(pixelShader, null, 0);

		Window.DeviceContext.Draw(3, 0);
	}

	private void PreLoadShaders()
	{
		ConsoleLog.WriteMagenta("Pre-loading shaders...");

		var loader = Factory.Get<IShaderLoader>();

		loader.PreLoadShader("pass-through");

		ConsoleLog.WriteMagenta("Shader pre-loading complete.");
	}
}
