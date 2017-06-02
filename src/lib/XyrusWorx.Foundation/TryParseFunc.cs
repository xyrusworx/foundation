using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public delegate bool TryParseFunc<T>(string data, out T value);
}