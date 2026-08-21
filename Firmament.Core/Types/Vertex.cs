using System.Runtime.InteropServices;

namespace Firmament.Core.Types;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex
{
	public Vertex() { }

	public Vertex(float x, float y, float r, float g, float b)
	{
		X = x;
		Y = y;
		R = r;
		G = g;
		B = b;
	}

	public float X { get; set; }

	public float Y { get; set; }

	public float R { get; set; }

	public float G { get; set; }

	public float B { get; set; }
}
