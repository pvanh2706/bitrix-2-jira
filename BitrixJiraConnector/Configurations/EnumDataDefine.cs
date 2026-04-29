using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Configurations
{
	/// <summary>
	/// Loại Deal
	/// </summary>
	public enum LOAI_DEAL
	{
		TRIEN_KHAI_MOI = 3095,
		TRIEN_KHAI_BO_SUNG = 3096,
		NGUNG_HUY_DICH_VU = 3097,
		CHUYEN_DOI_DICH_VU = 3098,
		CAP_KEY = 3099,
		HO_TRO_KHAC = 3100,
	}
	/// <summary>
	/// Thứ tự thời gian gửi mail thông báo lỗi khi tạo iss
	/// </summary>
	public enum SAVE_TIME_SEND_MAIL_TO
	{
		NO_SAVE = 0,
		TIME_SEND_MAIL_FIRST = 1,
		TIME_SEND_MAIL_SECOND = 2,
		TIME_SEND_MAIL_THIRD = 3,
	}
	/// <summary>
	/// Loại Deal trong Bitrix
	/// </summary>
	public enum TYPE_PIPE_LINE
	{
		SALE = 0,
		RENEWAL = 3,
		CROSS_SALE = 4,
	}
}
