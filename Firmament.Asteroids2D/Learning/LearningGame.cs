using Firmament.Core;
using Firmament.Core.Extensions;
using Silk.NET.Input;

namespace Firmament.Asteroids2D.Learning;

public class LearningGame
{
	private TitleReporter titleReporter;
	private TriangleDrawer triangleDrawer;

	public ColorShifter ColorShifter { get; private set; }

	public FirmamentWindow Window { get; private set; }

	public void OnLoad()
	{
		ColorShifter = new ColorShifter(this);

		ColorShifter.SetInitialClearColor();

		titleReporter = new TitleReporter(this);

		triangleDrawer = new TriangleDrawer(this);

		triangleDrawer.Load();
	}

	public void OnRender(double delta)
	{
		ColorShifter.Render(delta);
		titleReporter.Render(delta);
		triangleDrawer.Render(delta);
	}

	public void OnUpdate(double delta)
	{
		titleReporter.Update(delta);
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

	private void OnCleanUp()
	{
		triangleDrawer.CleanUp();
	}

	private void OnGamepadButtonDown(IGamepad gamepad, Button button)
	{
		button.RunAction(ButtonName.Start, Window.Close);
		button.RunAction(ButtonName.A, ColorShifter.ToggleBackgroundPause);
		button.RunAction(ButtonName.B, CyclePresentSyncInterval);
	}

	private void OnKeyDown(IKeyboard source, Key key, int scancode)
	{
		key.RunAction(Key.Space, ColorShifter.ToggleBackgroundPause);
		key.RunAction(Key.Escape, () => Window.Close());
		key.RunAction(Key.V, CyclePresentSyncInterval);
	}
}
