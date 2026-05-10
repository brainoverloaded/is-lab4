using IsLabApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace IsLabApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly NotesStore _store;
    private readonly ILogger<NotesController> _logger;

    public NotesController(NotesStore store, ILogger<NotesController> logger)
    {
        _store = store;
        _logger = logger;
    }

    // GET /api/notes — список всех заметок
    [HttpGet]
    public IActionResult GetAll() => Ok(_store.GetAll());

    // GET /api/notes/{id} — получить одну заметку
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var note = _store.GetById(id);
        return note is not null ? Ok(note) : NotFound();
    }

    // POST /api/notes — создать заметку
    [HttpPost]
    public IActionResult Create([FromBody] Note note)
    {
        // Минимальная валидация
        if (string.IsNullOrWhiteSpace(note.Title))
            return BadRequest("Title is required");

        var created = _store.Create(note);
        _logger.LogInformation("Created note #{Id}: {Title}", created.Id, created.Title);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // DELETE /api/notes/{id} — удалить заметку
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (_store.Delete(id))
        {
            _logger.LogInformation("Deleted note #{Id}", id);
            return NoContent();
        }
        return NotFound();
    }
}