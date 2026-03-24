namespace GamaEdtech.Common.Localization
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Collections.Generic;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;

    public class CultureRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, [NotNull] RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.ContainsKey(Constants.LanguageIdentifier))
            {
                return false;
            }

            var lst = httpContext?.RequestServices.GetRequiredService<ILanguageService>().GetActiveLanguages();
            var lang = values[Constants.LanguageIdentifier]?.ToString();

            return lst?.Exists(t => t!.Equals(lang, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }
}
