using BitrixJiraConnector.Configurations;
using BitrixJiraConnector.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Services
{
	class BitrixJiraDBServices
	{
		private static readonly AppDbContext _dbContext = new AppDbContext();
		public static async Task<List<BitrixJiraInfo>> GetListDealCreatedByDay(string beginDateSearch)
		{
			// List<BitrixJiraInfo> result = _dbContext.BitrixJiraInfoes.Where(i => i.Bitrix_DateSearch == beginDateSearch).ToList();
			List<BitrixJiraInfo> result = _dbContext.BitrixJiraInfoes.Where(i => i.Bitrix_DateSearch.CompareTo(beginDateSearch) >= 0).ToList();
			
			return result;
		}
		public static async Task InsertData(BitrixJiraInfo bitrixJiraDB)
		{
			BitrixJiraInfo bitrixJiraInfo = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == bitrixJiraDB.Bitrix_DealID);
			if (bitrixJiraInfo == null)
			{
				_dbContext.BitrixJiraInfoes.Add(bitrixJiraDB);
				_dbContext.SaveChanges();
			}
			
		}
		public static async Task<BitrixJiraInfo> GetDealByDealID(int dealID)
		{
			BitrixJiraInfo result = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == dealID);
			return result;
		}

		public static async Task ResetNumberCheckError(int dealID)
		{
			BitrixJiraInfo result = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == dealID);
			if (result != null)
			{
				result.NumberCheckError = 0;
				_dbContext.SaveChanges();
			}
		}

		public static async Task IncreaseNumberCheckError(int dealID)
		{
			BitrixJiraInfo result = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == dealID);
			if (result != null)
			{
				result.NumberCheckError += 1;
				_dbContext.SaveChanges();
			}
		}
		

		public static async Task SetBitrixCreateIssSuccess(int dealID, string urlIssuesCreated)
		{
			BitrixJiraInfo result = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == dealID);
			if (result != null)
			{
				result.HaveError = 0;
				result.Jira_Link = urlIssuesCreated;
				_dbContext.SaveChanges();
			}
		}
		/// <summary>
		/// "2024/06/18".CompareTo("2024/06/17") => 1
		/// "2024/06/18".CompareTo("2024/06/18") => 0
		/// "2024/06/18".CompareTo("2024/06/19") => -1
		/// </summary>
		/// <param name="dealID"></param>
		/// <param name="FromDate"></param>
		/// <param name="ToDate"></param>
		/// <returns></returns>
		public static async Task<List<BitrixJiraInfo>> SearchDealID(int? dealID, DateTime FromDate, DateTime ToDate)
		{
			// Chuyển đổi FromDate và ToDate từ DateTimePicker sang chuỗi "yyyy-MM-dd"
			string fromDateStr = FromDate.ToString("yyyy/MM/dd");
			string toDateStr = ToDate.ToString("yyyy/MM/dd");

			// Thực hiện truy vấn LINQ
			List<BitrixJiraInfo> result = _dbContext.BitrixJiraInfoes
				.Where(i =>
					(dealID == null || i.Bitrix_DealID == dealID)
					&& i.Bitrix_DateSearch.CompareTo(fromDateStr) >= 0
					&& i.Bitrix_DateSearch.CompareTo(toDateStr) <= 0
				).ToList();
			return result;
		}

		public static async Task AddLogException(ExceptionLog exceptionLog)
		{
			_dbContext.ExceptionLog.Add(exceptionLog);
			_dbContext.SaveChanges();
		} 

		public static async Task SaveCauHinhQuetDeal(int? quetLaiSau, int? guiLaiEmailSau, int? soNgayQuet)
		{
			ConfigData itemQuetLaiSau = _dbContext.ConfigData.FirstOrDefault(i => i.KeyConfig == "QuetLaiSau");
			if (itemQuetLaiSau != null)
			{
				itemQuetLaiSau.ValueConfig = quetLaiSau.ToString();
			}

			ConfigData itemGuiLaiEmailSau = _dbContext.ConfigData.FirstOrDefault(i => i.KeyConfig == "GuiLaiEmailSau");
			if (itemGuiLaiEmailSau != null)
			{
				itemGuiLaiEmailSau.ValueConfig = guiLaiEmailSau.ToString();
			}
			ConfigData itemSoNgayQuet = _dbContext.ConfigData.FirstOrDefault(i => i.KeyConfig == "SoNgayQuet");
			if (itemSoNgayQuet != null)
			{
				itemSoNgayQuet.ValueConfig = soNgayQuet.ToString();
			}
			_dbContext.SaveChanges();
		}

		public static async Task<List<ConfigData>> GetConfigDatasAsync()
		{
			List<ConfigData> result = _dbContext.ConfigData.ToList();
			return result;
		}

		public static async Task UpdateDateTimeSendMail(int dealID, int SaveTimeSendMailTo)
		{
			BitrixJiraInfo result = _dbContext.BitrixJiraInfoes.FirstOrDefault(i => i.Bitrix_DealID == dealID);
			if (result != null)
			{
				long currentUnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
				if (SaveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_FIRST) result.DateTimeSendMailFirst = currentUnixTimestamp;
				if (SaveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_SECOND) result.DateTimeSendMailSecond = currentUnixTimestamp;
				if (SaveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD) result.DateTimeSendMailThird = currentUnixTimestamp;
				result.LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				_dbContext.SaveChanges();
			}
		}
	}
}
