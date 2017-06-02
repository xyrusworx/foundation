using System;
using JetBrains.Annotations;
using XyrusWorx.Communication.Provider;

namespace XyrusWorx.Communication.Security
{
	[PublicAPI]
	public class BasicUser : IAuthenticatedUser
	{
		public BasicUser([NotNull] WebService context, string username, string password)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			Context = context;
			Name = username;
			Password = password;
		}

		public WebService Context { get; }
		public string Name { get; }
		public string Password { get; }
	}
}