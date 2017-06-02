using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Components
{
	[PublicAPI]
	public class BindingFactory
	{
		public string[] Path { get; set; } = new string[0];
		public string PathString
		{
			get
			{
				return string.Join(".", Path?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? new string[0]);
			}
			set
			{
				var elements = new List<string>();
				var data = value ?? string.Empty;

				var braceScope = new Scope();
				var item = string.Empty;

				foreach (var c in data)
				{
					if (c == '(')
					{
						braceScope.Enter();
						item += c;
					}
					else if (c == ')')
					{
						braceScope.Leave();
						item += c;
					}
					else if (c == '.' && !braceScope.IsInScope)
					{
						elements.Add(item);
						item = string.Empty;
					}
					else
					{
						item += c;
					}
				}

				elements.Add(item);
				Path = elements.ToArray();
			}
		}

		public BindingMode Mode { get; set; } = BindingMode.Default;
		public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.Default;

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }
		public object ConverterParameterResourceKey { get; set; }

		public BindingBase CreateBinding(params string[] subPath) => CreateBinding(Mode, UpdateSourceTrigger, subPath);
		public BindingBase CreateBinding(BindingMode mode, params string[] subPath) => CreateBinding(mode, UpdateSourceTrigger, subPath);
        public BindingBase CreateBinding(BindingMode mode, UpdateSourceTrigger trigger, params string[] subPath)
		{
			var pathTrain = Path.Concat(subPath).Where(x => !string.IsNullOrWhiteSpace(x));
			var bindingPath = string.Join(".", pathTrain);

			var converterParameter = ConverterParameter;
			if (ConverterParameterResourceKey != null)
			{
				converterParameter = Application.Current?.TryFindResource(ConverterParameterResourceKey);
			}

			return new Binding(bindingPath)
			{
				Mode = mode,
				UpdateSourceTrigger = trigger,
				Converter = Converter,
				ConverterParameter = converterParameter
			};
		}
	}
}