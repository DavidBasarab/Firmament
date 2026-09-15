using System.Drawing;
using Firmament.Core;
using Firmament.Core.Extensions;
using Silk.NET.Input;

namespace Firmament.Asteroids2D;

public class LearningGame
{
	private const double ColorTransitionSeconds = 5.0;

	private readonly List<Color> colors =
	[
		Color.BlueViolet,
		Color.Crimson,
		Color.DarkOrange,
		Color.DeepSkyBlue,
		Color.ForestGreen,
		Color.Gold,
		Color.HotPink,
		Color.Indigo,
		Color.LimeGreen,
		Color.MediumOrchid,
	];

	private int colorIndex;
	private double colorTransitionProgress;
	private float[] currentClearColor;
	private bool pauseBackgroundSwitch;
	private float[] startColor;
	private float[] targetColor;
	private FirmamentWindow window;

	public void OnLoad()
	{
		window.SetClearColor(colors[0].ToArray());
	}

	public void OnRender(double delta)
	{
		if (currentClearColor is null)
		{
			InitializeColorTransition();
		}

		AdvanceColorTransition(delta);
	}

	public void OnUpdate(double delta) { }

	public void Run()
	{
		window = new FirmamentWindow(1280, 720, "Firmament");

		window.Load += OnLoad;
		window.Update += OnUpdate;
		window.Render += OnRender;
		window.CleanUp += OnCleanUp;
		window.KeyDown += OnKeyDown;
		window.GamepadButtonDown += OnGamepadButtonDown;

		window.Run();
	}

	private void AdvanceColorTransition(double delta)
	{
		if (pauseBackgroundSwitch)
		{
			return;
		}

		colorTransitionProgress += delta / ColorTransitionSeconds;

		while (colorTransitionProgress >= 1.0)
		{
			colorTransitionProgress -= 1.0;
			MoveToNextColor();
		}

		InterpolateClearColor();

		window.SetClearColor(currentClearColor);
	}

	private int GetNextColorIndex()
	{
		return (colorIndex + 1) % colors.Count;
	}

	private void InitializeColorTransition()
	{
		startColor = colors[colorIndex].ToArray();
		targetColor = colors[GetNextColorIndex()].ToArray();
		currentClearColor = [0f, 0f, 0f, 0f];
		colorTransitionProgress = 0.0;
	}

	private void InterpolateClearColor()
	{
		var t = (float)colorTransitionProgress;

		for (var channel = 0; channel < currentClearColor.Length; channel++)
		{
			currentClearColor[channel] = startColor[channel] + (targetColor[channel] - startColor[channel]) * t;
		}
	}

	private void MoveToNextColor()
	{
		colorIndex = GetNextColorIndex();
		startColor = targetColor;
		targetColor = colors[GetNextColorIndex()].ToArray();
	}

	private void OnCleanUp() { }

	private void OnGamepadButtonDown(IGamepad gamepad, Button button)
	{
		// button.RunAction(ButtonName.Start, () => Window.Close());
		// button.RunAction(ButtonName.A, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
	}

	private void OnKeyDown(IKeyboard source, Key key, int scancode)
	{
		key.RunAction(Key.Space, () => pauseBackgroundSwitch = !pauseBackgroundSwitch);
		key.RunAction(Key.Escape, () => window.Close());

		// key.RunAction(Key.V, CyclePresentSyncInterval);
	}
}
