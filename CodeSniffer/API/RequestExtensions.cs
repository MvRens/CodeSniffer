using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CodeSniffer.API
{
    public static class RequestExtensions
    {
        public static IReadOnlyList<CultureInfo> Cultures(this HttpRequest request)
        {
            return request.GetTypedHeaders().AcceptLanguage
                .OrderByDescending(l => l.Quality ?? 1)
                .Select(l =>
                {
                    try
                    {
                        return CultureInfo.GetCultureInfo(l.Value.ToString());
                    }
                    catch (CultureNotFoundException)
                    {
                        return null;
                    }
                })
                .Where(l => l != null)
                .Cast<CultureInfo>()
                .Distinct()
                .ToList();
        }


        public static string Author(this HttpRequest request)
        {
            var usernameClaim = request.HttpContext.User.FindFirst(ClaimTypes.Name);
            if (usernameClaim == null)
                throw new UnauthorizedAccessException();

            return usernameClaim.Value;
        }
    }
}
