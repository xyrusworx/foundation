using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class CheckEditTemplateExtension : EditTemplateExtension
	{
		public CheckEditTemplateExtension(string propertyPath) : base(propertyPath)
		{
		}

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var textBox = CreateElement<CheckBox>();

			textBox.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			textBox.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

			textBox.SetBinding(ToggleButton.IsCheckedProperty, PropertyBindingFactory.CreateBinding(BindingMode.TwoWay, UpdateSourceTrigger.PropertyChanged));

			template.VisualTree = textBox;

			return template;
		}
	}
}