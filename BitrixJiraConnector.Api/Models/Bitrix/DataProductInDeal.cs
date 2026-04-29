namespace BitrixJiraConnector.Api.Models.Bitrix;

public class DataProductInDeal
{
    public int ID { get; set; }
    public string OWNER_ID { get; set; } = "";
    public string OWNER_TYPE { get; set; } = "";
    public int PRODUCT_ID { get; set; }
    public string PRODUCT_NAME { get; set; } = "";
    public string ORIGINAL_PRODUCT_NAME { get; set; } = "";
    public string PRODUCT_DESCRIPTION { get; set; } = "";
    public double PRICE { get; set; }
    public double PRICE_EXCLUSIVE { get; set; }
    public double PRICE_NETTO { get; set; }
    public double PRICE_BRUTTO { get; set; }
    public double PRICE_ACCOUNT { get; set; }
    public double QUANTITY { get; set; }
    public int DISCOUNT_TYPE_ID { get; set; }
    public double DISCOUNT_RATE { get; set; }
    public double DISCOUNT_SUM { get; set; }
    public double TAX_RATE { get; set; }
    public string TAX_INCLUDED { get; set; } = "";
    public string CUSTOMIZED { get; set; } = "";
    public string MEASURE_CODE { get; set; } = "";
    public string MEASURE_NAME { get; set; } = "";
    public int SORT { get; set; }
}
