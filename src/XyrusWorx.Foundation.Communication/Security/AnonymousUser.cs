using System;
using JetBrains.Annotations;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public class AnonymousUser : IAuthenticatedUser
	{
		internal AnonymousUser([NotNull] WebService context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			Context = context;
		}

		public string Name => null;
		public WebService Context { get; }
	}
}