using System.Collections.Concurrent;

namespace IsLabApp.Models;

public class NotesStore
{
    private readonly ConcurrentDictionary<int, Note> _notes = new();
    private int _nextId = 1;

    public IEnumerable<Note> GetAll() => _notes.Values;

    public Note? GetById(int id) => _notes.TryGetValue(id, out var note) ? note : null;

    public Note Create(Note note)
    {
        note.Id = _nextId++;
        note.CreatedAt = DateTime.UtcNow;
        _notes[note.Id] = note;
        return note;
    }

    public bool Delete(int id) => _notes.TryRemove(id, out _);
}
