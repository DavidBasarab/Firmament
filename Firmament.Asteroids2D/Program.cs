using Autofac;
using FatCat.Toolkit.Injection;
using Firmament.Asteroids2D.Learning;
using Firmament.Core;

namespace Firmament.Asteroids2D;

public static class Program
{
	public static void Main()
	{
		SystemScope.Initialize(new ContainerBuilder(), ScopeOptions.SetLifetimeScope);

		var game = Factory.Get<LearningGame>();

		game.Run();
	}
}
