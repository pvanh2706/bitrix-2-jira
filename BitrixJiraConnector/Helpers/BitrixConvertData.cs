using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Helpers
{
	class BitrixConvertData
	{
		/// <summary>
		/// Lay thong tin key cua 1 array trong json 
		/// VD: Voi json sau muon lay ra gia tri VALUE thi dung: GetValueFromJson(json1, "UF_CRM_1531008031", "618", "VALUE");
		/*{
			"result": 
			{
				"UF_CRM_1531008031": 
				{
					"items": [
						{
							"ID": "618",
							"VALUE": "ezFolio"
						},
						{
							"ID": "619",
							"VALUE": "ezCloudHotel"
						}
					]
				}
			}
		}*/
		/// </summary>
		/// <param name="json"></param>
		/// <param name="key"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string GetValueItemInArrayFromJson(string json, string key, string itemId, string itemKey)
		{
			var jobject = JObject.Parse(json);
			var items = jobject["result"]?[key]?["items"];
			if (items != null)
			{
				foreach (var item in items)
				{
					if (item["ID"]?.ToString() == itemId)
					{
						return item[itemKey]?.ToString();
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Lay thong tin key cua Json 1 level
		/// VD: Voi json sau muon lay ra gia tri ID thi dung: GetValueFromJson(json2, "result", "ID")
		/*{
			   "result": {
				  "ID": "36792",
				  "TITLE": "Kỹ thuật test chức năng",
				  "TYPE_ID": "SALE"
				}
			}
		*/
		/// </summary>
		/// <param name="json"></param>
		/// <param name="key"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public static string GetValueFromArrayJson(JObject jobject, string key, dynamic id)
		{
			var items = jobject["result"];
			if (items != null)
			{
				foreach (var item in items)
				{
					if (item["ID"]?.ToString() == id.ToString())
					{
						// return item["VALUE"]?.ToString();
						return item[key]?.ToString();
					}
				}
			}
			return null;
		}
		public static string GetValueFromObjectJson(JObject jobject, string key, dynamic id)
		{
			var item = jobject["result"];
			if (item != null && item["ID"]?.ToString() == id.ToString())
			{
				return item[key]?.ToString();
			}
			return null;
		}
		/// <summary>
		/// Lay thong tin value trong item mang
		/// GetValueItemInArrayFromJson(json, "UF_CRM_1531008031", "618")
		/// </summary>
		/// <param name="json"></param>
		/// <param name="key"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string GetValueItemInArrayFromJson(JObject jobject, string key, dynamic id)
		{
			var items = jobject["result"]?[key]?["items"];
			if (items != null)
			{
				foreach (var item in items)
				{
					if (item["ID"]?.ToString() == id.ToString())
					{
						return item["VALUE"]?.ToString();
					}
				}
			}
			return null;
		}
	}
}
