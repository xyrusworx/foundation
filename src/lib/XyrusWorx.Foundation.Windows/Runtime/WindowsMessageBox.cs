﻿using System;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using XyrusWorx.Runtime;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public class WindowsMessageBox : IMessageBox, IAsyncMessageBox
	{
		private readonly IApplicationHost mApplication;

		private string mTitle;
		private string mMessage;
		private MessageBoxImage mClass;
		private object mOwner;

		public WindowsMessageBox()
		{
		}
		public WindowsMessageBox([CanBeNull] IApplicationHost application) : this()
		{
			mApplication = application;
		}

		public IMessageBox Title(string title)
		{
			mTitle = title.NormalizeNull();
			return this;
		}
		public IMessageBox Message(string message)
		{
			mMessage = message.NormalizeNull();
			return this;
		}
		public IMessageBox Notice()
		{
			mClass = MessageBoxImage.Information;
			return this;
		}
		public IMessageBox Notice(string message)
		{
			mMessage = message.NormalizeNull();
			mClass = MessageBoxImage.Information;
			return this;
		}
		public IMessageBox Warning()
		{
			mClass = MessageBoxImage.Warning;
			return this;
		}
		public IMessageBox Warning(string message)
		{
			mMessage = message.NormalizeNull();
			mClass = MessageBoxImage.Warning;
			return this;
		}
		public IMessageBox Error()
		{
			mClass = MessageBoxImage.Error;
			return this;
		}
		public IMessageBox Error(string message)
		{
			mMessage = message.NormalizeNull();
			mClass = MessageBoxImage.Error;
			return this;
		}
		public IMessageBox Owner(object view)
		{
			mOwner = view;
			return this;
		}

		public void Display()
		{
			Display(MessageBoxButton.OK, mClass);
		}
		public bool Ask()
		{
			var result = Display(MessageBoxButton.YesNo, mClass != MessageBoxImage.None ? mClass : MessageBoxImage.Question);
			return result == MessageBoxResult.Yes;
		}
		public bool? AskOrCancel()
		{
			var result = Display(MessageBoxButton.YesNoCancel, mClass != MessageBoxImage.None ? mClass : MessageBoxImage.Question);
			if (result == MessageBoxResult.Cancel)
			{
				return null;
			}

			return result == MessageBoxResult.Yes;
		}

		IAsyncMessageBox IMessageBox.Async => this;

		async Task IAsyncMessageBox.Display()
		{
			await DisplayAsync(MessageBoxButton.OK, mClass);
		}
		async Task<bool> IAsyncMessageBox.Ask()
		{
			var result = await DisplayAsync(MessageBoxButton.YesNo, mClass != MessageBoxImage.None ? mClass : MessageBoxImage.Question);
			return result == MessageBoxResult.Yes;
		}
		async Task<bool?> IAsyncMessageBox.AskOrCancel()
		{
			var result = await DisplayAsync(MessageBoxButton.YesNoCancel, mClass != MessageBoxImage.None ? mClass : MessageBoxImage.Question);
			if (result == MessageBoxResult.Cancel)
			{
				return null;
			}

			return result == MessageBoxResult.Yes;
		}

		private Func<MessageBoxResult> Compile(MessageBoxButton button, MessageBoxImage image)
		{
			var fallbackTitle = "Information";
			switch (image)
			{
				case MessageBoxImage.Hand:
					fallbackTitle = "Error";
					break;
				case MessageBoxImage.Question:
					fallbackTitle = "Question";
					break;
				case MessageBoxImage.Exclamation:
					fallbackTitle = "Warning";
					break;
			}

			var function = new Func<MessageBoxResult>(() => MessageBox.Show((Window)mOwner, mMessage, mTitle.NormalizeNull() ?? ((Window)mOwner).Title.NormalizeNull() ?? fallbackTitle, button, image));

			if (!(mOwner is Window))
			{
				function = () => MessageBox.Show(mMessage, mTitle.NormalizeNull() ?? fallbackTitle, button, mClass);
			}

			if (string.IsNullOrEmpty(mMessage))
			{
				function = () => default(MessageBoxResult);
			}

			return function;
		}
		private MessageBoxResult Display(MessageBoxButton button, MessageBoxImage image)
		{
			var function = Compile(button, image);

			if (mApplication != null)
			{
				return mApplication.Execute(function);
			}

			return function();
		}
		private async Task<MessageBoxResult> DisplayAsync(MessageBoxButton button, MessageBoxImage image)
		{
			var function = Compile(button, image);

			if (mApplication != null)
			{
				return await mApplication.ExecuteAsync(function);
			}

			return await Task.Run(() => function());
		}
	}
}