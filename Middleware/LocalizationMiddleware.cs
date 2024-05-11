using System.Globalization;

namespace weapploc.Middleware;

public class LocalizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cultureKey = context.Request.Headers.AcceptLanguage.ToString();

        if (cultureKey.Contains(','))
        {
            cultureKey = cultureKey.Split(',').First();
            if (cultureKey.Length == 2)
            {
                cultureKey += "-" + cultureKey.ToUpper();
            }
        }

        if (!string.IsNullOrEmpty(cultureKey))
        {
            if (DoesCultureExist(cultureKey!))
            {
                var culture = new CultureInfo(cultureKey!);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        await next(context);
    }

    private static bool DoesCultureExist(string cultureName)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => 
            string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
    }
}