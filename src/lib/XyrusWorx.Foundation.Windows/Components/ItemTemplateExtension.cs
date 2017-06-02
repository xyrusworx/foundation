using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class ItemTemplateExtension : TemplateExtension
	{
		private BindingFactory mItemPropertyBindingFactory;

		public ItemTemplateExtension(string itemPropertyPath)
		{
			mItemPropertyBindingFactory = new BindingFactory { Mode = BindingMode.OneWay };
			ItemPropertyPath = itemPropertyPath;
		}

		[ConstructorArgument("itemPropertyPath")]
		public string ItemPropertyPath
		{
			get { return mItemPropertyBindingFactory.PathString; }
			set { mItemPropertyBindingFactory.PathString = value; }
		}

		public IValueConverter ItemPropertyConverter
		{
			get { return mItemPropertyBindingFactory.Converter; }
			set { mItemPropertyBindingFactory.Converter = value; }
		}
		public object ItemPropertyConverterParameter
		{
			get { return mItemPropertyBindingFactory.ConverterParameter; }
			set { mItemPropertyBindingFactory.ConverterParameter = value; }
		}
		public object ItemPropertyConverterParameterResourceKey
		{
			get { return mItemPropertyBindingFactory.ConverterParameterResourceKey; }
			set { mItemPropertyBindingFactory.ConverterParameterResourceKey = value; }
		}

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var resultItem = CreateElement<TextBlock>();

			resultItem.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
			resultItem.SetBinding(TextBlock.TextProperty, mItemPropertyBindingFactory.CreateBinding());

			template.VisualTree = resultItem;

			return template;
		}

		protected BindingFactory ItemPropertyBindingFactory => mItemPropertyBindingFactory;
	}
}