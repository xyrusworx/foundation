using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI, DataContract]
	public class ViewModel : INotifyPropertyChanged
	{
		[IgnoreDataMember]
		private IScope mNotificationSupressionScope;

		public ViewModel()
		{
			Setup();
		}

		[field: IgnoreDataMember, NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		[NotNull]
		public IScope EnterSupressNotificationScope()
		{
			if (mNotificationSupressionScope == null)
			{
				Setup();
			}
			
			return mNotificationSupressionScope.Enter();
		}
		public void NotifyChange(string property = null)
		{
			OnPropertyChanged(property ?? string.Empty);
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (mNotificationSupressionScope == null)
			{
				Setup();
			}
			
			if (mNotificationSupressionScope.IsInScope)
			{
				return;
			}

			void Notify()
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
				GlobalPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			if (PropertyChangedDispatcher != null)
			{
				PropertyChangedDispatcher.Invoke(Notify);
			}
			else
			{
				Notify();
			}
		}

		public static event PropertyChangedEventHandler GlobalPropertyChanged;

		[IgnoreDataMember]
		protected virtual Dispatcher PropertyChangedDispatcher => null;
		
		private void Setup()
		{
			mNotificationSupressionScope = new Scope(() => { }, () => NotifyChange(string.Empty));
		}
	}

	[PublicAPI, DataContract]
	public class ViewModel<T> : ViewModel
	{
		private T mModel;

		public virtual T Model
		{
			get { return mModel; }
			set
			{
				if (Equals(value, mModel)) return;
				mModel = value;
				OnPropertyChanged(string.Empty);
				OnModelChanged(value);
			}
		}

		protected virtual void OnModelChanged(T model){}
	}
}
