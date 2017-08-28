using System;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Communication.Client 
{
	[PublicAPI]
	public class RequestInterceptorContext
	{
		private readonly RequestBuilder mOwner;

		internal RequestInterceptorContext([NotNull] RequestBuilder owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}
			
			mOwner = owner;
		}

		[NotNull]
		public string Verb => mOwner.GetVerb();
		
		[NotNull]
		public string Path => mOwner.GetRequestPath();
		
		[NotNull]
		public IKeyValueStore<object> Parameters => mOwner.GetParameters();
		
		[NotNull]
		public IKeyValueStore<string> Headers => mOwner.GetHeaders();

		[CanBeNull]
		public string GetBodyString() => mOwner.GetBodyString().NormalizeNull();
	}
}