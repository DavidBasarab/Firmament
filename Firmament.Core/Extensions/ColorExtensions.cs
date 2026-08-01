using System.Drawing;

namespace Firmament.Core.Extensions;

public static class ColorExtensions
{
	public static float[] ToArray(this Color color)
	{
		return [color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f];
	}
}
