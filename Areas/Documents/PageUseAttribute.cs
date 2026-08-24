using System;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Documents
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class PageUseAttribute : Attribute
	{
		public PageUseAttribute(params string[] pages)
		{
			Pages = pages;
		}

		public string[] Pages { get; }
	}
}
