using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public delegate void ChangeEventHandler<T>(object sender, ChangeEventArgs<T> args);
}