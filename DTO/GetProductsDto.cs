namespace kolokwium.DTO;

public class GetProductsDto
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public decimal stickerPrice { get; set; }
    public GetProductTypeDto productType { get; set; }
    public List<GetVendorsDto> vendors { get; set; }
}