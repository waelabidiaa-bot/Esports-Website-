namespace EsportPlatform.API.Data.DTOs.GameDto;

public record GameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Platforms { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public byte[]? ImageData { get; init; }
}   