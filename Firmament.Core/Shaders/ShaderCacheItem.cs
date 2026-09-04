using FatCat.Toolkit.Caching;

namespace Firmament.Core.Shaders;

public class ShaderCacheItem(string fullName, string source) : ICacheItem
{
	public string CacheId => FullName;

	public string FullName { get; set; } = fullName;

	public string Source { get; set; } = source;
}
