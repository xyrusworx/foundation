using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI, MarkupExtensionReturnType(typeof(DataTemplate))]
	public class ComboBoxItemTemplateExtension : ItemTemplateExtension
	{
		public ComboBoxItemTemplateExtension(string itemPropertyPath) : base(itemPropertyPath)
		{
		}

		protected override DataTemplate GetTemplate()
		{
			var template = new DataTemplate();
			var resultItem = CreateElement<Label>();

			resultItem.Name = "label";
			resultItem.SetValue(Control.PaddingProperty, new Thickness(3, 0, 3, 0));
			resultItem.SetBinding(ContentControl.ContentProperty, ItemPropertyBindingFactory.CreateBinding());

			var mouseOverBinding = new MultiBinding();

			mouseOverBinding.Bindings.Add(new Binding(UIElement.IsMouseOverProperty.Name)
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, /*typeof(UIElement)*/ typeof(ComboBoxItem), 1),
				Mode = BindingMode.OneWay
			});

			mouseOverBinding.Converter = new MultiBooleanAndConverter();

			var isMouseOverTrigger = new DataTrigger { Binding = mouseOverBinding, Value = true };
			isMouseOverTrigger.Setters.Add(new Setter
			{
				TargetName = resultItem.Name,
				Value = Application.Current?.TryFindResource("LightBackgroundBrush") as Brush,
				Property = Control.ForegroundProperty
			});

			template.Triggers.Add(isMouseOverTrigger);
			template.VisualTree = resultItem;

			return template;
		}
	}
}