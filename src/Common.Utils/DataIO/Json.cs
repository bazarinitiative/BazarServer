using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Common.Utils
{
	public static class Json
	{
		public class OrderedContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
			{
				return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Serialize(object? obj, bool indent = false)
		{
			return JsonConvert.SerializeObject(obj, indent ? Formatting.Indented : Formatting.None);
		}

		public static object? Deserialize(string str)
		{
			return JsonConvert.DeserializeObject(str);
		}

		public static JObject DeserializeJObject(string str)
		{
			var obj = JObject.Parse(str);
			return obj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="str"></param>
		/// <param name="nullStringToEmpty">convert null string to string.empty</param>
		/// <returns></returns>
		public static T? Deserialize<T>(string str, bool nullStringToEmpty = false)
		{
			if (nullStringToEmpty)
			{
				JsonConverter converter = new NullToEmptyStringConverter();
				return JsonConvert.DeserializeObject<T>(str, converter);

			}
			else
			{
				return JsonConvert.DeserializeObject<T>(str);
			}
		}

		class NullToEmptyStringConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return objectType == typeof(string);
			}

			public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
			{
				var str = reader.Value;
				return str ?? "";
			}

			public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
			{
				if (value == null)
				{
					writer.WriteValue("");
				}
			}
		}
	}
}
