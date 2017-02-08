using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public class EventToCommand : TriggerAction<DependencyObject>
	{
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
			nameof(CommandParameter),
			typeof(object),
			typeof(EventToCommand),
			new PropertyMetadata(
				null,
				OnCommandParameterPropertyChanged));

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command),
			typeof(ICommand),
			typeof(EventToCommand),
			new PropertyMetadata(
				null,
				OnCommandPropertyChanged));

		public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register(
			nameof(MustToggleIsEnabled),
			typeof(bool),
			typeof(EventToCommand),
			new PropertyMetadata(
				false,
				OnMustToogleIsEnabledPropertyChanged));

		private object mCommandParameterValue;
		private bool? mMustToggleValue;

		public ICommand Command
		{
			get
			{
				return (ICommand)GetValue(CommandProperty);
			}

			set
			{
				SetValue(CommandProperty, value);
			}
		}

		public object CommandParameter
		{
			get
			{
				return GetValue(CommandParameterProperty);
			}

			set
			{
				SetValue(CommandParameterProperty, value);
			}
		}
		public object CommandParameterValue
		{
			get
			{
				return mCommandParameterValue ?? CommandParameter;
			}

			set
			{
				mCommandParameterValue = value;
				EnableDisableElement();
			}
		}

		public bool MustToggleIsEnabled
		{
			get
			{
				return (bool)GetValue(MustToggleIsEnabledProperty);
			}

			set
			{
				SetValue(MustToggleIsEnabledProperty, value);
			}
		}
		public bool MustToggleIsEnabledValue
		{
			get
			{
				return mMustToggleValue ?? MustToggleIsEnabled;
			}
			set
			{
				mMustToggleValue = value;
				EnableDisableElement();
			}
		}

		public bool PassEventArgsToCommand
		{
			get;
			set;
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			EnableDisableElement();
		}
		protected override void Invoke(object parameter)
		{
			if (AssociatedElementIsDisabled())
			{
				return;
			}

			var command = GetCommand();
			var commandParameter = CommandParameterValue;

			if (commandParameter == null
			    && PassEventArgsToCommand)
			{
				commandParameter = parameter;
			}

			if (command != null
			    && command.CanExecute(commandParameter))
			{
				command.Execute(commandParameter);
			}
		}

		private FrameworkElement GetAssociatedObject()
		{
			return AssociatedObject as FrameworkElement;
		}
		private ICommand GetCommand()
		{
			return Command;
		}

		private bool AssociatedElementIsDisabled()
		{
			var element = GetAssociatedObject();

			return AssociatedObject == null
			       || (element != null
			           && !element.IsEnabled);
		}
		private void EnableDisableElement()
		{
			var element = GetAssociatedObject();

			if (element == null)
			{
				return;
			}

			var command = GetCommand();

			if (MustToggleIsEnabledValue
			    && command != null)
			{
				element.IsEnabled = command.CanExecute(CommandParameterValue);
			}
		}

		private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
		{
			if (element == null)
			{
				return;
			}

			if (e.OldValue != null)
			{
				((ICommand)e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
			}

			var command = (ICommand)e.NewValue;

			if (command != null)
			{
				command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
			}

			element.EnableDisableElement();
		}
		private void OnCommandCanExecuteChanged(object sender, EventArgs e)
		{
			EnableDisableElement();
		}

		private static void OnCommandPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			OnCommandChanged(s as EventToCommand, e);
		}
		private static void OnCommandParameterPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			var sender = s as EventToCommand;
			if (sender?.AssociatedObject == null)
			{
				return;
			}

			sender.EnableDisableElement();
		}
		private static void OnMustToogleIsEnabledPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			var sender = s as EventToCommand;
			if (sender?.AssociatedObject == null)
			{
				return;
			}

			sender.EnableDisableElement();
		}
	}
}