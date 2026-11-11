
using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.GameDto;

public record CreateGameDto (
                            string Name,
                            string Genre,
                            string Description,
                            string Platforms,
                            bool IsActive,
                            byte[] ImageData)
{
    
    
}