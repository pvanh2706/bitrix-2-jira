namespace BitrixJiraConnector
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tabPageHome = new System.Windows.Forms.TabPage();
			this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
			this.btnEnd = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.tabPageDealList = new System.Windows.Forms.TabPage();
			this.labelSearch_DealID = new System.Windows.Forms.Label();
			this.labelSearch_ToDate = new System.Windows.Forms.Label();
			this.labelSearch_FromDate = new System.Windows.Forms.Label();
			this.dateTimePicker_ToDate = new System.Windows.Forms.DateTimePicker();
			this.textBoxDealID = new System.Windows.Forms.TextBox();
			this.buttonSearchDeal = new System.Windows.Forms.Button();
			this.dateTimePicker_FromDate = new System.Windows.Forms.DateTimePicker();
			this.dataGridViewDealList = new System.Windows.Forms.DataGridView();
			this.Config = new System.Windows.Forms.TabPage();
			this.groupBoxCauHinhQuet = new System.Windows.Forms.GroupBox();
			this.buttonLuuCauHinhQuetDeal = new System.Windows.Forms.Button();
			this.textBoxSoNgayQuet = new System.Windows.Forms.TextBox();
			this.textBoxGuiEmailSau = new System.Windows.Forms.TextBox();
			this.textBoxQuetSau = new System.Windows.Forms.TextBox();
			this.labelSoNgayQuet = new System.Windows.Forms.Label();
			this.labelGuiEmailSau = new System.Windows.Forms.Label();
			this.labelQuetDealSau = new System.Windows.Forms.Label();
			this.tabControlMain.SuspendLayout();
			this.tabPageHome.SuspendLayout();
			this.tabPageDealList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewDealList)).BeginInit();
			this.Config.SuspendLayout();
			this.groupBoxCauHinhQuet.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(994, 24);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
			// 
			// tabControlMain
			// 
			this.tabControlMain.Controls.Add(this.tabPageHome);
			this.tabControlMain.Controls.Add(this.tabPageDealList);
			this.tabControlMain.Controls.Add(this.Config);
			this.tabControlMain.Location = new System.Drawing.Point(12, 0);
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			this.tabControlMain.Size = new System.Drawing.Size(970, 515);
			this.tabControlMain.TabIndex = 5;
			// 
			// tabPageHome
			// 
			this.tabPageHome.Controls.Add(this.richTextBoxLog);
			this.tabPageHome.Controls.Add(this.btnEnd);
			this.tabPageHome.Controls.Add(this.btnStart);
			this.tabPageHome.Location = new System.Drawing.Point(4, 24);
			this.tabPageHome.Name = "tabPageHome";
			this.tabPageHome.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageHome.Size = new System.Drawing.Size(962, 487);
			this.tabPageHome.TabIndex = 0;
			this.tabPageHome.Text = "Home";
			this.tabPageHome.UseVisualStyleBackColor = true;
			// 
			// richTextBoxLog
			// 
			this.richTextBoxLog.Location = new System.Drawing.Point(17, 20);
			this.richTextBoxLog.Name = "richTextBoxLog";
			this.richTextBoxLog.Size = new System.Drawing.Size(925, 422);
			this.richTextBoxLog.TabIndex = 9;
			this.richTextBoxLog.Text = "";
			// 
			// btnEnd
			// 
			this.btnEnd.Location = new System.Drawing.Point(867, 458);
			this.btnEnd.Name = "btnEnd";
			this.btnEnd.Size = new System.Drawing.Size(75, 23);
			this.btnEnd.TabIndex = 1;
			this.btnEnd.Text = "Kết thúc";
			this.btnEnd.UseVisualStyleBackColor = true;
			this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(776, 458);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "Bắt đầu";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// tabPageDealList
			// 
			this.tabPageDealList.Controls.Add(this.labelSearch_DealID);
			this.tabPageDealList.Controls.Add(this.labelSearch_ToDate);
			this.tabPageDealList.Controls.Add(this.labelSearch_FromDate);
			this.tabPageDealList.Controls.Add(this.dateTimePicker_ToDate);
			this.tabPageDealList.Controls.Add(this.textBoxDealID);
			this.tabPageDealList.Controls.Add(this.buttonSearchDeal);
			this.tabPageDealList.Controls.Add(this.dateTimePicker_FromDate);
			this.tabPageDealList.Controls.Add(this.dataGridViewDealList);
			this.tabPageDealList.Location = new System.Drawing.Point(4, 24);
			this.tabPageDealList.Name = "tabPageDealList";
			this.tabPageDealList.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDealList.Size = new System.Drawing.Size(962, 487);
			this.tabPageDealList.TabIndex = 1;
			this.tabPageDealList.Text = "Danh sách Deal";
			this.tabPageDealList.UseVisualStyleBackColor = true;
			// 
			// labelSearch_DealID
			// 
			this.labelSearch_DealID.AutoSize = true;
			this.labelSearch_DealID.Location = new System.Drawing.Point(511, 23);
			this.labelSearch_DealID.Name = "labelSearch_DealID";
			this.labelSearch_DealID.Size = new System.Drawing.Size(44, 15);
			this.labelSearch_DealID.TabIndex = 7;
			this.labelSearch_DealID.Text = "Deal ID";
			// 
			// labelSearch_ToDate
			// 
			this.labelSearch_ToDate.AutoSize = true;
			this.labelSearch_ToDate.Location = new System.Drawing.Point(255, 23);
			this.labelSearch_ToDate.Name = "labelSearch_ToDate";
			this.labelSearch_ToDate.Size = new System.Drawing.Size(92, 15);
			this.labelSearch_ToDate.TabIndex = 6;
			this.labelSearch_ToDate.Text = "Đến ngày (quét)";
			// 
			// labelSearch_FromDate
			// 
			this.labelSearch_FromDate.AutoSize = true;
			this.labelSearch_FromDate.Location = new System.Drawing.Point(17, 23);
			this.labelSearch_FromDate.Name = "labelSearch_FromDate";
			this.labelSearch_FromDate.Size = new System.Drawing.Size(84, 15);
			this.labelSearch_FromDate.TabIndex = 5;
			this.labelSearch_FromDate.Text = "Từ ngày (quét)";
			// 
			// dateTimePicker_ToDate
			// 
			this.dateTimePicker_ToDate.CustomFormat = "dd/MM/yyyy";
			this.dateTimePicker_ToDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateTimePicker_ToDate.Location = new System.Drawing.Point(353, 19);
			this.dateTimePicker_ToDate.Name = "dateTimePicker_ToDate";
			this.dateTimePicker_ToDate.Size = new System.Drawing.Size(111, 23);
			this.dateTimePicker_ToDate.TabIndex = 4;
			// 
			// textBoxDealID
			// 
			this.textBoxDealID.Location = new System.Drawing.Point(579, 19);
			this.textBoxDealID.Name = "textBoxDealID";
			this.textBoxDealID.Size = new System.Drawing.Size(100, 23);
			this.textBoxDealID.TabIndex = 3;
			this.textBoxDealID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxDealID_KeyPress);
			// 
			// buttonSearchDeal
			// 
			this.buttonSearchDeal.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.buttonSearchDeal.Location = new System.Drawing.Point(881, 19);
			this.buttonSearchDeal.Name = "buttonSearchDeal";
			this.buttonSearchDeal.Size = new System.Drawing.Size(75, 23);
			this.buttonSearchDeal.TabIndex = 2;
			this.buttonSearchDeal.Text = "Tìm kiếm";
			this.buttonSearchDeal.UseVisualStyleBackColor = true;
			this.buttonSearchDeal.Click += new System.EventHandler(this.buttonSearchDeal_Click);
			// 
			// dateTimePicker_FromDate
			// 
			this.dateTimePicker_FromDate.CustomFormat = "dd/MM/yyyy";
			this.dateTimePicker_FromDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateTimePicker_FromDate.Location = new System.Drawing.Point(105, 19);
			this.dateTimePicker_FromDate.Name = "dateTimePicker_FromDate";
			this.dateTimePicker_FromDate.Size = new System.Drawing.Size(114, 23);
			this.dateTimePicker_FromDate.TabIndex = 1;
			this.dateTimePicker_FromDate.Value = new System.DateTime(2024, 6, 19, 11, 43, 37, 0);
			// 
			// dataGridViewDealList
			// 
			this.dataGridViewDealList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewDealList.Location = new System.Drawing.Point(6, 66);
			this.dataGridViewDealList.Name = "dataGridViewDealList";
			this.dataGridViewDealList.RowTemplate.Height = 25;
			this.dataGridViewDealList.Size = new System.Drawing.Size(950, 415);
			this.dataGridViewDealList.TabIndex = 0;
			// 
			// Config
			// 
			this.Config.Controls.Add(this.groupBoxCauHinhQuet);
			this.Config.Location = new System.Drawing.Point(4, 24);
			this.Config.Name = "Config";
			this.Config.Padding = new System.Windows.Forms.Padding(3);
			this.Config.Size = new System.Drawing.Size(962, 487);
			this.Config.TabIndex = 2;
			this.Config.Text = "Cấu hình";
			this.Config.UseVisualStyleBackColor = true;
			// 
			// groupBoxCauHinhQuet
			// 
			this.groupBoxCauHinhQuet.Controls.Add(this.buttonLuuCauHinhQuetDeal);
			this.groupBoxCauHinhQuet.Controls.Add(this.textBoxSoNgayQuet);
			this.groupBoxCauHinhQuet.Controls.Add(this.textBoxGuiEmailSau);
			this.groupBoxCauHinhQuet.Controls.Add(this.textBoxQuetSau);
			this.groupBoxCauHinhQuet.Controls.Add(this.labelSoNgayQuet);
			this.groupBoxCauHinhQuet.Controls.Add(this.labelGuiEmailSau);
			this.groupBoxCauHinhQuet.Controls.Add(this.labelQuetDealSau);
			this.groupBoxCauHinhQuet.Location = new System.Drawing.Point(15, 17);
			this.groupBoxCauHinhQuet.Name = "groupBoxCauHinhQuet";
			this.groupBoxCauHinhQuet.Size = new System.Drawing.Size(341, 187);
			this.groupBoxCauHinhQuet.TabIndex = 0;
			this.groupBoxCauHinhQuet.TabStop = false;
			this.groupBoxCauHinhQuet.Text = "Cấu hình quét Deal";
			// 
			// buttonLuuCauHinhQuetDeal
			// 
			this.buttonLuuCauHinhQuetDeal.Location = new System.Drawing.Point(230, 150);
			this.buttonLuuCauHinhQuetDeal.Name = "buttonLuuCauHinhQuetDeal";
			this.buttonLuuCauHinhQuetDeal.Size = new System.Drawing.Size(98, 23);
			this.buttonLuuCauHinhQuetDeal.TabIndex = 6;
			this.buttonLuuCauHinhQuetDeal.Text = "Lưu";
			this.buttonLuuCauHinhQuetDeal.UseVisualStyleBackColor = true;
			this.buttonLuuCauHinhQuetDeal.Click += new System.EventHandler(this.buttonLuuCauHinhQuetDeal_Click);
			// 
			// textBoxSoNgayQuet
			// 
			this.textBoxSoNgayQuet.Location = new System.Drawing.Point(228, 95);
			this.textBoxSoNgayQuet.Name = "textBoxSoNgayQuet";
			this.textBoxSoNgayQuet.Size = new System.Drawing.Size(100, 23);
			this.textBoxSoNgayQuet.TabIndex = 5;
			this.textBoxSoNgayQuet.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxSoNgayQuet_KeyPress);
			// 
			// textBoxGuiEmailSau
			// 
			this.textBoxGuiEmailSau.Location = new System.Drawing.Point(228, 62);
			this.textBoxGuiEmailSau.Name = "textBoxGuiEmailSau";
			this.textBoxGuiEmailSau.Size = new System.Drawing.Size(100, 23);
			this.textBoxGuiEmailSau.TabIndex = 4;
			this.textBoxGuiEmailSau.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGuiEmailSau_KeyPress);
			// 
			// textBoxQuetSau
			// 
			this.textBoxQuetSau.Location = new System.Drawing.Point(228, 29);
			this.textBoxQuetSau.Name = "textBoxQuetSau";
			this.textBoxQuetSau.Size = new System.Drawing.Size(100, 23);
			this.textBoxQuetSau.TabIndex = 3;
			this.textBoxQuetSau.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxQuetSau_KeyPress);
			// 
			// labelSoNgayQuet
			// 
			this.labelSoNgayQuet.AutoSize = true;
			this.labelSoNgayQuet.Location = new System.Drawing.Point(15, 103);
			this.labelSoNgayQuet.Name = "labelSoNgayQuet";
			this.labelSoNgayQuet.Size = new System.Drawing.Size(176, 15);
			this.labelSoNgayQuet.TabIndex = 2;
			this.labelSoNgayQuet.Text = "Số ngày quét (sau ngày hiện tại)";
			// 
			// labelGuiEmailSau
			// 
			this.labelGuiEmailSau.AutoSize = true;
			this.labelGuiEmailSau.Location = new System.Drawing.Point(15, 70);
			this.labelGuiEmailSau.Name = "labelGuiEmailSau";
			this.labelGuiEmailSau.Size = new System.Drawing.Size(121, 15);
			this.labelGuiEmailSau.TabIndex = 1;
			this.labelGuiEmailSau.Text = "Gửi Email sau (lần lỗi)";
			// 
			// labelQuetDealSau
			// 
			this.labelQuetDealSau.AutoSize = true;
			this.labelQuetDealSau.Location = new System.Drawing.Point(15, 32);
			this.labelQuetDealSau.Name = "labelQuetDealSau";
			this.labelQuetDealSau.Size = new System.Drawing.Size(115, 15);
			this.labelQuetDealSau.TabIndex = 0;
			this.labelQuetDealSau.Text = "Quét deal sau (phút)";
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(994, 527);
			this.Controls.Add(this.tabControlMain);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "FormMain";
			this.Text = "FormMain";
			this.Load += new System.EventHandler(this.FormMain_Load);
			this.tabControlMain.ResumeLayout(false);
			this.tabPageHome.ResumeLayout(false);
			this.tabPageDealList.ResumeLayout(false);
			this.tabPageDealList.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewDealList)).EndInit();
			this.Config.ResumeLayout(false);
			this.groupBoxCauHinhQuet.ResumeLayout(false);
			this.groupBoxCauHinhQuet.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MenuStrip menuStrip1;
		private ContextMenuStrip contextMenuStrip1;
		private TabControl tabControlMain;
		private TabPage tabPageHome;
		private TabPage tabPageDealList;
		private Button btnEnd;
		private Button btnStart;
		private RichTextBox richTextBoxLog;
		private TabPage Config;
		private Label labelSearch_FromDate;
		private DateTimePicker dateTimePicker_ToDate;
		private TextBox textBoxDealID;
		private Button buttonSearchDeal;
		private DateTimePicker dateTimePicker_FromDate;
		private DataGridView dataGridViewDealList;
		private Label labelSearch_DealID;
		private Label labelSearch_ToDate;
		private GroupBox groupBoxCauHinhQuet;
		private Button buttonLuuCauHinhQuetDeal;
		private TextBox textBoxSoNgayQuet;
		private TextBox textBoxGuiEmailSau;
		private TextBox textBoxQuetSau;
		private Label labelSoNgayQuet;
		private Label labelGuiEmailSau;
		private Label labelQuetDealSau;
	}
}