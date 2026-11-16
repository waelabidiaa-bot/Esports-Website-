using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data;

using EsportPlatform.API.Data.DTOs.PlayerDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;

namespace EsportPlatform.API.services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRespository;

    public PlayerService(IPlayerRepository playerRespository)
    {
        _playerRespository = playerRespository;
    }

    public async Task<List<PlayerDto>> GetAllPlayersAsync()
    {
        var players = await _playerRespository.GetAllAsync();
        return players.Select(p => new PlayerDto(p)).ToList();
    }

    public async Task<PagedResult<PlayerDto>> GetAllPlayersPagedAsync(
        PaginationQuery pagination)
    {
        pagination ??= new PaginationQuery();

        var page = await _playerRespository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize);

        return page.Map(p => new PlayerDto(p));
    }

    public async Task<PlayerDto?> GetPlayerByIdAsync(int id)
    {
        var player = await _playerRespository.GetByIdAsync(id);
        return player is null ? null : new PlayerDto(player);
    }

    public async Task<PlayerDto?> CreatePlayerAsync(CreatePlayerDto playerDto)
    {
        var duplicate = await _playerRespository.IsPlayerExist(playerDto.Gamertag);
        if (duplicate)
        {
            return null;
        }

        if (playerDto.TeamId is not null)
        {
            var team = await _playerRespository.FindTeamByPlayer(playerDto.TeamId.Value);
            if (team is null || team.RosterSize >= team.MaxRoster)
            {
                return null;
            }

            team.RosterSize++;
        }

        // Use the DTO helper method to avoid manual mapping and missing fields!
        var newPlayer = playerDto.ToEntity();

        await _playerRespository.AddAsync(newPlayer);
        await _playerRespository.SaveChangesAsync();

        return new PlayerDto(newPlayer);
    }

    public async Task<PlayerDto?> UpdatePlayerAsync(UpdatePlayerDto player, int id)
    {
        var existing = await _playerRespository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        var duplicate = await _playerRespository.FindDuplicate(id, player.Gamertag);
        if (duplicate)
        {
            return null;
        }

        if (player.TeamId != existing.TeamId)
        {
            if (existing.TeamId is not null)
            {
                var oldTeam = await _playerRespository.FindTeamByPlayer(existing.TeamId.Value);
                if (oldTeam is not null && oldTeam.RosterSize > 0)
                {
                    oldTeam.RosterSize--;
                }
            }

            if (player.TeamId is not null)
            {
                var newTeam = await _playerRespository.FindTeamByPlayer(player.TeamId.Value);
                if (newTeam is null || newTeam.RosterSize >= newTeam.MaxRoster)
                {
                    return null;
                }

                newTeam.RosterSize++;
            }
        }

        existing.Gamertag = player.Gamertag;
        existing.FullName = player.FullName;
        existing.Email = player.Email;
        existing.TeamId = player.TeamId;
        existing.Role = player.Role;
        existing.Country = player.Country;
        existing.Status = player.Status;

        await _playerRespository.SaveChangesAsync();
        return new PlayerDto(existing);
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        var player = await _playerRespository.GetByIdAsync(id);
        if (player is null)
        {
            return false;
        }

        if (player.TeamId is not null)
        {
            var team = await _playerRespository.FindTeamByPlayer(player.TeamId.Value);
            if (team is not null && team.RosterSize > 0)
            {
                team.RosterSize--;
            }
        }

        _playerRespository.Delete(player);
        await _playerRespository.SaveChangesAsync();
        return true;
    }

    public async Task<PlayerDto?> SetStatusAsync(int id, string status)
    {
        var player = await _playerRespository.GetByIdAsync(id);
        if (player is null)
        {
            return null;
        }

        player.Status = status;
        await _playerRespository.SaveChangesAsync();
        return new PlayerDto(player);
    }
}