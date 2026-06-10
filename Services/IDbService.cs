using kolokwium.DTO;

namespace kolokwium.Services;

public interface IDbService
{
    Task<GetMakersDto> GetMakersAsync(string? name);
    Task CreateMakerWithProdcutsAsync(CreateMakerWithProductsDto dto);
}