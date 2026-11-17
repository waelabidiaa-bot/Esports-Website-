using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data;

using EsportPlatform.API.Data.DTOs.PlayerDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class PlayerService : IPlayerService
{
    private const string Approved = "Approved";
    private const string Pending = "Pending";
    private const string Rejected = "Rejected";

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

    public async Task<List<PlayerDto>> GetPendingPlayersAsync()
    {
        var players = await _playerRespository.GetAllAsync();
        return players
            .Where(p => p.ApprovalStatus == Pending)
            .OrderBy(p => p.Id)
            .Select(p => new PlayerDto(p))
            .ToList();
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

    public async Task<PlayerDto?> CreatePlayerAsync(
        CreatePlayerDto playerDto,
        bool autoApprove = false)
    {
        var duplicate = await _playerRespository.IsPlayerExist(playerDto.Gamertag);
        if (duplicate)
        {
            return null;
        }

        // Only an admin can create a player that is immediately on the roster.
        var approval = autoApprove ? Approved : Pending;
        Team? team = null;

        if (playerDto.TeamId is not null)
        {
            team = await _playerRespository.FindTeamByPlayer(playerDto.TeamId.Value);
            if (team is null || !await HasRosterSpaceAsync(team))
            {
                return null;
            }

            // Pending players reserve a slot but do not count as an approved member yet.
            if (approval == Approved)
            {
                team.RosterSize++;
            }
        }

        var newPlayer = playerDto.ToEntity();
        newPlayer.ApprovalStatus = approval;

        await _playerRespository.AddAsync(newPlayer);
        await _playerRespository.SaveChangesAsync();

        return new PlayerDto(newPlayer);
    }

    /// <summary>
    /// A slot is free only when approved members plus players already waiting for
    /// approval are still below the team's roster cap.
    /// </summary>
    private async Task<bool> HasRosterSpaceAsync(Team team)
    {
        if (team.RosterSize >= team.MaxRoster)
        {
            return false;
        }

        var pending = await _playerRespository.CountPendingPlayersForTeamAsync(team.Id);
        return team.RosterSize + pending < team.MaxRoster;
    }

    public async Task<PlayerDto?> UpdatePlayerAsync(
        UpdatePlayerDto player,
        int id,
        bool autoApprove = false)
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
            // Validate the destination before changing any tracked entity.
            Team? newTeam = null;
            if (player.TeamId is not null)
            {
                newTeam = await _playerRespository.FindTeamByPlayer(player.TeamId.Value);
                if (newTeam is null || !await HasRosterSpaceAsync(newTeam))
                {
                    return null;
                }
            }

            if (existing.TeamId is not null &&
                existing.ApprovalStatus == Approved)
            {
                var oldTeam = await _playerRespository.FindTeamByPlayer(existing.TeamId.Value);
                if (oldTeam is not null && oldTeam.RosterSize > 0)
                {
                    oldTeam.RosterSize--;
                }
            }

            // A user moving a player to another team must be approved again.
            existing.ApprovalStatus = autoApprove ? Approved : Pending;

            if (newTeam is not null && existing.ApprovalStatus == Approved)
            {
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

        if (player.TeamId is not null && player.ApprovalStatus == Approved)
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

    /// <summary>
    /// Admin-only decision on a submitted roster membership. Approving adds the
    /// player to the team's effective roster; rejecting removes their claim to a
    /// slot but keeps the record for the audit trail.
    /// </summary>
    public async Task<PlayerDto?> SetApprovalAsync(int id, bool approved)
    {
        var player = await _playerRespository.GetByIdAsync(id);
        if (player is null)
        {
            return null;
        }

        if (approved)
        {
            if (player.ApprovalStatus == Approved)
            {
                return new PlayerDto(player);
            }

            Team? team = null;
            if (player.TeamId is not null)
            {
                team = await _playerRespository.FindTeamByPlayer(player.TeamId.Value);
                if (team is null || team.RosterSize >= team.MaxRoster)
                {
                    return null;
                }
            }

            if (team is not null)
            {
                team.RosterSize++;
            }

            player.ApprovalStatus = Approved;
        }
        else
        {
            if (player.ApprovalStatus == Approved && player.TeamId is not null)
            {
                var team = await _playerRespository.FindTeamByPlayer(player.TeamId.Value);
                if (team is not null && team.RosterSize > 0)
                {
                    team.RosterSize--;
                }
            }

            player.ApprovalStatus = Rejected;
        }

        await _playerRespository.SaveChangesAsync();
        return new PlayerDto(player);
    }
}