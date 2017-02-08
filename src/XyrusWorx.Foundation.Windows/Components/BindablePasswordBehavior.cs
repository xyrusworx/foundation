using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class BindablePasswordBehavior : Behavior<PasswordBox>
	{
		private Scope mUpdateScope = new Scope();

		public string Password
		{
			get { return (string)GetValue(PasswordProperty); }
			set { SetValue(PasswordProperty, value); }
		}

		public static readonly DependencyProperty PasswordProperty =
			DependencyProperty.Register("Password", typeof(string), 
			typeof(BindablePasswordBehavior), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceChanged));

		protected override void OnAttached()
		{
			base.OnAttached();
			SetPasswordView(Password);
			AssociatedObject.PasswordChanged += OnTargetChanged;
		}
		protected override void OnDetaching()
		{
			AssociatedObject.PasswordChanged -= OnTargetChanged;
			base.OnDetaching();
		}

		private void SetPasswordView(string password)
		{
			if (mUpdateScope.IsInScope || AssociatedObject == null)
			{
				return;
			}

			AssociatedObject.Password = password;
		}

		private void OnTargetChanged(object sender, RoutedEventArgs e)
		{
			using (mUpdateScope.Enter())
			{
				SetCurrentValue(PasswordProperty, AssociatedObject.Password);
			}
		}
		private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CastTo<BindablePasswordBehavior>()?.SetPasswordView(e.NewValue as string);
		}
	}
}