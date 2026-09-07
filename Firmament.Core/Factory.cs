using FatCat.Toolkit.Injection;

namespace Firmament.Core;

public static class Factory
{
	public static T Get<T>()
		where T : class
	{
		return SystemScope.Container.Resolve<T>();
	}
}
