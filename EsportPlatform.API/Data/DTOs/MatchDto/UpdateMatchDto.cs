namespace EsportPlatform.API.Data.DTOs.MatchDto;

public record UpdateMatchDto(int TournamentId,
                             int GameId,
                             int? TeamAId,
                             int? TeamBId,
                             int ScoreA,
                             int ScoreB,
                             DateTime MatchDate,
                             string BestOf,
                             string Status,
                             string Stage
);
