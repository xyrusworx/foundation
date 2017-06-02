using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class BindingEvaluator : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value), typeof(object), typeof(BindingEvaluator), 
			new FrameworkPropertyMetadata(null));

		private Binding mValueBinding;

		public BindingEvaluator([CanBeNull] Binding binding)
		{
			ValueBinding = binding;
		}

		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public Binding ValueBinding
		{
			get { return mValueBinding; }
			set { mValueBinding = value; }
		}

		public object Evaluate(object source)
		{
			DataContext = source;
			SetBinding(ValueProperty, ValueBinding);
			return Value;
		}
	}
}
