using Firmament.Core;

namespace Firmament.Asteroids2D.Learning;

public class TitleReporter(LearningGame game)
{
	private FirmamentWindow Window { get; } = game.Window;

	private const double ReportIntervalSeconds = 0.1;

	private double peakRenderSeconds;
	private double rendersSinceLastReport;

	private double secondsSinceLastReport;

	private int updatesSinceLastReport;

	public void Render(double delta)
	{
		secondsSinceLastReport += delta;
		rendersSinceLastReport++;

		if (delta > peakRenderSeconds)
		{
			peakRenderSeconds = delta;
		}

		ReportPerformance();
	}

	public void Update(double delta)
	{
		updatesSinceLastReport++;
	}

	private void ReportPerformance()
	{
		if (secondsSinceLastReport < ReportIntervalSeconds)
		{
			return;
		}

		var framesPerSecond = rendersSinceLastReport / secondsSinceLastReport;
		var avgMilliseconds = secondsSinceLastReport / rendersSinceLastReport * 1000.0;
		var peakMilliseconds = peakRenderSeconds * 1000.0;
		var aspectRatio = (float)Window.BackBufferWidth / Window.BackBufferHeight;

		var title =
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliseconds:F2} ms avg | {peakMilliseconds:F2} ms peak | {updatesSinceLastReport} updates | Background Paused {game.ColorShifter.BackgroundPause} | {Window.ResizeCount} resizes | {Window.BackBufferWidth}x{Window.BackBufferHeight} @ {aspectRatio:F2}:1 | {DescribePresentMode()}";

		Window.SetWindowTitle(title);

		ResetWindow();
	}

	private void ResetWindow()
	{
		secondsSinceLastReport = 0.0;
		updatesSinceLastReport = 0;
		rendersSinceLastReport = 0;
		peakRenderSeconds = 0.0;
	}

	private string DescribePresentMode()
	{
		if (Window.PresentSyncInterval > 0)
		{
			return $"vsync {Window.PresentSyncInterval}";
		}

		if (Window.AllowTearingSupported)
		{
			return "tearing";
		}

		return "no-sync";
	}
}
