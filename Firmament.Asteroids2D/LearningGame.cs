using Firmament.Core;
using Firmament.Core.Extensions;
using Silk.NET.Input;

namespace Firmament.Asteroids2D;

public class LearningGame
{
	private const double ReportIntervalSeconds = 0.1;
	private ColorShifter colorShifter;

	private double peakRenderSeconds;
	private double rendersSinceLastReport;

	private double secondsSinceLastReport;

	private int updatesSinceLastReport;

	public FirmamentWindow Window { get; private set; }

	public void OnLoad()
	{
		colorShifter = new ColorShifter(Window);

		colorShifter.SetClearColor();
	}

	public void OnRender(double delta)
	{
		colorShifter.Render(delta);

		secondsSinceLastReport += delta;
		rendersSinceLastReport++;

		if (delta > peakRenderSeconds)
		{
			peakRenderSeconds = delta;
		}

		ReportPerformance();
	}

	public void OnUpdate(double delta)
	{
		updatesSinceLastReport++;
	}

	public void Run()
	{
		Window = new FirmamentWindow(1280, 720, "Firmament");

		Window.Load += OnLoad;
		Window.Update += OnUpdate;
		Window.Render += OnRender;
		Window.CleanUp += OnCleanUp;
		Window.KeyDown += OnKeyDown;
		Window.GamepadButtonDown += OnGamepadButtonDown;

		Window.Run();
	}

	private void CyclePresentSyncInterval()
	{
		Window.PresentSyncInterval = Window.PresentSyncInterval switch
		{
			0 => 1,
			1 => 2,
			_ => 0,
		};
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

	private void OnCleanUp() { }

	private void OnGamepadButtonDown(IGamepad gamepad, Button button)
	{
		button.RunAction(ButtonName.Start, Window.Close);
		button.RunAction(ButtonName.A, colorShifter.ToggleBackgroundPause);
		button.RunAction(ButtonName.B, CyclePresentSyncInterval);
	}

	private void OnKeyDown(IKeyboard source, Key key, int scancode)
	{
		key.RunAction(Key.Space, colorShifter.ToggleBackgroundPause);
		key.RunAction(Key.Escape, () => Window.Close());
		key.RunAction(Key.V, CyclePresentSyncInterval);
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
			$"Firmament - {framesPerSecond:F0} FPS | {avgMilliseconds:F2} ms avg | {peakMilliseconds:F2} ms peak | {updatesSinceLastReport} updates | Background Paused {colorShifter.BackgroundPause} | {Window.ResizeCount} resizes | {Window.BackBufferWidth}x{Window.BackBufferHeight} @ {aspectRatio:F2}:1 | {DescribePresentMode()}";

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
}
