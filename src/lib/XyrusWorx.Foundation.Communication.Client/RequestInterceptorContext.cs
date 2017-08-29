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
		public Uri RequestUri => mOwner.GetRequestUri();
		
		[NotNull]
		public IKeyValueStore<object> Parameters => mOwner.GetParameters();
		
		[NotNull]
		public IKeyValueStore<string> Headers => mOwner.GetHeaders();

		[CanBeNull]
		public string Body
		{
			get => mOwner.GetBodyString().NormalizeNull();
			set => mOwner.Body(value);
		}
	}
}