using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using System;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication
{
	public static class AuthenticationSchemes
	{
		public static string GetDefaultAuthenticationScheme(AuthenticationType authenticationType)
		{
			switch (authenticationType)
			{
				case AuthenticationType.Anonymous: return "Anonymous";
				case AuthenticationType.Cookie: return IdentityConstants.ApplicationScheme;
				case AuthenticationType.OAuth: return JwtBearerDefaults.AuthenticationScheme;
				case AuthenticationType.Windows: return NegotiateDefaults.AuthenticationScheme;
				default: throw new ArgumentException($"Unhandled authentication type: {authenticationType}.");
			}
		}
	}
}
