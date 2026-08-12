using Silk.NET.Input;

namespace Firmament.Core.Extensions;

public static class ControlExtensions
{
	public static void RunActionForKeyDown(this Key key, Key expectedKey, Action action)
	{
		if (key == expectedKey)
		{
			action();
		}
	}
}
