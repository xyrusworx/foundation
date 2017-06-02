using System;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public abstract class TemplateExtension : MarkupExtension
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var template = GetTemplate();

			return template;
		}

		protected FrameworkElementFactory CreateElement<T>() where T: FrameworkElement
		{
			return CreateElement(typeof (T));
		}
		protected abstract DataTemplate GetTemplate();

		private FrameworkElementFactory CreateElement([NotNull] Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var factory = new FrameworkElementFactory(type);
			var style = TryFindResourceIncludingMergedDictionaries(Application.Current?.Resources, type);

			factory.SetValue(FrameworkElement.StyleProperty, style);

			return factory;
		}

		// http://stackoverflow.com/questions/3786206/how-to-get-a-resource-from-a-merged-resourcedictionary-in-a-code-behind-file
		private object TryFindResourceIncludingMergedDictionaries(ResourceDictionary dictionary, object key)
		{
			if (dictionary == null)
			{
				return null;
			}

			var hasKey = dictionary.Keys.OfType<object>().Contains(key);
			if (!hasKey)
			{
				foreach (var mergedDictionary in dictionary.MergedDictionaries)
				{
					var valueInMergedDictionaries = TryFindResourceIncludingMergedDictionaries(mergedDictionary, key);
					if (valueInMergedDictionaries != null)
					{
						return valueInMergedDictionaries;
					}
				}

				return null;
			}

			return dictionary[key];
		}
	}
}