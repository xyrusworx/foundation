using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public sealed class RelayCommand : RelayCommand<object>
	{
		public RelayCommand([NotNull] Action executeMethod, [CanBeNull] Func<bool> canExecuteMethod = null)
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			var execute = executeMethod == null ? (Action<object>) null : o => executeMethod();
			var canExecute = canExecuteMethod == null ? (Func<object, bool>) null : o => canExecuteMethod();

			Initialize(execute, canExecute);
		}

		public static ICommand WrapKeyPress([NotNull] ICommand target, params Key[] keys) => new RelayCommand<KeyEventArgs>(
			a => target.Execute(null),
			a => keys.Any(x => x == a.Key) && target.CanExecute(null));

		public static ICommand CreateDefault() => new RelayCommand(() => {}, () => false);

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
	public class RelayCommand<T> : ICommand
	{
		private Action<T> mExecuteMethod;
		private Func<T, bool> mCanExecuteMethod;

		protected RelayCommand()
		{
		}
		public RelayCommand([NotNull] Action<T> executeMethod, [CanBeNull] Func<T, bool> canExecuteMethod = null)
		{
			Initialize(executeMethod, canExecuteMethod);
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute(T parameter = default(T))
		{
			if (!CanExecute(parameter))
			{
				return;
			}

			mExecuteMethod(parameter);
		}
		public bool CanExecute(T parameter = default(T))
		{
			return mCanExecuteMethod == null || mCanExecuteMethod(parameter);
		}

		protected void Initialize([NotNull] Action<T> executeMethod, [CanBeNull] Func<T, bool> canExecuteMethod = null)
		{
			if (executeMethod == null)
			{
				throw new ArgumentNullException(nameof(executeMethod));
			}

			mExecuteMethod = executeMethod;
			mCanExecuteMethod = canExecuteMethod;
		}

		void ICommand.Execute(object parameter)
		{
			if (!(this as ICommand).CanExecute(parameter))
			{
				return;
			}

			mExecuteMethod.Invoke(parameter is T ? (T)parameter : default(T));
		}
		bool ICommand.CanExecute(object parameter)
		{
			return mCanExecuteMethod == null || mCanExecuteMethod(parameter is T ? (T)parameter : default(T));
		}
	}
}
