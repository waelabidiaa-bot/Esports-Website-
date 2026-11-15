using EsportPlatform.API.services;

using EsportPlatform.API.Data.DTOs.HighlightDto;

namespace EsportPlatform.API.Interfaces.services.interfaces;

public interface IHighlightService
{
     public  Task<List<HighlightDto>> GetAllHighlightsAsync();
     public  Task<HighlightDto?> GetHighlightByIdAsync(int id);
      public Task<HighlightDto?> CreateHighlightAsync(CreateHighlightDto dto);
      public Task<HighlightDto?> UpdateHighlightAsync(int id, UpdateHighlightDto dto);
      public Task<bool> DeleteHighlightAsync(int id);


}