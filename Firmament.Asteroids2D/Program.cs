using Firmament.Core;

namespace Firmament.Asteroids2D;

public static class Program
{
	public static void Main()
	{
		using var window = new FirmamentWindow(1280, 720, "Firmament");

		window.Run();
	}
}
