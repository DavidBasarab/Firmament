using FatCat.Toolkit.Caching;
using FatCat.Toolkit.Data;

namespace Firmament.Core.Shaders;

public class ShaderCacheItem(string fullName, string source) : ICacheItem
{
	public string CacheId => FullName;

	public string FullName { get; set; } = fullName;

	public string Source { get; set; } = source;
}

public interface IShaderLoader
{
	string GetShaderSource(string shaderName);
}

public class ShaderLoader(IEmbeddedResourceRepository repository, IFatCatCache<ShaderCacheItem> cache) : IShaderLoader
{
	public string GetShaderSource(string shaderName)
	{
		var fullName = GetShaderFullName(shaderName);

		if (cache.InCache(fullName))
		{
			return cache.Get(fullName).Source;
		}

		var source = AddToCache(fullName);

		return source;
	}

	private string AddToCache(string fullName)
	{
		var source = repository.GetText(ShaderMarker.Marker.Assembly, fullName);

		cache.Add(new ShaderCacheItem(fullName, source));
		return source;
	}

	private static string GetShaderFullName(string shaderName)
	{
		var fullName = $"{ShaderMarker.Marker.Namespace}.{shaderName}";

		if (!shaderName.EndsWith(".hlsl"))
		{
			fullName += ".hlsl";
		}

		return fullName;
	}
}
