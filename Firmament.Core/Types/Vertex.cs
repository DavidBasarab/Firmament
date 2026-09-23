// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace Firmament.Core.Types;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex(float x, float y, float r, float g, float b)
{
	public float X = x;

	public float Y = y;

	public float R = r;

	public float G = g;

	public float B = b;
}
