﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI, DataContract]
	public class ViewModel : INotifyPropertyChanged
	{
		[IgnoreDataMember]
		private readonly IScope mNotificationSupressionScope;

		public ViewModel()
		{
			mNotificationSupressionScope = new Scope(() => { }, () => NotifyChange(string.Empty));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotNull]
		public IScope EnterSupressNotificationScope()
		{
			return mNotificationSupressionScope.Enter();
		}
		public void NotifyChange(string property = null)
		{
			OnPropertyChanged(property ?? string.Empty);
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (mNotificationSupressionScope.IsInScope)
			{
				return;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			GlobalPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static event PropertyChangedEventHandler GlobalPropertyChanged;
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
			}
		}
	}
}