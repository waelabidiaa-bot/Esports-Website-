using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.Data.DTOs.TeamDto;
using EsportPlatform.API.Data.DTOs.PlayerDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using EsportPlatform.API.Interfaces.repositories.interfaces;

namespace EsportPlatform.API.services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;

    public TeamService(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<List<TeamDto>> GetAllTeamsAsync()
    {
        var teams = await _teamRepository.GetAllAsync();
        return teams.Select(t => new TeamDto(t)).ToList();
    }

    public async Task<TeamDto?> GetTeamByIdAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        return team is null ? null : new TeamDto(team);
    }

    public async Task<TeamDto?> CreateTeamAsync(CreateTeamDto teamDto)
    {
        var gameExists = await _teamRepository.ExistTeamByGameId(teamDto.GameId);
        var duplicate = await _teamRepository.ExistTeamByNameOrTag(teamDto.Name, teamDto.Tag);

        if (!gameExists || duplicate)
        {
            return null;
        }

        var newTeam = teamDto.ToEntity();
        newTeam.CreatedAt = newTeam.CreatedAt == default ? DateTime.UtcNow : newTeam.CreatedAt;
        
        await _teamRepository.AddAsync(newTeam);
        await _teamRepository.SaveChangesAsync();
        
        return new TeamDto(newTeam);
    }

    public async Task<TeamDto?> UpdateTeamAsync(UpdateTeamDto teamDto, int id)
    {
        var existing = await _teamRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        var gameExists = await _teamRepository.ExistGameByTeamId(teamDto.GameId);
        var duplicate = await _teamRepository.ExistTeamByNameOrTagExcludingId(id, teamDto.Name, teamDto.Tag);

        if (!gameExists || duplicate)
        {
            return null;
        }

        teamDto.ApplyTo(existing);

        await _teamRepository.SaveChangesAsync();
        return new TeamDto(existing);
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team is null)
        {
            return false;
        }

        _teamRepository.Delete(team);
        await _teamRepository.SaveChangesAsync();
        return true;
    }

    public async Task<TeamDto?> SetStatusAsync(int id, string status)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team is null)
        {
            return null;
        }

        team.Status = status;
        await _teamRepository.SaveChangesAsync();
        return new TeamDto(team);
    }

    public async Task<TeamDto?> RegisterTeamAsync(CreateTeamDto teamDto, IEnumerable<CreatePlayerDto> roster)
    {
        var gameExists = await _teamRepository.ExistGameByTeamId(teamDto.GameId);
        var duplicate = await _teamRepository.ExistTeamByNameOrTag(teamDto.Name, teamDto.Tag);

        if (!gameExists || duplicate)
        {
            return null;
        }

        var team = teamDto.ToEntity();
        var players = roster.ToList();

        team.Status = "Pending";
        team.CreatedAt = DateTime.UtcNow;
        team.RosterSize = players.Count;

        await _teamRepository.AddAsync(team);
        await _teamRepository.SaveChangesAsync();

        foreach (var playerDto in players)
        {
            var player = playerDto.ToEntity();
            player.TeamId = team.Id;
            await _teamRepository.AddPlayerAsync(player);
        }

        var tournament = await _teamRepository.GetEligibleTournamentForTeamAsync(team.GameId);
        if (tournament is not null)
        {
            tournament.RegisteredTeams++;
        }

        await _teamRepository.SaveChangesAsync();
        return new TeamDto(team);
    }

    public async Task<PlayerDto?> AddPlayerToTeamAsync(int teamId, CreatePlayerDto playerDto)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team is null || team.RosterSize >= team.MaxRoster)
        {
            return null;
        }

        var player = playerDto.ToEntity();
        player.TeamId = teamId;

        await _teamRepository.AddPlayerAsync(player);
        team.RosterSize++;

        await _teamRepository.SaveChangesAsync();
        return new PlayerDto(player);
    }
}