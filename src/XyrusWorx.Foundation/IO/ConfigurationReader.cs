using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using XyrusWorx.Structures;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public abstract class ConfigurationReader : IHierarchicKeyValueStore
	{
		public virtual bool Exists(StringKeySequence sequence)
		{
			return GetValue(sequence) != null;
		}
		public object Read(StringKeySequence sequence)
		{
			if (Exists(sequence))
			{
				return GetValue(sequence);
			}

			return null;
		}

		protected abstract object GetValue(StringKeySequence sequence);
		protected abstract IEnumerable<StringKeySequence> EnumerateKeySequences();

		IEnumerable<StringKeySequence> IHierarchicKeyValueStore<object>.GetKeys() => EnumerateKeySequences();

		void IHierarchicKeyValueStore<object>.Write(StringKeySequence sequence, object value) { throw new NotSupportedException(); }
		void IHierarchicKeyValueStore<object>.SetDefault(StringKeySequence key, object defaultValue) { throw new NotSupportedException(); }
	}

	[PublicAPI]
	public abstract class ConfigurationReader<TModel> : ConfigurationReader where TModel: class, new()
	{
		private TModel mModel;

		private readonly Hierarchy<StringKey, object> mData;
		private readonly HashSet<StringKeySequence> mSequences;

		protected ConfigurationReader()
		{
			mData = new Hierarchy<StringKey, object>();
			mSequences = new HashSet<StringKeySequence>();
		}

		[NotNull]
		public TModel Model => mModel ?? (mModel = Initialize());

		public override bool Exists(StringKeySequence sequence)
		{
			if (mModel == null)
			{
				mModel = Initialize();
			}

			return IsValidSequence(sequence) && base.Exists(sequence);
		}

		protected sealed override object GetValue(StringKeySequence sequence)
		{
			if (mModel == null)
			{
				mModel = Initialize();
			}

			var current = mData;
			var segments = sequence.Segments;

			foreach (var segment in segments)
			{
				if (!(current?.Children.Contains(segment.Normalize()) ?? false))
				{
					return null;
				}

				current = current.Children[segment.Normalize()];
			}

			return current?.Value;
		}
		protected sealed override IEnumerable<StringKeySequence> EnumerateKeySequences()
		{
			return mSequences.AsEnumerable();
		}
		protected abstract TModel CreateModel();
		protected virtual bool IsValidSequence(StringKeySequence sequence) => true;

		private TModel Initialize()
		{
			var instance = CreateModel() ?? new TModel();

			Read(instance, new StringKeySequence(), mData, mSequences);

			return instance;
		}
		private void Read(object model, StringKeySequence? trackingSequence, Hierarchy<StringKey, object> subStructure, HashSet<StringKeySequence> sequenceCollector)
		{
			if (model == null)
			{
				return;
			}

			subStructure.Value = model;

			var type = model.GetType().GetTypeInfo();
			if (type.IsValueType || typeof(string).GetTypeInfo() == type)
			{
				if (trackingSequence != null && IsValidSequence(trackingSequence.Value))
				{
					mSequences.Add(trackingSequence.Value);
				}

				return;
			}

			if (typeof(IEnumerable<object>).GetTypeInfo().IsAssignableFrom(type))
			{
				var enumerable = model as IEnumerable<object> ?? new object[0];
				var counter = 0;

				foreach (var element in enumerable)
				{
					var child = new Hierarchy<StringKey, object>();
					var childKey = new StringKey(counter.ToString()).Normalize();

					Read(element, null, child, sequenceCollector);
					subStructure.Children[childKey] = child;

					counter++;
				}

				return;
			}

			var properties = model.GetType().GetTypeInfo().DeclaredProperties;

			foreach (var property in properties)
			{
				var child = new Hierarchy<StringKey, object>();
				var childKey = new StringKey(property.Name).Normalize();
				var childSequence = trackingSequence?.Concat(childKey);

				if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
				{
					continue;
				}

				if (property.GetMethod.GetParameters().Length > 0)
				{
					continue;
				}

				Read(property.GetValue(model), childSequence, child, sequenceCollector);
				subStructure.Children[childKey] = child;

				if (trackingSequence != null && IsValidSequence(trackingSequence.Value))
				{
					mSequences.Add(trackingSequence.Value);
				}
			}
		}
	}
}