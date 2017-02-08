using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class Scope : IScope
	{
		private readonly Stack<object> mStateStack = new Stack<object>();
		private readonly object mLock = new object();

		private readonly Action mOnEnter;
		private readonly Action mOnLeave;

		public Scope() { }
		public Scope(Action onEnter, Action onLeave)
		{
			mOnEnter = onEnter;
			mOnLeave = onLeave;
		}

		protected virtual void EnteringOverride() { }
		protected virtual void EnteredOverride() { }

		protected virtual void LeavingOverride() { }
		protected virtual void LeftOverride() { }

		internal event EventHandler Entering;
		internal event EventHandler Entered;

		internal event EventHandler Leaving;
		internal event EventHandler Left;

		[NotNull] public IScope Enter(object state = null)
		{
			lock (mLock)
			{
				if (EnterTrigger == ScopeEnterTrigger.Entering)
				{
					mOnEnter?.Invoke();
				}

				EnteringOverride();
				Entering?.Invoke(this, new EventArgs());

				mStateStack.Push(state);

				Entered?.Invoke(this, new EventArgs());
				EnteredOverride();

				if (EnterTrigger == ScopeEnterTrigger.Entered)
				{
					mOnEnter?.Invoke();
				}
			}
			
			return this;
		}
		[NotNull] public IScope Leave()
		{
			lock (mLock)
			{
				if (mStateStack.Count <= 0)
				{
					return this;
				}

				if (LeaveTrigger == ScopeLeaveTrigger.Leaving)
				{
					mOnLeave?.Invoke();
				}

				LeavingOverride();
				Leaving?.Invoke(this, new EventArgs());

				mStateStack.Pop();

				Left?.Invoke(this, new EventArgs());
				LeftOverride();

				if (LeaveTrigger == ScopeLeaveTrigger.Left)
				{
					mOnLeave?.Invoke();
				}

				return this;
			}
		}

		void IDisposable.Dispose()
		{
			Leave();
		}

		public object State
		{
			get
			{
				lock (mLock)
				{
					return IsInScope ? mStateStack.Peek() : null;
				}
			}
		}
		public bool IsInScope
		{
			get
			{
				lock (mLock)
				{
					return mStateStack.Count > 0;
				}
			}
		}

		public ScopeEnterTrigger EnterTrigger { get; set; } = ScopeEnterTrigger.Entered;
		public ScopeLeaveTrigger LeaveTrigger { get; set; } = ScopeLeaveTrigger.Leaving;

		public IEnumerable Stack
		{
			get
			{
				lock (mLock)
				{
					return mStateStack.ToArray();
				}
			}
		}
	}
}