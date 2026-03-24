namespace GamaEdtech.Common.Localization
{
    using System.Globalization;
    using System.Linq;

    using GamaEdtech.Common.Core;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public static class LocalizationExtensions
    {
        public static RequestLocalizationOptions RequestLocalizationOptions
        {
            get
            {
                var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(Globals.GetCulture).ToList();
                return new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(Constants.DefaultLanguageCode),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,
                    RequestCultureProviders = [new RouteValueRequestCultureProvider()],
                };
            }
        }

        public static void ConfigureRequestLocalization(this IServiceCollection services)
        {
            var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(Globals.GetCulture).ToList();
            _ = services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(Constants.DefaultLanguageCode);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders.Insert(0, new RouteValueRequestCultureProvider());
            });
        }
    }
}
