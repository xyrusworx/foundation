using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public class RouteInfo
	{
		private readonly Type mTargetType;
		private readonly MethodBase mTargetMethod;

		internal RouteInfo([NotNull] Type targetType, [NotNull] MethodBase targetMethod, WebServiceVerbs verbs, string route)
		{
			if (targetType == null) throw new ArgumentNullException(nameof(targetType));
			if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

			mTargetType = targetType;
			mTargetMethod = targetMethod;

			AllowedVerbs = verbs;
			Route = route;
		}

		public WebServiceVerbs AllowedVerbs { get; }
		public string Route { get; }

		public string GetRouteSourceExpression()
		{
			var possibleVerbs = Enum
				.GetValues(typeof(WebServiceVerbs))
				.OfType<WebServiceVerbs>()
				.Except(new[] {WebServiceVerbs.All, WebServiceVerbs.None})
				.Select(x => x.ToString().ToUpper())
				.ToArray();

			var verbs = possibleVerbs
				.Where(x => AllowedVerbs.HasFlag((WebServiceVerbs)Enum.Parse(typeof(WebServiceVerbs), x, true)))
				.ToArray();

			var verbString = verbs.Length == 1 ? verbs[0] : $"[ {string.Join(" | ", verbs)} ]";

			if (AllowedVerbs == WebServiceVerbs.All)
			{
				verbString = "*";
			}

			if (verbs.Length > possibleVerbs.Length / 2)
			{
				var deniedVerbs = possibleVerbs.Except(verbs).ToArray();
				verbString = $"~[ {string.Join(" | ", deniedVerbs)} ]";
			}

			return $"{verbString} /{Route?.Replace("{", "<").Replace("}", ">")}";
		}
		public string GetRouteTargetExpression()
		{
			return $"[{mTargetType.Name}]::{mTargetMethod.Name}";
		}
	}
}