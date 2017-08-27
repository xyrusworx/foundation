using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx
{
	[PublicAPI]
	public class MultiResult : IResult
	{
		public bool HasError => Results.Any(x => x?.HasError ?? false);
		public string ErrorDescription
		{
			get
			{
				var errors = Results.Where(x => x?.HasError ?? false).ToArray();
				if (errors.Length == 0)
				{
					return null;
				}

				if (errors.Length == 1)
				{
					return errors[0].ErrorDescription;
				}

				return "Multiple errors have occured.";
			}
		}

		[NotNull]
		public IList<IResult> Results { get; } = new ResultListProxy();
		
		public void ThrowIfError()
		{
			var errors = Results.Where(x => x?.HasError ?? false).ToArray();
			if (errors.Length == 0)
			{
				return;
			}

			if (errors.Length == 1)
			{
				throw new Exception(errors[0].ErrorDescription);
			}

			throw new AggregateException(errors.Select(x => new Exception(x.ErrorDescription)));
		}

		class ResultListProxy : ListProxy<IResult>
		{
			public override void Add([NotNull] IResult item)
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				base.Add(item);
			}
			public override void Insert(int index, [NotNull] IResult item)
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				base.Insert(index, item);
			}

			[NotNull]
			public override IResult this[int index]
			{
				get => base[index];
				set
				{
					if (value == null)
					{
						throw new ArgumentNullException(nameof(value));
					}

					base[index] = value;
				}
			}
		}
	}
}