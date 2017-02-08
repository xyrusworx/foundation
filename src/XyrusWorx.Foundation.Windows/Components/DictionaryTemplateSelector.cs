using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[ContentProperty("Templates")]
	[PublicAPI]
	public class DictionaryTemplateSelector : DataTemplateSelector
	{
		public DictionaryTemplateSelector()
		{
			Templates = new Collection<DataTemplate>();
		}

		[UsedImplicitly]
		public Collection<DataTemplate> Templates
		{
			get;
			set;
		}
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				return null;
			}

			var itemType = item.GetType();
			var templates =
				from template in Templates ?? new Collection<DataTemplate>()
				let type = template.DataType as Type
				where type != null
				select new
				{
					Type = type,
					Template = template
				};

			return templates.FirstOrDefault(x => x.Type.IsAssignableFrom(itemType))?.Template;

		}
	}
}
