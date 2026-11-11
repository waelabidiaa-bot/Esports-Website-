namespace EsportPlatform.API.Data.DTOs.MatchDto;

public record UpdateMatchDto(int TournamentId,
                             int GameId,
                             int? TeamAId,
                             int? TeamBId,
                             int ScoreA,
                             int ScoreB,
                             string BestOf,
                             string Status,
                             string Stage
                             

);