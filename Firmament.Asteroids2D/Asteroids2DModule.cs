using Autofac;
using FatCat.Toolkit.Console;
using JetBrains.Annotations;

namespace Firmament.Asteroids2D;

[UsedImplicitly]
public class Asteroids2DModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		ConsoleLog.WriteCyan("Loading Asteroids2D module...");
	}
}
