using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using EsportPlatform.API.Data.DTOs.HighlightDto;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HighlightController : ControllerBase
{
    private readonly HighlightService _highlightService;

    public HighlightController(HighlightService highlightService)
    {
        _highlightService = highlightService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllHighlights()
    {
        return Ok(await _highlightService.GetAllHighlightsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHighlightById(int id)
    {
        var highlight = await _highlightService.GetHighlightByIdAsync(id);
        if (highlight is null)
        {
            return NotFound("Highlight doesn't exist.");
        }

        return Ok(highlight);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHighlight([FromBody] CreateHighlightDto highlight)
    {
        if (string.IsNullOrWhiteSpace(highlight.Title))
        {
            return BadRequest("Highlight title is required.");
        }

        if (string.IsNullOrWhiteSpace(highlight.VideoUrl))
        {
            return BadRequest("Video URL is required.");
        }

        var created = await _highlightService.CreateHighlightAsync( highlight);
        if (created is null)
        {
            return BadRequest("The selected game is invalid.");
        }

        return CreatedAtAction(nameof(GetHighlightById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHighlight([FromBody] UpdateHighlightDto highlight, int id)
    {
        var existing = await _highlightService.GetHighlightByIdAsync(id);
        if (existing is null)
        {
            return NotFound("There is no highlight with that id.");
        }

        var updated = await _highlightService.UpdateHighlightAsync( id, highlight);
        if (updated is null)
        {
            return BadRequest("The selected game is invalid.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHighlight(int id)
    {
        var deleted = await _highlightService.DeleteHighlightAsync(id);
        if (!deleted)
        {
            return NotFound("This highlight doesn't exist.");
        }

        return NoContent();
    }
}
