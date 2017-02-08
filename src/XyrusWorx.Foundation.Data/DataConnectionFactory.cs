using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public abstract class DataConnectionFactory
	{
		private static readonly Dictionary<StringKey, DataConnectionFactory> mRegisteredFactories = new Dictionary<StringKey, DataConnectionFactory>();

		[NotNull]
		public static IReadOnlyDictionary<StringKey, DataConnectionFactory> Factories => mRegisteredFactories;

		[NotNull] public static DataConnectionFactory Open<T>() where T : DataConnectionFactory
		{
			var name = typeof(T).AssemblyQualifiedName;
			var key = new StringKey(name ?? typeof(T).FullName);

			if (!mRegisteredFactories.ContainsKey(key.Normalize()))
			{
				throw new KeyNotFoundException($"The provider \"{key}\" wasn't registered.");
			}

			return mRegisteredFactories[key.Normalize()];
		}
		[NotNull] public static DataConnectionFactory Open(StringKey factoryKey)
		{
			if (factoryKey.IsEmpty)
			{
				throw new ArgumentNullException(nameof(factoryKey));
			}

			if (!mRegisteredFactories.ContainsKey(factoryKey.Normalize()))
			{
				throw new KeyNotFoundException($"The provider \"{factoryKey}\" wasn't registered.");
			}

			return mRegisteredFactories[factoryKey.Normalize()];
		}

		public static DataConnectionFactory Register<TFactory>() where TFactory : DataConnectionFactory, new() => Register(new TFactory());
		public static DataConnectionFactory Register([NotNull] DataConnectionFactory factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			return Register(factory.GetType().AssemblyQualifiedName, factory);
		}
		public static DataConnectionFactory Register<TFactory>(StringKey factoryKey) where TFactory : DataConnectionFactory, new() => Register(factoryKey, new TFactory());
		public static DataConnectionFactory Register(StringKey factoryKey, [NotNull] DataConnectionFactory factory)
		{
			if (factoryKey.IsEmpty)
			{
				throw new ArgumentNullException(nameof(factoryKey));
			}

			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (mRegisteredFactories.ContainsKey(factoryKey.Normalize()))
			{
				throw new InvalidOperationException($"The provider \"{factoryKey}\" was already registered.");
			}

			mRegisteredFactories.Add(factoryKey.Normalize(), factory);

			return factory;
		}

		[NotNull] public static DataConnection CreateConnection<TFactory>(IKeyValueStore connectionData) where TFactory : DataConnectionFactory
		{
			return Open<TFactory>().CreateConnection(connectionData);
		}
		[NotNull] public static DataConnection CreateConnection(StringKey factoryKey, IKeyValueStore connectionData)
		{
			return Open(factoryKey).CreateConnection(connectionData);
		}

		[NotNull] public DataConnection CreateConnection(IKeyValueStore connectionData)
		{
			var connection = CreateConnectionOverride(connectionData ?? new MemoryKeyValueStore());
			if (connection == null)
			{
				throw new TypeLoadException();
			}

			return new DataConnection(connection);
		}
		[NotNull] protected abstract IDbConnection CreateConnectionOverride(IKeyValueStore connectionData);
	}

	[PublicAPI]
	public class DataConnectionFactory<T> : DataConnectionFactory where T : IDbConnection, new()
	{
		protected sealed override IDbConnection CreateConnectionOverride(IKeyValueStore connectionData)
		{
			var connection = new T();
			var connectionString = new DbConnectionStringBuilder();

			foreach (var key in connectionData.GetKeys())
			{
				connectionString.Add(key, connectionData.Read(key));
			}

			connection.ConnectionString = connectionString.ToString();

			return connection;
		}
	}
}