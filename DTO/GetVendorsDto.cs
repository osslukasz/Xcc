namespace kolokwium.DTO;

public class GetVendorsDto
{
    public string code { get; set; }
    public string name { get; set; }
    public int amount { get; set; }
    public decimal pricePerUnit { get; set; }
}