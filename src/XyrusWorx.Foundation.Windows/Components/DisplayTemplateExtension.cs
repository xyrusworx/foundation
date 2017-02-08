using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class DisplayTemplateExtension : TemplateExtension
	{
		private BindingFactory mPropertyBindingFactory;

		public DisplayTemplateExtension(string propertyPath)
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

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var textBlock = CreateElement<TextBlock>();

			textBlock.SetValue(TextBlock.PaddingProperty, new Thickness(5, 0, 5, 0));
			textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
			textBlock.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
			textBlock.SetBinding(TextBlock.TextProperty, mPropertyBindingFactory.CreateBinding());
			template.VisualTree = textBlock;
			template.Triggers.Add(new DataTrigger
			{
				Binding = new Binding(UIElement.IsEnabledProperty.Name)
				{
					RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1),
					Mode = BindingMode.OneWay
				},
				Value = false
			});

			return template;
		}
	}
}