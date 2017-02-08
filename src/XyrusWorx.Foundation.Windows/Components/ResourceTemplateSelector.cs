using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public sealed class ResourceTemplateSelector : DataTemplateSelector
	{
		public ResourceDictionary ResourceDictionary { get; set; }

		public Binding KeyBinding { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				return null;
			}

			object key = item.GetType();
			if (KeyBinding != null)
			{
				var evaluator = new BindingEvaluator(KeyBinding);
				key = evaluator.Evaluate(item);
			}

			if (ResourceDictionary == null)
			{
				return Application.Current?.TryFindResource(key) as DataTemplate;
			}

			if (ResourceDictionary.Contains(key))
			{
				return ResourceDictionary[key] as DataTemplate;
			}

			return null;
		}
	}
}