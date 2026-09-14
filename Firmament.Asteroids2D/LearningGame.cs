using Firmament.Core;

namespace Firmament.Asteroids2D;

public class LearningGame
{
	public void Run()
	{
		using var window = new FirmamentWindow(1280, 720, "Firmament");

		window.Run();
	}
}
