using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.ViewModels
{
	[PublicAPI]
	public class SearchExpression
	{
		private Regex[] mSearchStringTokens;
		private Regex mCompositeSearchString;
		private string mOriginalSearchString;

		public SearchExpression(string searchString)
		{
			mSearchStringTokens = Regex
				.Split(searchString ?? string.Empty, @"\W+", RegexOptions.IgnoreCase)
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(StringUtils.ToRegexLiteral)
				.Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase))
				.ToArray();
			mCompositeSearchString = new Regex(searchString.NormalizeNull()?.ToRegexLiteral() ?? ".*");
			mOriginalSearchString = searchString;
		}

		public string Input => mOriginalSearchString;
		
		public Regex ExactMatchToken => mCompositeSearchString;
		public Regex[] WordTokens => mSearchStringTokens;

		public bool IsMatch(string input)
		{
			if (string.IsNullOrEmpty(mOriginalSearchString))
			{
				return true;
			}

			if (ExactMatchToken.IsMatch(input ?? string.Empty))
			{
				return true;
			}

			foreach (var token in WordTokens)
			{
				if (token.IsMatch(input ?? string.Empty))
				{
					return true;
				}
			}

			return false;
		}
	}
}