using System;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime 
{
	[PublicAPI]
	public class ServiceInterfaceAttribute : Attribute
	{
		public ServiceInterfaceAttribute([CanBeNull] Type interfaceType)
		{
			InterfaceType = interfaceType;
		}
		
		[CanBeNull]
		public Type InterfaceType { get; }
	}
}