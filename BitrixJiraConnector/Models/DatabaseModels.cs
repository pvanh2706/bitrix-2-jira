using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Models
{
	public class BitrixJiraInfo
	{
		[Key]
		public int Bitrix_DealID { get; set; }
		public string Bitrix_DealLink { get; set; }
		public string Bitrix_DateSearch { get; set; }
		public int IsSendDataToJira { get; set; }
		public int IsSendEmail { get; set; }
		public string Jira_Link { get; set; }
		public int HaveError { get; set; }
		public string ErrorInfo { get; set; }
		public string DateTimeCreated { get; set; }
		public int  NumberCheckError { get; set; }
		public long? DateTimeSendMailFirst { get; set; }
		public long? DateTimeSendMailSecond { get; set; }
		public long? DateTimeSendMailThird { get; set; }
		public string LastChangeData { get; set; }
	}
	public class ConfigData
	{
		[Key]
		public int ID { get; set; }
		public string KeyConfig { get; set; }
		public string ValueConfig { get; set; }
		public string Description { get; set; }
	}

	/// <summary>
	/// Id: Khóa chính tự động tăng để xác định duy nhất mỗi log.
	/// ExceptionMessage: Thông báo lỗi từ ngoại lệ.
	/// StackTrace: Stack trace của ngoại lệ, giúp xác định vị trí lỗi trong mã nguồn.
	/// ExceptionType: Loại của ngoại lệ (ví dụ: System.NullReferenceException).
	/// Source: Nguồn hoặc lớp gây ra ngoại lệ.
	/// LoggedAt: Thời gian xảy ra lỗi, lưu dưới dạng chuỗi ISO 8601 (YYYY-MM-DD HH:MM:SS)
	/// </summary>
	public class ExceptionLog
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int DealID { get; set; }
		public string ExceptionMessage { get; set; }
		public string StackTrace { get; set; }
		public string ExceptionType { get; set; }
		public string Source { get; set; }
		[Required]
		public string LoggedAt { get; set; }  // Store as TEXT in the format "yyyy-MM-dd HH:mm:ss"
	}

	public class CheckSendEmail
	{
		public bool IsSendMail { get; set; }
		public int SaveTimeSendMailTo { get; set; }
	}
}
