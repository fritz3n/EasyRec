using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EasyRec.Configuration
{
	class KeyConverter : JsonConverter<Keys>
	{
		public override Keys ReadJson(JsonReader reader, Type objectType, Keys existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			string token = reader.Value as string ?? reader.Value.ToString();
			string stripped = Regex.Replace(token, @"<[^>]+>", string.Empty);
			if (Enum.TryParse(stripped, out Keys result))
			{
				return result;
			}
			return default;
		}

		public override void WriteJson(JsonWriter writer, Keys value, JsonSerializer serializer) => writer.WriteValue(value.ToString());
	}
}
