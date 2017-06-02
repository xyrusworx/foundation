using System;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI, AttributeUsage(AttributeTargets.Method)]
	public class RequireAuthenticationAttribute : Attribute
	{
		private readonly Type mProviderType;

		public RequireAuthenticationAttribute() { }
		public RequireAuthenticationAttribute(Type providerType)
		{
			mProviderType = providerType;
		}

		public Type ProviderType => mProviderType;
	}
}