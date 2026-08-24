using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class AuditLog
	{
		public int Id { get; set; }

		public DateTimeOffset ChangedOn { get; set; }

		public ChangedByUser ChangedBy { get; set; }

		public AuditType Type { get; set; }

		public AuditAction Action { get; set; }

		public string Source { get; set; }

		public object OldValue { get; set; }

		public object NewValue { get; set; }
	}

	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class ChangedByUser
	{
		public string Id { get; set; }

		public string Email { get; set; }
	}
}
