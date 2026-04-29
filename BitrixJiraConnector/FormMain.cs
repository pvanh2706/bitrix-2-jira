using BitrixJiraConnector.Configurations;
using BitrixJiraConnector.Models;
using BitrixJiraConnector.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BitrixJiraConnector
{
	public partial class FormMain : Form
	{
		private static CancellationTokenSource _cts;
		private Task _runningTask;
		public FormMain()
		{
			InitializeComponent();
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
			
		}

		private async Task LoadConfig()
		{
			ConfigJiraBitrix.configDatas = await BitrixJiraDBServices.GetConfigDatasAsync();
			string quetSau = ConfigJiraBitrix.configDatas.FirstOrDefault(i => i.KeyConfig == "QuetLaiSau")?.ValueConfig;
			string guiEmailSau = ConfigJiraBitrix.configDatas.FirstOrDefault(i => i.KeyConfig == "GuiLaiEmailSau")?.ValueConfig;
			string soNgayQuet = ConfigJiraBitrix.configDatas.FirstOrDefault(i => i.KeyConfig == "SoNgayQuet")?.ValueConfig;


			textBoxQuetSau.Text = quetSau;
			textBoxGuiEmailSau.Text = guiEmailSau;
			textBoxSoNgayQuet.Text = soNgayQuet;

			ConfigJiraBitrix.quetSauPhut = int.Parse(quetSau);
			ConfigJiraBitrix.soNgayQuet = int.Parse(soNgayQuet);
			ConfigJiraBitrix.guiLaiEmailSau = int.Parse(guiEmailSau);
			ConfigJiraBitrix.haveReloadConfig = false;
		}
		#region Tab Trang chủ
		private async void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				_cts = new CancellationTokenSource();
				await Task.Run(() => { writeLogtoForm("Bắt đầu chạy chương trình kết nối Bitrix và jira"); });
				await LoadConfig();
				_runningTask = MainAsync(_cts.Token);
				try
				{
					await _runningTask;
				}
				catch (OperationCanceledException)
				{
					await Task.Run(() => { writeLogtoForm("Kết thúc chạy chương trình kết nối Bitrix và jira"); });
				}
			}
			catch (Exception ex)
			{
				ExceptionLog exceptionLog = new ExceptionLog();
				exceptionLog.DealID = 0;
				exceptionLog.ExceptionMessage = ex.Message;
				exceptionLog.StackTrace = ex.StackTrace ?? "";
				exceptionLog.ExceptionType = ex.GetType().ToString();
				exceptionLog.Source = ex.Source ?? "";
				exceptionLog.LoggedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

				await BitrixJiraDBServices.AddLogException(exceptionLog);
				await Task.Run(() => { writeLogtoForm("Error load form:" + ex.Message); });
			}
			
		}

		private void btnEnd_Click(object sender, EventArgs e)
		{
			if (_cts != null)
			{
				_cts.Cancel();
			}
		}
		private async Task MainAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			// string beginDateSearch = DateTime.Now.ToString("yyyy/MM/dd");
			int numDaySearchBack = ConfigJiraBitrix.soNgayQuet;
			string beginDateSearch = DateTime.Now.AddDays(numDaySearchBack * (-1)).ToString("yyyy/MM/dd");
			List<BitrixJiraInfo> dealCreatedByDays = await BitrixJiraDBServices.GetListDealCreatedByDay(beginDateSearch);
			// Danh sách Deal đã tạo iss thành công và iss có lỗi nhưng không cần gửi Email
			List<BitrixJiraInfo> dealProcesseds = dealCreatedByDays
				.Where(i => i.HaveError == 0 || (i.HaveError == 1 && i.IsSendEmail == 0) || (i.DateTimeSendMailThird != null))
				.ToList();
			if (dealProcesseds != null && dealProcesseds.Any()) ConfigJiraBitrix.dealIDProcessed = dealProcesseds.Select(i => i.Bitrix_DealID).ToList();

			int numMinutes = ConfigJiraBitrix.quetSauPhut;
			int millisecondsDelay = numMinutes * 60 * 1000;
			while (true)
			{
				if (ConfigJiraBitrix.haveReloadConfig) await LoadConfig();
				await BitrixServices.getDeals_Bitrix(token);
				writeLogtoForm("==================Quét và xử lý xong===============");
				await Task.Delay(millisecondsDelay, token);
			}



			// Lấy giá trị của CustomField
			//JObject dataCustomField_Bitrix = await BitrixServices.getValueFiledCustom_Bitrix();
			//await BitrixServices.getDataDealBitrix_And_CreateIssues(36792, dataCustomField_Bitrix, beginDateSearch, false); // 36792
			//writeLogtoForm("==================Quét và xử lý xong===============");
		}
		public void writeLogtoForm(string txtLog)
		{
			string log = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + txtLog;
			if (richTextBoxLog.InvokeRequired)
			{
				richTextBoxLog.Invoke(new Action(() => {
					if (richTextBoxLog.Lines.Length == 5) richTextBoxLog.Clear();
					richTextBoxLog.AppendText(log);
					richTextBoxLog.AppendText(Environment.NewLine);
				}));
			}
			else
			{
				if (richTextBoxLog.Lines.Length == 5) richTextBoxLog.Clear();
				richTextBoxLog.AppendText(log);
				richTextBoxLog.AppendText(Environment.NewLine);
			}
		}
		#endregion

		#region Tab Danh sách Deal
		private async void buttonSearchDeal_Click(object sender, EventArgs e)
		{
			DateTime selectedFromDateDate = dateTimePicker_FromDate.Value.Date;
			DateTime selectedToDate = dateTimePicker_ToDate.Value.Date;
			if (selectedFromDateDate > selectedToDate)
			{
				MessageBox.Show("Từ ngày (quét) không thể lớn hơn Đến ngày (quét).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			int? dealID = null;
			if (!string.IsNullOrEmpty(textBoxDealID.Text))
			{
				if (int.TryParse(textBoxDealID.Text, out int result))
				{
					dealID = result; // Gán giá trị int cho biến nullable int
				}
				else
				{
					MessageBox.Show("Vui lòng nhập số nguyên hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}

			List<BitrixJiraInfo> dealList = await BitrixJiraDBServices.SearchDealID(dealID, selectedFromDateDate, selectedToDate);
			dataGridViewDealList.DataSource = dealList;

			// MessageBox.Show($"Selected Date: {selectedFromDateDate.ToString("yyyy-MM-dd")}");
		}
		private void textBoxDealID_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Chỉ cho phép nhập số từ 0 đến 9 và phím Backspace
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true; // Ngăn không cho ký tự được nhập vào TextBox
			}
		}

		#endregion

		#region Tab Cấu hình
		private void textBoxQuetSau_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Chỉ cho phép nhập số từ 0 đến 9 và phím Backspace
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true; // Ngăn không cho ký tự được nhập vào TextBox
			}
		}

		private void textBoxGuiEmailSau_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Chỉ cho phép nhập số từ 0 đến 9 và phím Backspace
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true; // Ngăn không cho ký tự được nhập vào TextBox
			}
		}

		private void textBoxSoNgayQuet_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Chỉ cho phép nhập số từ 0 đến 9 và phím Backspace
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true; // Ngăn không cho ký tự được nhập vào TextBox
			}
		}

		private async void buttonLuuCauHinhQuetDeal_Click(object sender, EventArgs e)
		{
			int? quetLaiSau = null;
			int? guiLaiEmailSau = null;
			int? soNgayQuet = null;

			if (!string.IsNullOrEmpty(textBoxQuetSau.Text))
			{
				if (int.TryParse(textBoxQuetSau.Text, out int result))
				{
					quetLaiSau = result; // Gán giá trị int cho biến nullable int
				}
				else
				{
					MessageBox.Show("Vui lòng nhập số nguyên hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			else
			{
				MessageBox.Show("Quét deal sau (phút) không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (!string.IsNullOrEmpty(textBoxGuiEmailSau.Text))
			{
				if (int.TryParse(textBoxGuiEmailSau.Text, out int result))
				{
					guiLaiEmailSau = result; // Gán giá trị int cho biến nullable int
				}
				else
				{
					MessageBox.Show("Vui lòng nhập số nguyên hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			else
			{
				MessageBox.Show("Gửi lại Email sau (lần lỗi) không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (!string.IsNullOrEmpty(textBoxSoNgayQuet.Text))
			{
				if (int.TryParse(textBoxSoNgayQuet.Text, out int result))
				{
					soNgayQuet = result; // Gán giá trị int cho biến nullable int
				}
				else
				{
					MessageBox.Show("Vui lòng nhập số nguyên hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			else
			{
				MessageBox.Show("Số ngày quét không được để trống", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			await BitrixJiraDBServices.SaveCauHinhQuetDeal(quetLaiSau, guiLaiEmailSau, soNgayQuet);
			ConfigJiraBitrix.haveReloadConfig = true;
			MessageBox.Show("Lưu thành công");
		}
		#endregion
	}
}
