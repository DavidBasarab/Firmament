using OpenTK.Windowing.Desktop;

namespace Firmament.Core;

public class FirmamentWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
	: GameWindow(gameWindowSettings, nativeWindowSettings) { }
