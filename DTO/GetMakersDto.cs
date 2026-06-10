namespace kolokwium.DTO;

public class GetMakersDto
{
    public int id  { get; set; }
    public string name { get; set; }
    public List<GetProductsDto> products { get; set; }
    
}