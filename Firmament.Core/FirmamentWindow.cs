using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Firmament.Core;

public class FirmamentWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
	: GameWindow(gameWindowSettings, nativeWindowSettings)
{
	private DateTime lastUpdateTime;
	private int rendersSinceLastReport;
	private TimeSpan runTime;
	private double secondsSinceLastReport;
	private TimeSpan updateAverage;
	private TimeSpan updateDelta;
	private int updatesSinceLastReport;

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);

		GetUpdateData(args);
		PrintUpdates();
	}

	private void GetUpdateData(FrameEventArgs args)
	{
		secondsSinceLastReport += args.Time;
		runTime += TimeSpan.FromSeconds(args.Time);
		updatesSinceLastReport++;
		updateDelta = DateTime.Now - lastUpdateTime;
		lastUpdateTime = DateTime.Now;

		updateAverage = (updateAverage * (updatesSinceLastReport - 1) + updateDelta) / updatesSinceLastReport;
	}

	private void PrintUpdates()
	{
		if (secondsSinceLastReport < 1.0)
		{
			return;
		}

		Console.WriteLine(
			$"Updates per second: {updatesSinceLastReport}, Renders per second: {rendersSinceLastReport}, Run time: {runTime} | Update delta: {updateDelta}, Update average: {updateAverage}"
		);

		secondsSinceLastReport = 0.0;
		updatesSinceLastReport = 0;
		rendersSinceLastReport = 0;
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		rendersSinceLastReport++;
	}
}
