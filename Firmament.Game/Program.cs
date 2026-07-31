using Firmament.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Firmament.Game;

public static class Program
{
	public static void Main()
	{
		var nativeWindowSettings = new NativeWindowSettings
		{
			ClientSize = new Vector2i(1280, 720),
			Title = "Firmament",
			API = ContextAPI.OpenGL,
			APIVersion = new Version(4, 6),
			Profile = ContextProfile.Core,
			Flags = ContextFlags.ForwardCompatible,
		};

		using var window = new FirmamentWindow(new GameWindowSettings(), nativeWindowSettings);

		window.Run();
	}
}
