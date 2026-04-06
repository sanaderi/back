namespace GamaEdtech.Common.Localization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Collections.Generic;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class RouteValueRequestCultureProvider() : IRequestCultureProvider
    {
        public async Task<ProviderCultureResult?> DetermineProviderCultureResult([NotNull] HttpContext httpContext)
        {
            var path = httpContext.Request.Path;

            if (string.IsNullOrEmpty(path))
            {
                return new ProviderCultureResult(Constants.DefaultLanguageCode);
            }

            var routeValues = httpContext.Request.Path.Value?.Split('/');
            if (routeValues is null || routeValues.Length <= 1)
            {
                return new ProviderCultureResult(Constants.DefaultLanguageCode);
            }

            var lst = await httpContext.RequestServices.GetRequiredService<ILanguageService>().GetActiveLanguagesAsync();
            if (lst is null)
            {
                return new ProviderCultureResult(Constants.DefaultLanguageCode);
            }

            var culture = lst.Find(t => t!.Equals(routeValues[1], StringComparison.OrdinalIgnoreCase));
            return culture is null
                ? new ProviderCultureResult(lst[0])
                : new ProviderCultureResult(culture);
        }
    }
}
