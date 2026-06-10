namespace kolokwium.DTO;

public class CreateMakerWithProductsDto
{
    public string Name { get; set; }
    public List<CreateProductDto> Products { get; set; }
}