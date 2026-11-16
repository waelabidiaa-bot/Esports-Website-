using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class GameApiService : BaseApiService
{
    private const string Endpoint = "api/game";

    public GameApiService(ApiClient api) : base(api) { }

    public async Task<List<Game>> GetGamesAsync()
    {
        var dtos = await Api.GetAsync<List<EsportPlatform.API.Data.DTOs.GameDto.GameDto>>(Endpoint);
        var games = new List<Game>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                games.Add(new Game
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Genre = dto.Genre,
                    Publisher = dto.Publisher,
                    Description = dto.Description,
                    Platforms = dto.Platforms,
                    IsActive = dto.IsActive,
                    ImageData = dto.ImageData,
                    ImageContentType = dto.ImageContentType
                });
            }
        }
        return games;
    }

    public async Task<Game?> AddGameAsync(Game game)
    {
        var dto = new EsportPlatform.API.Data.DTOs.GameDto.GameDto
        {
            Name = game.Name,
            Genre = game.Genre,
            Publisher = game.Publisher,
            Description = game.Description,
            Platforms = game.Platforms,
            IsActive = game.IsActive,
            ImageData = game.ImageData,
            ImageContentType = game.ImageContentType
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.GameDto.GameDto>(
            HttpMethod.Post, Endpoint, dto);

        if (result is null) return null;

        return new Game
        {
            Id = result.Id,
            Name = result.Name,
            Genre = result.Genre,
            Publisher = result.Publisher,
            Description = result.Description,
            Platforms = result.Platforms,
            IsActive = result.IsActive,
            ImageData = result.ImageData,
            ImageContentType = result.ImageContentType
        };
    }

    public async Task<Game?> UpdateGameAsync(Game game)
    {
        if (game.Id == 0) return null;

        var dto = new EsportPlatform.API.Data.DTOs.GameDto.GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Publisher = game.Publisher,
            Description = game.Description,
            Platforms = game.Platforms,
            IsActive = game.IsActive,
            ImageData = game.ImageData,
            ImageContentType = game.ImageContentType
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.GameDto.GameDto>(
            HttpMethod.Put, $"{Endpoint}/{game.Id}", dto);

        if (result is null) return null;

        return new Game
        {
            Id = result.Id,
            Name = result.Name,
            Genre = result.Genre,
            Publisher = result.Publisher,
            Description = result.Description,
            Platforms = result.Platforms,
            IsActive = result.IsActive,
            ImageData = result.ImageData,
            ImageContentType = result.ImageContentType
        };
    }

    public async Task ToggleGameAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Patch, $"{Endpoint}/{id}/toggle");
    }
}