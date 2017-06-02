using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx.Threading
{
	[PublicAPI]
	public abstract class MessageDispatcher<T> : Resource
	{
		private readonly DelayQueue<T> mQueue;
		internal readonly object DispatchLock = new object();

		protected MessageDispatcher()
		{
			MessageScope = new Scope(OnScopeEntered, OnScopeLeaving);
			LinkedDispatchers = new LinkedMessageDispatcherCollection<T>(this);

			mQueue = new DelayQueue<T>(TimeSpan.FromSeconds(1));
			mQueue.Callback = Send;

			MessageScope.EnterTrigger = ScopeEnterTrigger.Entered;
			MessageScope.LeaveTrigger = ScopeLeaveTrigger.Left;

			LinkedDispatchers.InsertAction = OnAttach;
			LinkedDispatchers.RemoveAction = OnDetach;
		}

		public bool IsFlushDelayed
		{
			get { return mQueue.IsDelayEnabled; }
			set { mQueue.IsDelayEnabled = value; }
		}
		public TimeSpan FlushInterval
		{
			get { return mQueue.FlushInterval; }
			set { mQueue.FlushInterval = value; }
		}

		public LinkedMessageDispatcherCollection<T> LinkedDispatchers { get; }
		public IScope MessageScope { get; }

		[CanBeNull]
		internal MessageDispatcher<T> Parent { get; set; }

		public void Flush() => mQueue.Flush();

		protected void Dispatch([NotNull] T message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			mQueue.Enqueue(message);
		}
		protected void Dispatch([NotNull] IEnumerable<T> messages)
		{
			if (messages == null) throw new ArgumentNullException(nameof(messages));
			mQueue.Enqueue(messages);
		}

		protected abstract void DispatchOverride([NotNull] T[] messages);
		protected virtual bool FilterOverride(ref T message) => true;

		protected virtual void ScopeEnteredOverride(object scope)
		{

		}
		protected virtual void ScopeLeftOverride(object scope)
		{

		}

		protected virtual void LinkEstablishedOverride(MessageDispatcher<T> dispatcher)
		{

		}
		protected virtual void LinkRemovedOverride(MessageDispatcher<T> dispatcher)
		{

		}

		protected sealed override void DisposeOverride()
		{
			try
			{
				CleanupDispatcherOverride();
			}
			finally
			{
				mQueue.Dispose();
			}
		}
		protected virtual void CleanupDispatcherOverride() { }

		private void OnScopeEntered()
		{
			foreach (var log in LinkedDispatchers)
			{
				log.MessageScope.Enter(MessageScope.State);
			}

			try
			{
				ScopeEnteredOverride(MessageScope.State);
			}
			catch
			{
				// ignored
			}
		}
		private void OnScopeLeaving()
		{
			foreach (var log in LinkedDispatchers)
			{
				log.MessageScope.Leave();
			}

			try
			{
				ScopeLeftOverride(MessageScope.State);
			}
			catch
			{
				// ignored
			}
		}

		private void OnAttach(MessageDispatcher<T> item)
		{
			try
			{
				LinkEstablishedOverride(item);
			}
			catch
			{
				// ignored
			}
		}
		private void OnDetach(MessageDispatcher<T> item)
		{
			try
			{
				LinkRemovedOverride(item);
			}
			catch
			{
				// ignored
			}
		}

		private void Send(IEnumerable<T> messages)
		{
			lock (DispatchLock)
			{
				var messageList = new List<T>();

				foreach (var message in messages)
				{
					var messageCopy = message;
					if (!FilterOverride(ref messageCopy))
					{
						continue;
					}

					messageList.Add(messageCopy);
				}

				var messageArray = messageList.ToArray();

				DispatchOverride(messageArray);

				foreach (var dispatcher in LinkedDispatchers)
				{
					dispatcher.DispatchOverride(messageArray);
				}
			}
		}
	}
}