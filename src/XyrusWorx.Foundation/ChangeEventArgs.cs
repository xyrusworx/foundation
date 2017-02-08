using System;
using JetBrains.Annotations;

namespace XyrusWorx
{
	[PublicAPI]
	public class ChangeEventArgs<T> : EventArgs
	{
		public ChangeEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public T OldValue { get; }
		public T NewValue { get; }
	}
}