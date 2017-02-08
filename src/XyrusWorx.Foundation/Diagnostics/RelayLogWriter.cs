using System;
using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public sealed class RelayLogWriter : LogWriter
	{
		private readonly ILogWriter mTarget;

		public RelayLogWriter([NotNull] ILogWriter target)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			mTarget = target;
		}

		protected override void DispatchOverride(LogMessage[] messages)
		{
			using (mTarget.MessageScope.Enter(MessageScope.State))
			{
				foreach (var message in messages)
				{
					mTarget.Write(message.Text, message.Class);
				}
			}
		}
	}
}