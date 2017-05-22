using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;

namespace XyrusWorx
{
	[PublicAPI]
	public class CommandLineProcessor
	{
		private readonly Type mModelType;
		private readonly ILogWriter mLog;
		private readonly List<Tuple<ICommandLineTokenVisitor, PropertyInfo>> mProperties;

		public CommandLineProcessor([NotNull] Type modelType, ILogWriter log = null)
		{
			if (modelType == null)
			{
				throw new ArgumentNullException(nameof(modelType));
			}

			mProperties = new List<Tuple<ICommandLineTokenVisitor, PropertyInfo>>();
			mModelType = modelType;
			mLog = log;

			CollectProperties(mModelType);
		}

		public IEnumerable<StringKey> GetKnownKeys() => mProperties.Select(x => x.Item1.GetKey()).Where(x => !x.IsEmpty);

		public void Read([NotNull] CommandLineKeyValueStore commandLine, [NotNull] object model)
		{
			if (commandLine == null)
			{
				throw new ArgumentNullException(nameof(commandLine));
			}

			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}

			if (!mModelType.GetTypeInfo().IsInstanceOfType(model))
			{
				return;
			}

			foreach (var property in mProperties)
			{
				property.Item1.Prepare(commandLine);
			}

			foreach (var property in mProperties)
			{
				property.Item1.Visit(commandLine, property.Item2, model, mLog ?? new NullLogWriter());
			}
		}
		public void WriteDocumentation([NotNull] CommandLineDocumentation documentation)
		{
			if (documentation == null)
			{
				throw new ArgumentNullException(nameof(documentation));
			}

			foreach (var property in mProperties)
			{
				var attribute = property.Item2.GetCustomAttribute<CommandLineAnnotationAttribute>();
				if (attribute == null)
				{
					attribute = new CommandLineAnnotationAttribute();
				}

				attribute.AddToDocumentation(documentation, property.Item2);
			}
		}

		private void CollectProperties(Type type)
		{
			var allProperties = type.GetTypeInfo().DeclaredProperties.Where(x => !x.GetMethod.IsStatic && x.CanWrite);

			foreach (var property in allProperties)
			{
				var attributes = property.GetCustomAttributes();

				foreach (var attribute in attributes)
				{
					if (attribute is ICommandLineTokenVisitor)
					{
						mProperties.Add(new Tuple<ICommandLineTokenVisitor, PropertyInfo>((ICommandLineTokenVisitor)attribute, property));
						break;
					}
				}
			}

			var typeInfo = type.GetTypeInfo();

			if (typeInfo.BaseType != null && typeInfo.BaseType != typeof(object))
			{
				CollectProperties(typeInfo.BaseType);
			}
		}
	}

	[PublicAPI]
	public class CommandLineProcessor<TModel> : CommandLineProcessor where TModel : class
	{
		public CommandLineProcessor(ILogWriter log = null) : base(typeof(TModel), log) { }
	}
}