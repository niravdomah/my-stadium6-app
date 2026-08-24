using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.JsonConverters;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Controls.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Controls.Controllers
{
	[ApiController]
	[Area("Controls")]
	[Route("api/[area]/[controller]/[action]")]
	public class DataGridController : ControllerBase
	{
		private readonly IDataGridFilter dataGridFilter;
		private readonly Config config;

		public DataGridController(IDataGridFilter dataGridFilter, IOptionsSnapshot<Config> configOptions)
		{
			this.dataGridFilter = dataGridFilter;
			this.config = configOptions.Value;
		}

		// POST: api/Controls/DataGrid/SetData?controlId=[controlId]&isChunkStart=[isChunkStart]
		[HttpPost]
		public IActionResult SetData(string controlId, bool isChunkStart, SetDataInfo setDataInfo)
		{
			if (isChunkStart)
			{
				SetSessionData(controlId, setDataInfo.JsonData, setDataInfo.ColumnNameHeaderMap);
			}
			else
			{
				GetSessionData(controlId, out string existingJsonData, out _);

				var existingJsonArray = JArray.Parse(existingJsonData);
				var newJsonArray = JArray.Parse(setDataInfo.JsonData);
				var mergedArray = new JArray(existingJsonArray.Concat(newJsonArray));

				SetSessionData(controlId, mergedArray.ToString());
			}

			return NoContent();
		}

		// POST: api/Controls/DataGrid/Search?controlId=[controlId]
		[HttpPost]
		public IActionResult Search(string controlId, [FromBody] string searchQuery)
		{
			GetSessionData(controlId, out string jsonData, out var columnNameHeaderMap);
			var data = string.IsNullOrWhiteSpace(jsonData) ? [] : JArray.Parse(jsonData);

			if (string.IsNullOrEmpty(searchQuery))
				return Ok(data);

			this.dataGridFilter.Initialise(
				searchQuery,
				columnNameHeaderMap,
				(rowIndex, columnName) => ((JObject)data[rowIndex]).GetValue(columnName)?.ToObject<object>());

			var filteredData = new List<JToken>();
			for (int i = 0; i < data.Count; i++)
			{
				if (this.dataGridFilter.MeetsFilterCriteria(i))
					filteredData.Add(data[i].ToString(Formatting.None));
			}

			return Ok(filteredData);
		}

		// GET: api/Controls/DataGrid/Export?controlId=[controlId]
		[HttpGet]
		public IActionResult Export(string controlId)
		{
			GetSessionData(controlId, out string jsonData, out var columnNameHeaderMap);

			DataTable data;
			if (string.IsNullOrWhiteSpace(jsonData) || jsonData == "[]")
			{
				data = new DataTable();
				foreach (string column in columnNameHeaderMap.Keys)
					data.Columns.Add(column);
			}
			else
			{
				data = JsonConvert.DeserializeObject<DataTable>(jsonData, new IsoDateTimeConverter { DateTimeFormat = JsonSerializerFormats.DateTime });
			}

			foreach (var column in data.Columns.Cast<DataColumn>())
			{
				column.Caption = columnNameHeaderMap.TryGetValue(column.ColumnName, out string headerName) ? headerName : string.Empty;
			}

			var memoryStream = new MemoryStream();

			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Worksheet 1");

				for (int columnIndex = 0; columnIndex < data.Columns.Count; columnIndex++)
					worksheet.Cell(1, columnIndex + 1).Value = data.Columns[columnIndex].Caption;

				if (data.Rows.Count != 0)
					worksheet.Cell(2, 1).InsertData(data);

				SetColumnFormats(data, worksheet);
				workbook.SaveAs(memoryStream);
			}
			memoryStream.Position = 0;

			return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		}

		private void SetColumnFormats(DataTable dataTable, IXLWorksheet worksheet)
		{
			if (dataTable.Rows.Count != 0)
			{
				var firstRow = dataTable.Rows[0];
				for (int columnIndex = 0; columnIndex < firstRow.ItemArray.Length; columnIndex++)
				{
					if (firstRow[columnIndex] is DateTime)
						worksheet.Column(columnIndex + 1).Style.NumberFormat.Format = this.config.DateFormat;
				}
			}
		}

		private void SetSessionData(string controlId, string jsonData, IDictionary<string, string> columnNameHeaderMap = null)
		{
			HttpContext.Session.SetString($"{controlId}_JsonData", jsonData);

			if (columnNameHeaderMap != null)
				HttpContext.Session.SetString($"{controlId}_ColumnNameHeaderMap", JsonConvert.SerializeObject(columnNameHeaderMap));
		}

		private void GetSessionData(string controlId, out string jsonData, out Dictionary<string, string> columnNameHeaderMap)
		{
			jsonData = HttpContext.Session.GetString($"{controlId}_JsonData");

			string columnNameHeaderMapJson = HttpContext.Session.GetString($"{controlId}_ColumnNameHeaderMap");
			columnNameHeaderMap = string.IsNullOrWhiteSpace(columnNameHeaderMapJson) ? [] :
				JsonConvert.DeserializeObject<Dictionary<string, string>>(columnNameHeaderMapJson);
		}
	}
}
