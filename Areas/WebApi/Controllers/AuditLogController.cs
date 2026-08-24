using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;
using Twenty57.Stadium.WebApp.Spa.Template.Controllers;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[AuthenticationTypeValidation]
	[UserApiKeyValidation]
	public class AuditLogController : AValidatedControllerBase
	{
		private readonly IAuditLogStore auditLogStore;

		public AuditLogController(IAuditLogStore auditLogStore)
		{
			this.auditLogStore = auditLogStore;
		}

		private const string DateFormat = "yyyy-MM-dd";

		// GET: api/auditlog?key=key&dateFrom=yyyy-MM-dd&dateTo=yyyy-MM-dd
		[HttpGet]
		public async Task<IActionResult> All(string key, string dateFrom = null, string dateTo = null)
		{
			if (!TryParseDateParameter("dateFrom", dateFrom, out var from, out var fromError))
				return GetValidationResult(fromError);

			if (!TryParseDateParameter("dateTo", dateTo, out var to, out var toError))
				return GetValidationResult(toError);

			if (from.HasValue && to.HasValue && from.Value > to.Value)
				return GetValidationResult("'dateFrom' cannot be later than 'dateTo'.");

			var query = this.auditLogStore.AuditLogs;

			if (from.HasValue)
			{
				var fromBoundary = new DateTimeOffset(from.Value);
				query = query.Where(a => a.ChangedOn.CompareTo(fromBoundary) >= 0);
			}

			if (to.HasValue)
			{
				var toBoundary = new DateTimeOffset(to.Value.AddDays(1));
				query = query.Where(a => a.ChangedOn.CompareTo(toBoundary) < 0);
			}

			var auditLogs = await query.ToArrayAsync();

			return Ok(auditLogs.Select(a =>
			{
				return new AuditLog
				{
					Id = a.Id,
					ChangedOn = a.ChangedOn,
					ChangedBy = a.ChangedBy == null
								? null
								: JsonSerializer.Deserialize<ChangedByUser>(a.ChangedBy, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
					Type = a.Type,
					Action = a.Action,
					Source = a.Source,
					OldValue = a.OldValue == null ? null : JObject.Parse(a.OldValue),
					NewValue = a.NewValue == null ? null : JObject.Parse(a.NewValue)
				};
			})
				.OrderByDescending(a => a.ChangedOn));
		}

		private static bool TryParseDateParameter(string name, string value, out DateTime? date, out string error)
		{
			date = null;
			error = null;

			if (string.IsNullOrWhiteSpace(value))
				return true;

			if (!DateTime.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
			{
				string example = new DateTime(2025, 12, 1).ToString(DateFormat, CultureInfo.InvariantCulture);
				error = $"Invalid '{name}' value '{value}'. Expected format is {DateFormat} (for example {example}).";
				return false;
			}

			date = parsed;
			return true;
		}
	}
}
