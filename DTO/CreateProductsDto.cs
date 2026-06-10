namespace kolokwium.DTO;

public class CreateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal stickerPrice { get; set; }
    public string Type { get; set; }
}