using kolokwium.DTO;
using kolokwium.Exceptions;
using kolokwium.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolokwium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MakersController : ControllerBase
{
    private readonly IDbService _dbService;
    public MakersController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> GetMakers([FromQuery]string? name)
    {
        try
        {
            var result = await _dbService.GetMakersAsync(null);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Post([FromBody] CreateMakerWithProductsDto dto)
    {
            
        try
        {
            await _dbService.CreateMakerWithProdcutsAsync(dto);
            return Created($"api/makers", dto);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
            
    }
}
    
   
