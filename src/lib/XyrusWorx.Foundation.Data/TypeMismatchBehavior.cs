using JetBrains.Annotations;

namespace XyrusWorx.Data 
{
	[PublicAPI]
	public enum TypeMismatchBehavior
	{
		Throw,
		Convert,
		ResultNull
	}
}