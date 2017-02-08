using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public sealed class AsyncRelayCommand : RelayCommand<object>
	{
		public AsyncRelayCommand([NotNull] Func<Task> executeMethod, [CanBeNull] Func<bool> canExecuteMethod = null)
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			var execute = executeMethod == null ? (Action<object>) null : async o => await executeMethod();
			var canExecute = canExecuteMethod == null ? (Func<object, bool>) null : o => canExecuteMethod();

			Initialize(execute, canExecute);
		}

		public void Execute()
		{
			Execute(null);
		}
		public bool CanExecute()
		{
			return CanExecute(null);
		}
	}

	[PublicAPI]
	public sealed class AsyncRelayCommand<T> : RelayCommand<T>
	{
		public AsyncRelayCommand([NotNull] Func<T, Task> executeMethod, [CanBeNull] Func<T, bool> canExecuteMethod = null)
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			var execute = executeMethod == null ? (Action<T>)null : async o => await executeMethod(o);
			var canExecute = canExecuteMethod;

			Initialize(execute, canExecute);
		}
	}
}