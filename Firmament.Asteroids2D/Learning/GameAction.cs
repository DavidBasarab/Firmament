using Firmament.Core;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Firmament.Asteroids2D.Learning;

public abstract class GameAction(LearningGame game)
{
	protected ComPtr<ID3D11DeviceContext> DeviceContext => Window.DeviceContext;

	protected FirmamentWindow Window { get; } = game.Window;

	protected LearningGame Game { get; } = game;
}
