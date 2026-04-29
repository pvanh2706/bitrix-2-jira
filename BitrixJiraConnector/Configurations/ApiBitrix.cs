using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Configurations
{
	// Link document API: https://training.bitrix24.com/rest_help/
	class ApiBitrix
	{
		public static string API_GET_DEAL_FIELD = "/crm.deal.fields";
		public static string API_GET_DEAL_DATA = "/crm.deal.get/?id=";
		public static string API_GET_USER_DATA = "/user.get/?ID=";
		public static string API_GET_CONTACT_DATA = "/crm.contact.get/?id=";
		public static string API_GET_PRODUCT_DATA = "/crm.deal.productrows.get/?id=";
		public static string API_GET_LIST_DEAL_DATA = "/crm.deal.list/";
		public static string API_GET_LIST_PRODUCT_DATA = "/crm.product.list/";
		public static string API_GET_PRODUCT_BY_ID_DATA = "/crm.product.get/?id=";
		public static string API_GET_LIST_SECTION_DATA = "/crm.productsection.list/";
		// Lấy Danh sách thông tin liên hệ theo DealID
		public static string API_GET_LIST_CONTACT_OF_DEAL = "/crm.deal.contact.items.get/?id=";
	}
}
