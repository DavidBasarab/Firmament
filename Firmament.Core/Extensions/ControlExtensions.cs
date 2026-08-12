using Silk.NET.Input;

namespace Firmament.Core.Extensions;

public static class ControlExtensions
{
	public static void RunAction(this Button button, ButtonName expectName, Action action)
	{
		if (button.Name == expectName)
		{
			action();
		}
	}

	public static void RunAction(this Key key, Key expectedKey, Action action)
	{
		if (key == expectedKey)
		{
			action();
		}
	}
}
