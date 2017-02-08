using System;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public sealed class CommandLineReader<TModel> : ConfigurationReader<TModel> where TModel: class, new()
	{
		private readonly IKeyValueStore<string> mCommandLine;

		public CommandLineReader([NotNull] IKeyValueStore<string> commandLine)
		{
			if (commandLine == null)
			{
				throw new ArgumentNullException(nameof(commandLine));
			}

			mCommandLine = commandLine;
		}

		protected override bool IsValidSequence(StringKeySequence sequence)
		{
			if (sequence.IsEmpty)
			{
				return false;
			}

			return mCommandLine.Exists(sequence.ToString("-").AsKey().Normalize());
		}
		protected override TModel CreateModel()
		{
			var model = new TModel();
			
			SetValues(new StringKeySequence(), model);

			return model;
		}

		private void SetValues(StringKeySequence sequence, object model)
		{
			if (model == null)
			{
				return;
			}

			var properties = model.GetType().GetTypeInfo().DeclaredProperties;

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType.GetTypeInfo();
				var propertyName = property.Name.AsKey().Normalize();
				var fullSequence = sequence.Concat(propertyName).ToString("-").AsKey().Normalize();

				if (propertyType.IsValueType || Equals(propertyType, typeof(string).GetTypeInfo()))
				{
					if (!property.CanWrite || !mCommandLine.Exists(fullSequence))
					{
						continue;
					}

					var value = mCommandLine.Read(fullSequence);
					var deserializedValue = value?.TryDeserialize(propertyType, CultureInfo.InvariantCulture);

					property.SetValue(model, deserializedValue);
				}
				else if (property.SetMethod?.GetParameters().Length == 1)
				{
					SetValues(sequence.Concat(propertyName), property.GetValue(model));
				}
			}
		}
	}
}