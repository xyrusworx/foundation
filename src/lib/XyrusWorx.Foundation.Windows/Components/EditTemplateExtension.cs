using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class EditTemplateExtension : TemplateExtension
	{
		private BindingFactory mPropertyBindingFactory;

		public EditTemplateExtension(string propertyPath)
		{
			mPropertyBindingFactory = new BindingFactory { Mode = BindingMode.OneWay };
			PropertyPath = propertyPath;
		}

		[ConstructorArgument("propertyPath")]
		public string PropertyPath
		{
			get { return mPropertyBindingFactory.PathString; }
			set { mPropertyBindingFactory.PathString = value; }
		}

		public IValueConverter PropertyConverter
		{
			get { return mPropertyBindingFactory.Converter; }
			set { mPropertyBindingFactory.Converter = value; }
		}
		public object PropertyConverterParameter
		{
			get { return mPropertyBindingFactory.ConverterParameter; }
			set { mPropertyBindingFactory.ConverterParameter = value; }
		}
		public object PropertyConverterParameterResourceKey
		{
			get { return mPropertyBindingFactory.ConverterParameterResourceKey; }
			set { mPropertyBindingFactory.ConverterParameterResourceKey = value; }
		}

		public int MaxLength { get; set; }
		public bool DelayUpdate { get; set; }

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var textBox = CreateElement<TextBox>();

			textBox.SetValue(Control.BorderThicknessProperty, new Thickness(0));
			textBox.SetValue(TextBlock.PaddingProperty, new Thickness(4, 0, 4, 0));
			textBox.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
			textBox.SetValue(FrameworkElement.MarginProperty, new Thickness(-1, 0, 0, 0));
			textBox.SetValue(TextBox.MaxLengthProperty, MaxLength);
			textBox.SetBinding(TextBox.TextProperty, PropertyBindingFactory.CreateBinding(BindingMode.TwoWay, DelayUpdate ? UpdateSourceTrigger.LostFocus : UpdateSourceTrigger.PropertyChanged));

			template.VisualTree = textBox;

			return template;
		}

		protected BindingFactory PropertyBindingFactory => mPropertyBindingFactory;
	}
}