using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.IO
{
	[PublicAPI]
	public class XmlConfiguration : KeyValueStore<string>
	{
		private readonly IBlobStore mStorage;
		private readonly StringKey mContainerKey;

		private static readonly XName mRootElementName = XName.Get(@"configuration");
		private static readonly XName mContainerElementName = XName.Get(@"appSettings");
		private static readonly XName mItemElementName = XName.Get(@"add");
		private static readonly XName mKeyAttributeName = XName.Get(@"key");
		private static readonly XName mValueAttributeName = XName.Get(@"value");

		public XmlConfiguration([NotNull] IBlobStore storage, StringKey containerKey)
		{
			if (storage == null)
			{
				throw new ArgumentNullException(nameof(storage));
			}

			if (string.IsNullOrWhiteSpace(containerKey))
			{
				throw new ArgumentNullException(nameof(containerKey));
			}

			mStorage = storage;
			mContainerKey = containerKey;

			if (!mStorage.Exists(containerKey))
			{
				CreateXml();
			}
		}

		private void CreateXml()
		{
			var document = new XDocument(
				new XDeclaration("1.0", "utf-8", ""),
				new XElement(mRootElementName,
					new XElement(mContainerElementName)));

			SaveDocument(document);
		}

		private XElement GetItemElement(string key)
		{
			XDocument document;
			return GetItemElement(key, out document);
		}
		private XElement GetItemElement(string key, out XDocument document)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			var container = GetContainerElement(out document);
			var nodes = container.Elements(mItemElementName);
			var result = nodes.FirstOrDefault(x => x.Attribute(mKeyAttributeName)?.Value == key);

			return result;
		}

		private XElement GetContainerElement()
		{
			XDocument document;
			return GetContainerElement(out document);
		}
		private XElement GetContainerElement(out XDocument document)
		{
			try
			{
				document = LoadDocument();

				var root = document.Element(mRootElementName);
				if (root == null)
				{
					var formattedMessage = $"Failed to read configuration file \"{mContainerKey}\". The root node does not match the required schema.";

					throw new FormatException(formattedMessage);
				}

				var container = root.Element(mContainerElementName);
				if (container == null)
				{
					var formattedMessage = $"Failed to read configuration file \"{mContainerKey}\". The configuration node does not match the required schema.";

					throw new FormatException(formattedMessage);
				}

				return container;
			}
			catch (XmlException xmlException)
			{
				var formattedMessage = $"Failed to read configuration file \"{mContainerKey}\". {xmlException.Message}";

				throw new FormatException(formattedMessage, xmlException);
			}
		}

		private XDocument LoadDocument()
		{
			using (var stream = mStorage.Open(mContainerKey).AsText().Read())
			{
				return XDocument.Load(stream, LoadOptions.None);
			}
		}
		private void SaveDocument(XDocument document)
		{
			mStorage.Erase(mContainerKey); // empty text file before saving

			using (var stream = mStorage.Open(mContainerKey).AsText().Write())
			{
				document.Save(stream, SaveOptions.None);
			}
		}

		protected sealed override string GetValue(StringKey key)
		{
			var element = GetItemElement(key);

			return element?.Attribute(mValueAttributeName)?.Value;
		}
		protected sealed override void SetValue(StringKey key, string value)
		{
			XDocument document;

			var element = GetItemElement(key, out document);

			try
			{
				if (element == null)
				{
					var container = GetContainerElement(out document);
					container.Add(new XElement(mItemElementName,
						new XAttribute(mKeyAttributeName, key),
						new XAttribute(mValueAttributeName, value ?? string.Empty)));
				}
				else
				{
					var attribute = element.Attribute(mValueAttributeName);
					if (attribute == null)
					{
						element.Add(new XAttribute(mValueAttributeName, value ?? string.Empty));
					}
					else
					{
						attribute.Value = value ?? string.Empty;
					}
				}

				SaveDocument(document);
			}
			catch (XmlException xmlException)
			{
				var formattedMessage = $"Failed to write to configuration file \"{mContainerKey}\". {xmlException.Message}";

				throw new FormatException(formattedMessage, xmlException);
			}
		}
		protected sealed override IEnumerable<StringKey> Enumerate()
		{
			var container = GetContainerElement();
			var nodes = container.Elements(mItemElementName);

			return
				from node in nodes
				let keyAttribute = node.Attribute(mKeyAttributeName)
				where keyAttribute != null
				select new StringKey(keyAttribute.Value);
		}
	}
}