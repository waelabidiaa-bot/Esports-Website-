using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.MatchDto;

public record MatchDto
{
    
    public int Id { get; init; }
    public int TournamentId { get; init; }
    

    public int GameId { get; init; }
    

    public int? TeamAId { get; set; }
   

    public int? TeamBId { get; init; }
    

    public int ScoreA { get; init; }
    public int ScoreB { get; init; }

    public string Stage { get; init; } = "Group Stage";
    public string BestOf { get; init; } = "Bo3";
    public string Status { get; init; } = "Upcoming";
    public DateTime MatchDate { get; init; }

   
    



    public MatchDto(){}

    public MatchDto(Match m)
    {
        Id = m.Id;
        TournamentId= m.TournamentId;
        GameId = m.GameId ;
        TeamAId = m.TeamAId;
        TeamBId = m.TeamBId;
        ScoreA = m.ScoreA;
        ScoreB= m.ScoreB;
        Stage = m.Stage;
        BestOf = m.BestOf;
        Status = m.Status;
        MatchDate = m.MatchDate ;
        









    }
}