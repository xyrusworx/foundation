using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI, DataContract]
	public abstract class CollectionViewModel<T> : ViewModel
	{
		[IgnoreDataMember]
		public bool HasItems => Items.Any();

		[IgnoreDataMember]
		public abstract IList<T> Items { get; }
	}
}