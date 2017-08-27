using JetBrains.Annotations;

namespace XyrusWorx.Data 
{
	[PublicAPI]
	public enum FieldNotFoundBehavior
	{
		Throw,
		ResultNull
	}
}