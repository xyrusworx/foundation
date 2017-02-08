using System;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	class ServiceHandle
	{
		private readonly Guid mTypeHash;

		public ServiceHandle([NotNull] Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			Type = type;
			ConcreteType = type;
			Instance = null;
			mTypeHash = new Guid(Type.AssemblyQualifiedName.Hash());
		}
		public ServiceHandle([NotNull] Type type, [NotNull] Type concreteType)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (concreteType == null)
			{
				throw new ArgumentNullException(nameof(concreteType));
			}

			Type = type;
			ConcreteType = concreteType;
			Instance = null;
			mTypeHash = new Guid(Type.AssemblyQualifiedName.Hash());
		}
		public ServiceHandle([NotNull] Type type, [NotNull] object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var targetType = type.GetTypeInfo();
			var concreteType = instance.GetType();

			if (!targetType.IsAssignableFrom(concreteType.GetTypeInfo()))
			{
				throw new ArgumentException($"The provided instance of type \"{concreteType.FullName}\" is not assignable to the key type \"{targetType.FullName}\"");
			}

			Type = type;
			ConcreteType = concreteType;
			Instance = instance;
			mTypeHash = new Guid(Type.AssemblyQualifiedName.Hash());
		}

		public object Instance { get; }
		public Type Type { get; }
		public Type ConcreteType { get; }

		public bool HasInstance => Instance != null;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ServiceHandle) obj);
		}
		public bool Equals(ServiceHandle other)
		{
			return mTypeHash.Equals(other.mTypeHash);
		}

		public override int GetHashCode()
		{
			return mTypeHash.GetHashCode();
		}

		public static bool operator ==(ServiceHandle left, ServiceHandle right)
		{
			return Equals(left, right);
		}
		public static bool operator !=(ServiceHandle left, ServiceHandle right)
		{
			return !Equals(left, right);
		}
	}
}