using System;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI, AttributeUsage(AttributeTargets.Parameter)]
	public class FromQueryAttribute : Attribute
	{

	}
}