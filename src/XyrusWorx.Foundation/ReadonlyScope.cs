using System;
using System.Collections;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class ReadonlyScope : IReadonlyScope
	{
		private readonly Scope mScope;

		private readonly Action mOnEnter;
		private readonly Action mOnLeave;

		public ReadonlyScope([NotNull] Scope scope)
		{
			if (scope == null)
			{
				throw new ArgumentNullException(nameof(scope));
			}

			mScope = scope;

			mScope.Entering += OnEntering;
			mScope.Entered += OnEntered;

			mScope.Leaving += OnLeaving;
			mScope.Left += OnLeft;
		}
		public ReadonlyScope([NotNull] Scope scope, Action onEnter, Action onLeave)
		{
			mOnEnter = onEnter;
			mOnLeave = onLeave;
		}

		public ScopeEnterTrigger EnterTrigger { get; set; } = ScopeEnterTrigger.Entered;
		public ScopeLeaveTrigger LeaveTrigger { get; set; } = ScopeLeaveTrigger.Leaving;

		protected virtual void EnteringOverride() { }
		protected virtual void EnteredOverride() { }

		protected virtual void LeavingOverride() { }
		protected virtual void LeftOverride() { }
		
		public object State => mScope.State;
		public bool IsInScope => mScope.IsInScope;
		public IEnumerable Stack => mScope.Stack;

		private void OnEntering(object sender, EventArgs e)
		{
			if (EnterTrigger == ScopeEnterTrigger.Entering)
			{
				mOnEnter?.Invoke();
			}

			EnteringOverride();
		}
		private void OnEntered(object sender, EventArgs e)
		{
			EnteredOverride();

			if (EnterTrigger == ScopeEnterTrigger.Entered)
			{
				mOnEnter?.Invoke();
			}
		}

		private void OnLeaving(object sender, EventArgs e)
		{
			if (LeaveTrigger == ScopeLeaveTrigger.Leaving)
			{
				mOnLeave?.Invoke();
			}

			LeavingOverride();
		}
		private void OnLeft(object sender, EventArgs e)
		{
			LeftOverride();

			if (LeaveTrigger == ScopeLeaveTrigger.Left)
			{
				mOnLeave?.Invoke();
			}
		}
	}
}