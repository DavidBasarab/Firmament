using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Firmament.Core;

public class FirmamentWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
	: GameWindow(gameWindowSettings, nativeWindowSettings)
{
	private const double ReportIntervalSeconds = 1.0;

	private double peakRenderSeconds;
	private int rendersSinceLastReport;
	private double secondsSinceLastReport;
	private int updatesSinceLastReport;

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		secondsSinceLastReport += args.Time;
		rendersSinceLastReport++;

		if (args.Time > peakRenderSeconds)
		{
			peakRenderSeconds = args.Time;
		}

		if (secondsSinceLastReport < ReportIntervalSeconds)
		{
			return;
		}

		ReportPerformance();
		ResetWindow();
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		updatesSinceLastReport++;
	}

	private void ReportPerformance()
	{
		var framesPerSecond = rendersSinceLastReport / secondsSinceLastReport;
		var avgMilliSeconds = secondsSinceLastReport / rendersSinceLastReport * 1000.0;
		var peakMilliseconds = peakRenderSeconds * 1000.0;

		Title =
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliSeconds} ms avg | {peakMilliseconds} ms peak | {updatesSinceLastReport} updates/s";
	}

	private void ResetWindow()
	{
		secondsSinceLastReport = 0.0;
		updatesSinceLastReport = 0;
		rendersSinceLastReport = 0;
		peakRenderSeconds = 0.0;
	}
}
