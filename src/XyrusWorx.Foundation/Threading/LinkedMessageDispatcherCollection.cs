using System;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public sealed class LinkedMessageDispatcherCollection<T> : LinkableList<MessageDispatcher<T>>
	{
		private readonly MessageDispatcher<T> mOwner;

		public LinkedMessageDispatcherCollection([NotNull] MessageDispatcher<T> owner) : base(owner.DispatchLock)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			mOwner = owner;
		}
		protected override void HandleInsertOverride(MessageDispatcher<T> item)
		{
			if (mOwner.MessageScope.IsInScope && mOwner.MessageScope.State != null)
			{
				item.MessageScope.Enter(mOwner.MessageScope.State);
			}

			item.Parent = mOwner;
		}
		protected override void HandleRemoveOverride(MessageDispatcher<T> item)
		{
			item.Parent = null;
		}
	}
}