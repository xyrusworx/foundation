﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.MVVM
{
	[PublicAPI]
	public abstract class CollectionViewModel<T> : ViewModel
	{
		public bool HasItems => Items.Any();
		public abstract IList<T> Items { get; }
	}
}