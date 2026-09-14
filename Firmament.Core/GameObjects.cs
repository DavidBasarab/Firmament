using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Firmament.Core;

public static class GameObjects
{
	public static IWindow Window { get; private set; }

	public static void CreateWindow(int width, int height, string title)
	{
		var options = WindowOptions.Default with
		{
			API = GraphicsAPI.None,
			Size = new Vector2D<int>(width, height),
			Title = title,
		};

		Window = Silk.NET.Windowing.Window.Create(options);
	}

	public static void Dispose()
	{
		Window.Dispose();
	}

	public static D3D11 GetD3D11()
	{
		return D3D11.GetApi(Window);
	}

	public static DXGI GetDxgi()
	{
		return DXGI.GetApi(Window);
	}
}
