using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public enum PrimitiveDataType
	{
		Undefined = 0,
		Text,
		Integer,
		Decimal,
		Boolean,
		Enumeration
	}
}