using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BitrixJiraConnector.Helpers
{
	class CheckRequireFieldBitrix
	{
		//public static BitrixDataDealAPI_Result CheckRequireField_Bitrix()
		//{

		//}
		public static List<string> GetKeysWithNullOrEmptyValues(dynamic dataDealApi, List<string> keys)
		{
			List<string> keysWithNullOrEmptyValues = new List<string>();

			// Chuyển đổi dynamic thành JObject
			JObject data = dataDealApi as JObject;

			foreach (var key in keys)
			{
				try
				{
					// Kiểm tra xem key có tồn tại trong JObject không
					var value = data[key];

					if (value == null || value.Type == JTokenType.Null)
					{
						keysWithNullOrEmptyValues.Add(key);
						continue;
					}

					// Kiểm tra nếu giá trị là chuỗi
					if (value.Type == JTokenType.String)
					{
						if (string.IsNullOrEmpty(value.ToString()))
						{
							keysWithNullOrEmptyValues.Add(key);
							continue;
						}
					}

					// Kiểm tra nếu giá trị là mảng
					if (value.Type == JTokenType.Array)
					{
						var arrayValue = value as JArray;
						if (arrayValue != null && arrayValue.Count == 0)
						{
							keysWithNullOrEmptyValues.Add(key);
							continue;
						}
					}

					// Kiểm tra nếu giá trị là một đối tượng
					if (value.Type == JTokenType.Object)
					{
						var objValue = value as JObject;
						if (objValue != null && !objValue.HasValues)
						{
							keysWithNullOrEmptyValues.Add(key);
							continue;
						}
					}
				}
				catch (Exception ex)
				{
					// Xử lý lỗi nếu có
					Console.WriteLine($"Error: {ex.Message}");
					keysWithNullOrEmptyValues.Add(key); // Thêm key vào danh sách lỗi
				}
			}

			return keysWithNullOrEmptyValues;
		}

		public static List<string> GetValuesForKeys(Dictionary<string, string> keyValuePairs, List<string> keys)
		{
			List<string> values = new List<string>();

			foreach (var key in keys)
			{
				if (keyValuePairs.TryGetValue(key, out var value))
				{
					values.Add(value);
				}
				else
				{
					values.Add(""); // Thêm chuỗi rỗng nếu key không tồn tại trong từ điển
				}
			}

			return values;
		}
	}
}
