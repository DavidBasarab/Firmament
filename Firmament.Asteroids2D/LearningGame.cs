using System.Drawing;
using Firmament.Core;
using Firmament.Core.Extensions;

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

		window.Run();
	}

	private void AdvanceColorTransition(double delta)
	{
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
}
