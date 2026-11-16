using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class HighlightApiService : BaseApiService
{
    private const string Endpoint = "api/highlight";

    public HighlightApiService(ApiClient api) : base(api) { }

    public async Task<List<Highlight>> GetHighlightsAsync()
    {
        var dtos = await Api.GetAllAsync<EsportPlatform.API.Data.DTOs.HighlightDto.HighlightDto>(Endpoint);
        var highlights = new List<Highlight>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var highlight = new Highlight
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    GameId = dto.GameId,
                    MatchLabel = dto.MatchLabel,
                    VideoUrl = dto.VideoUrl,
                    Duration = dto.Duration,
                    Views = dto.Views,
                    PublishedAt = dto.PublishedAt
                };
                highlights.Add(highlight);
            }
        }
        return highlights;
    }

    public async Task AddHighlightAsync(Highlight highlight)
    {
        var dto = new EsportPlatform.API.Data.DTOs.HighlightDto.CreateHighlightDto(
            Title: highlight.Title,
            GameId: highlight.GameId,
            MatchLabel: highlight.MatchLabel,
            VideoUrl: highlight.VideoUrl,
            Duration: highlight.Duration,
            Views: highlight.Views);

        await Api.SendAsync<object>(
            HttpMethod.Post, Endpoint, dto);
    }

    public async Task DeleteHighlightAsync(int id)
    {
        await Api.SendAsync<object>(
            HttpMethod.Delete, $"{Endpoint}/{id}");
    }
}