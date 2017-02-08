using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class DateTimeEditTemplateExtension : EditTemplateExtension
	{
		public DateTimeEditTemplateExtension(string propertyPath) : base(propertyPath)
		{
		}

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var textBox = CreateElement<DatePicker>();

			textBox.SetValue(Control.BorderThicknessProperty, new Thickness(0));
			textBox.SetValue(TextBlock.PaddingProperty, new Thickness(4, 0, 4, 0));
			textBox.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
			textBox.SetValue(FrameworkElement.MarginProperty, new Thickness(-1, 0, 0, 0));
			textBox.SetBinding(DatePicker.SelectedDateProperty, PropertyBindingFactory.CreateBinding(BindingMode.TwoWay, UpdateSourceTrigger.PropertyChanged));

			template.VisualTree = textBox;

			return template;
		}
	}
}