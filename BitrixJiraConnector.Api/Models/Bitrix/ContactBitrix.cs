namespace BitrixJiraConnector.Api.Models.Bitrix;

public class ContactBitrix
{
    public string LastName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

public class ContactInforResult
{
    public ContactBitrix ContactBitrix { get; set; } = new();
    public bool LaThongTinLienHeKhiTrienKhai { get; set; }
}
