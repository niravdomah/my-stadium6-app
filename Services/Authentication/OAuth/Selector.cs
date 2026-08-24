using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.OAuth
{
	// From https://github.com/IdentityModel/IdentityModel.AspNetCore.AccessTokenValidation/blob/main/src/Selector.cs
	public static class Selector
	{
		/// <summary>
		/// Provides a forwarding func for JWT vs reference tokens (based on existence of dot in token)
		/// </summary>
		/// <param name="introspectionScheme">Scheme name of the introspection handler</param>
		/// <returns></returns>
		public static Func<HttpContext, string> ForwardReferenceToken(string introspectionScheme = "introspection")
		{
			string Select(HttpContext context)
			{
				var (scheme, credential) = GetSchemeAndCredential(context);
				if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) &&
					!credential.Contains("."))
				{
					return introspectionScheme;
				}

				return null;
			}

			return Select;
		}

		/// <summary>
		/// Extracts scheme and credential from Authorization header (if present)
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static (string, string) GetSchemeAndCredential(HttpContext context)
		{
			string header = context.Request.Headers["Authorization"].FirstOrDefault();

			if (string.IsNullOrEmpty(header))
			{
				return ("", "");
			}

			string[] parts = header.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2)
			{
				return ("", "");
			}

			return (parts[0], parts[1]);
		}
	}
}
