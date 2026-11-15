using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.Data.DTOs.TournamentDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using EsportPlatform.API.Interfaces.repositories.interfaces;

namespace EsportPlatform.API.services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository;
    
    public TournamentService(ITournamentRepository tournamentRepository)
    {
       _tournamentRepository = tournamentRepository;
    }

    public async Task<List<TournamentDto>> GetAllTournamentsAsync()
    {
        var tournaments = await _tournamentRepository.GetAllAsync();
        return tournaments.Select(t => new TournamentDto(t)).ToList();
    }

    public async Task<TournamentDto?> GetTournamentByIdAsync(int id)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        return tournament is null ? null : new TournamentDto(tournament);
    }

    public async Task<TournamentDto?> CreateTournamentAsync(CreateTournamentDto tournamentDto)
    {
        var gameExists = await _tournamentRepository.ExistGameByIdAsync(tournamentDto.GameId);
        var duplicate = await _tournamentRepository.ExistTournamentByNameAsync(tournamentDto.Name);

        if (!gameExists || duplicate)
        {
            return null;
        }

        var tournament = tournamentDto.ToEntity();

        await _tournamentRepository.AddAsync(tournament);
        await _tournamentRepository.SaveChangesAsync();

        return new TournamentDto(tournament);
    }

    public async Task<TournamentDto?> UpdateTournamentAsync(UpdateTournamentDto tournamentDto, int id)
    {
        var existing = await _tournamentRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        var gameExists = await _tournamentRepository.ExistGameByIdAsync(tournamentDto.GameId);
        var duplicate = await _tournamentRepository.ExistTournamentByNameExcludingIdAsync(id, tournamentDto.Name);

        if (!gameExists || duplicate)
        {
            return null;
        }

        tournamentDto.ApplyTo(existing);

        await _tournamentRepository.SaveChangesAsync();
        return new TournamentDto(existing);
    }

    public async Task<bool> DeleteTournamentAsync(int id)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        if (tournament is null)
        {
            return false;
        }

        _tournamentRepository.Delete(tournament);
        await _tournamentRepository.SaveChangesAsync();
        return true;
    }
}