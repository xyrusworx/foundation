using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using XyrusWorx.Collections;

namespace XyrusWorx
{
	[PublicAPI]
	public class AssociativeResult<TId> : IResult
	{
		private readonly Dictionary<TId, IResult> mResults;

		public AssociativeResult()
		{
			mResults = new Dictionary<TId, IResult>();
		}
		public AssociativeResult(IEnumerable<KeyValuePair<TId, IResult>> results) : this()
		{
			results?.Foreach(x => mResults.Add(x.Key, x.Value));
		}

		public bool HasError => mResults.Values.Any(x => x.HasError);
		public string ErrorDescription
		{
			get
			{
				if (!HasError)
				{
					return null;
				}

				var sb = new StringBuilder();

				foreach (var r in mResults)
				{
					if (r.Value.HasError)
					{
						sb.AppendLine($"{r.Key}: {r.Value.ErrorDescription}");
					}
				}

				return sb.ToString();
			}
		}

		public IResult this[TId id]
		{
			[NotNull]
			get
			{
				if (id == null) throw new ArgumentNullException(nameof(id));
				return mResults.GetValueByKeyOrDefault(id) ?? Result.Success;
			}

			[CanBeNull]
			set
			{
				if (id == null) throw new ArgumentNullException(nameof(id));
				mResults.AddOrUpdate(id, value ?? Result.Success);
			}
		}

		[NotNull]
		public IDictionary<TId, IResult> Results => mResults;
	}
}