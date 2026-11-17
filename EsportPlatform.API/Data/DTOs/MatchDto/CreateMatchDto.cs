namespace EsportPlatform.API.Data.DTOs.MatchDto;

public record CreateMatchDto(int TournamentId,
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
