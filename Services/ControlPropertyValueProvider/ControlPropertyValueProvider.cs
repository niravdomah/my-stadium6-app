using System;
using System.Collections.Generic;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.ControlPropertyValueProvider
{
	public class ControlPropertyValueProvider : IControlPropertyValueProvider
	{
		private readonly IDictionary<string, IDictionary<string, object>> propertiesByControlId;
		private readonly ISettingStore settingStore;
		private readonly IFolderPathService folderPathService;

		public ControlPropertyValueProvider(ISettingStore settingStore, IFolderPathService folderPathService)
		{
			this.settingStore = settingStore;
			this.folderPathService = folderPathService;

			this.propertiesByControlId = new Dictionary<string, IDictionary<string, object>>
			{
				// start of generated code

				// end of generated code
			};
		}

		public T GetPropertyValue<T>(string controlId, string propertyName)
		{
			int repeaterSuffixIndex = controlId.IndexOf("-item");
			controlId = repeaterSuffixIndex < 0
						? controlId
						: controlId.Substring(0, repeaterSuffixIndex);

			if (this.propertiesByControlId.TryGetValue(controlId, out var properties))
			{
				if (properties.TryGetValue(propertyName, out object propertyValue))
				{
					return (T)propertyValue;
				}
			}

			throw new Exception("Invalid parameter value.");
		}
	}
}
