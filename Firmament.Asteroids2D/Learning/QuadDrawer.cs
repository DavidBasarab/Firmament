using Firmament.Core;
using Firmament.Core.Shaders;
using Firmament.Core.Types;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Firmament.Asteroids2D.Learning;

public unsafe class QuadDrawer(LearningGame game) : GameAction(game), IDisposable
{
	private const float TurnInSeconds = 12.0f;

	private static Vertex LowerLeft { get; } = new(-0.5f, -0.5f, 1f, 1f, 1f);

	private static Vertex LowerRight { get; } = new(0.5f, -0.5f, 0f, 0f, 1f);

	private static Vertex UpperLeft { get; } = new(-0.5f, 0.5f, 1f, 0f, 0f);

	private static Vertex UpperRight { get; } = new(0.5f, 0.5f, 0f, 1f, 0f);

	private readonly List<nint> unmanagedSemanticNames = [];
	private float elapsedSecondsInTurn;
	private ComPtr<ID3D11Buffer> indexBuffer;

	private Shader shader;

	private ComPtr<ID3D11Buffer> vertexBuffer;

	public void Dispose()
	{
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		shader.Dispose();
	}

	public void Load()
	{
		CreateIndexBuffer();

		LoadShader();
	}

	public void Render(double delta)
	{
		// Could this stay in load and I just change the objects?
		CreateVertexBuffer(delta);

		BindVertexBuffer();
		BindIndexBuffer();
		shader.Bind();

		DrawQuad();
	}

	private byte* AllocatedSemanticName(string semanticName)
	{
		var pointer = SilkMarshal.StringToPtr(semanticName);

		unmanagedSemanticNames.Add(pointer);

		return (byte*)pointer;
	}

	private void BindIndexBuffer()
	{
		DeviceContext.IASetIndexBuffer(indexBuffer, Format.FormatR16Uint, 0);
	}

	private void BindVertexBuffer()
	{
		var stride = (uint)sizeof(Vertex);
		var offset = 0u;

		DeviceContext.IASetVertexBuffers(0, 1, ref vertexBuffer, ref stride, ref offset);
	}

	private void CreateIndexBuffer()
	{
		ushort[] indices = [0, 1, 2, 0, 2, 3];

		var bufferDescription = new BufferDesc
		{
			ByteWidth = (uint)(sizeof(ushort) * indices.Length),
			Usage = Usage.Immutable,
			BindFlags = (uint)BindFlag.IndexBuffer,
		};

		fixed (ushort* indexPointer = indices)
		{
			var initialData = new SubresourceData { PSysMem = indexPointer };

			SilkMarshal.ThrowHResult(Window.Device.CreateBuffer(in bufferDescription, in initialData, ref indexBuffer));
		}
	}

	private void CreateVertexBuffer(double delta)
	{
		elapsedSecondsInTurn = (elapsedSecondsInTurn + (float)delta) % TurnInSeconds;

		var angle = MathF.Tau * (elapsedSecondsInTurn / TurnInSeconds);

		Vertex[] vertices =
		[
			RotateAround(UpperLeft, angle),
			RotateAround(UpperRight, angle),
			RotateAround(LowerRight, angle),
			RotateAround(LowerLeft, angle),
		];

		var bufferDescription = new BufferDesc
		{
			ByteWidth = (uint)(sizeof(Vertex) * vertices.Length),
			Usage = Usage.Immutable,
			BindFlags = (uint)BindFlag.VertexBuffer,
		};

		vertexBuffer.Dispose();

		fixed (Vertex* vertexPtr = vertices)
		{
			var initialData = new SubresourceData { PSysMem = vertexPtr };

			SilkMarshal.ThrowHResult(Window.Device.CreateBuffer(in bufferDescription, in initialData, ref vertexBuffer));
		}
	}

	private InputElementDesc[] DescribeVertexLayout()
	{
		return
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

	private void DrawQuad()
	{
		DeviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
		DeviceContext.DrawIndexed(6, 0, 0);
	}

	private void FreeSemanticNames()
	{
		foreach (var semanticName in unmanagedSemanticNames)
		{
			SilkMarshal.Free(semanticName);
		}

		unmanagedSemanticNames.Clear();
	}

	private void LoadShader()
	{
		shader = new Shader(Window, Factory.Get<IShaderLoader>(), "pass-through");

		try
		{
			shader.Load(DescribeVertexLayout());
		}
		finally
		{
			FreeSemanticNames();
		}
	}

	private Vertex RotateAround(Vertex point, float angle)
	{
		var cosTheta = MathF.Cos(angle);
		var sinTheta = MathF.Sin(angle);

		var x = point.X * cosTheta - point.Y * sinTheta;
		var y = point.X * sinTheta + point.Y * cosTheta;

		return new Vertex(x, y, point.R, point.G, point.B);
	}
}
