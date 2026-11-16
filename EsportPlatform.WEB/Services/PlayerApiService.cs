using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class PlayerApiService : BaseApiService
{
    private const string Endpoint = "api/player";

    public PlayerApiService(ApiClient api) : base(api) { }

    public async Task<List<Player>> GetPlayersAsync()
    {
        var dtos = await Api.GetAsync<List<EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto>>(Endpoint);
        var players = new List<Player>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var player = new Player
                {
                    Id = dto.Id,
                    Gamertag = dto.Gamertag,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    TeamId = dto.TeamId,
                    Role = dto.Role,
                    Country = dto.Country,
                    Status = dto.Status
                };
                players.Add(player);
            }
        }
        return players;
    }

    public async Task<Player?> AddPlayerAsync(Player player)
    {
        var dto = new EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto
        {
            Gamertag = player.Gamertag,
            FullName = player.FullName,
            Email = player.Email,
            TeamId = player.TeamId,
            Role = player.Role,
            Country = player.Country,
            Status = player.Status
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto>(
            HttpMethod.Post, Endpoint, dto);

        if (result is null) return null;

        return new Player
        {
            Id = result.Id,
            Gamertag = result.Gamertag,
            FullName = result.FullName,
            Email = result.Email,
            TeamId = result.TeamId,
            Role = result.Role,
            Country = result.Country,
            Status = result.Status
        };
    }

    public async Task<Player?> UpdatePlayerAsync(Player player)
    {
        if (player.Id == 0) return null;

        var dto = new EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto
        {
            Id = player.Id,
            Gamertag = player.Gamertag,
            FullName = player.FullName,
            Email = player.Email,
            TeamId = player.TeamId,
            Role = player.Role,
            Country = player.Country,
            Status = player.Status
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto>(
            HttpMethod.Put, $"{Endpoint}/{player.Id}", dto);

        if (result is null) return null;

        return new Player
        {
            Id = result.Id,
            Gamertag = result.Gamertag,
            FullName = result.FullName,
            Email = result.Email,
            TeamId = result.TeamId,
            Role = result.Role,
            Country = result.Country,
            Status = result.Status
        };
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        var result = await Api.SendAsync<object>(
            HttpMethod.Delete, $"{Endpoint}/{id}");
        return result is not null;
    }

    public async Task SetPlayerStatusAsync(int id, string status)
    {
        await Api.SendAsync<object>(
            HttpMethod.Patch, $"{Endpoint}/{id}/status",
            new { status });
    }
}