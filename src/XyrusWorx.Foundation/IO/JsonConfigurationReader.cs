using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public sealed class JsonConfigurationReader<TModel> : ConfigurationReader<TModel> where TModel: class, new()
	{
		private readonly IBlobStore mBlobStore;
		private readonly StringKey mBlobKey;
		private readonly Encoding mEncoding;
		private HashSet<StringKeySequence> mKnownKeys;

		public JsonConfigurationReader([NotNull] IBlobStore blobStore, StringKey blobKey, Encoding encoding = null)
		{
			if (blobStore == null)
			{
				throw new ArgumentNullException(nameof(blobStore));
			}

			if (blobKey.IsEmpty)
			{
				throw new ArgumentNullException(nameof(blobKey));
			}

			mBlobStore = blobStore;
			mBlobKey = blobKey;
			mEncoding = encoding;
		}

		protected override bool IsValidSequence(StringKeySequence sequence)
		{
			return mKnownKeys?.Contains(sequence.Normalize()) ?? false;
		}
		protected override TModel CreateModel()
		{
			var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };

			if (!mBlobStore.Exists(mBlobKey))
			{
				mKnownKeys = new HashSet<StringKeySequence>();
				return new TModel();
			}

			using (var reader = mBlobStore.Open(mBlobKey).AsText(mEncoding ?? Encoding.UTF8).Read())
			{
				JObject obj = (JObject)serializer.Deserialize(new JsonTextReader(reader));

				mKnownKeys = new HashSet<StringKeySequence>();
				CollectKnownKeys(obj, new StringKeySequence());
			}

			using (var reader = mBlobStore.Open(mBlobKey).AsText(mEncoding ?? Encoding.UTF8).Read())
			{
				return serializer.Deserialize<TModel>(new JsonTextReader(reader)); 
			}
		}

		private void CollectKnownKeys(JObject obj, StringKeySequence current)
		{
			if (obj == null)
			{
				return;
			}

			foreach (var element in obj)
			{
				mKnownKeys.Add(current.Concat(element.Key.AsKey().Normalize()));

				if (element.Value?.Type == JTokenType.Object)
				{
					CollectKnownKeys((JObject)element.Value, current.Concat(element.Key.AsKey().Normalize()));
				}
			}
		}
	}
}