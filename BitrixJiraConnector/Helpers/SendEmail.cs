using BitrixJiraConnector.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BitrixJiraConnector.Helpers
{
	class PushNotify
	{
		public const string FromAddress = ConfigJiraBitrix.MailInfo_FromAddress;
		public const string MatKhauUngDungMail = ConfigJiraBitrix.MailInfo_password;
		public const string ToAddress = ConfigJiraBitrix.MailInfo_ToAddress;
		public static void SendEmail(string subject, string body, string toAddress)
		{
			string toAddressEmail = string.IsNullOrEmpty(toAddress) ? ToAddress : toAddress;
			// string toAddressEmail = ToAddress;
			MailMessage mail = new MailMessage(FromAddress, toAddressEmail, subject, body);
			mail.CC.Add(ToAddress);
			SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential(FromAddress, MatKhauUngDungMail),
				EnableSsl = true,
			};

			smtpClient.Send(mail);
		}
		public static async Task SendEmailSendGrid(string subject, string body, string toAddress, bool isSendEmailToAdminBitrixJira = false)
		{
			string toAddressEmail = string.IsNullOrEmpty(toAddress) ? ToAddress : toAddress;
			// string toAddressEmail = ToAddress;
			var client = new SendGridClient(ConfigJiraBitrix.SendGrid_API_Key);
			var from = new EmailAddress(ConfigJiraBitrix.SendGrid_FromAddress, ConfigJiraBitrix.SendGrid_FromName);
			var to = new EmailAddress(toAddressEmail);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
			var cc = new EmailAddress(ToAddress);
			msg.AddCc(cc);
			if (isSendEmailToAdminBitrixJira)
			{
				var ccAdminBitrix = new EmailAddress(ConfigJiraBitrix.MailInfo_AdminBitrixEmail);
				var ccAdminJira = new EmailAddress(ConfigJiraBitrix.MailInfo_AdminJiraEmail);
				msg.AddCc(ccAdminBitrix);
				msg.AddCc(ccAdminJira);
			}
			await client.SendEmailAsync(msg);
		}
	}
}
