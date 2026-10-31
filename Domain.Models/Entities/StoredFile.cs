namespace Domain.Models.Entities;

public class StoredFile
{
    public int Id { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required string StorageKey { get; init; }
    public long Size { get; set; }
}
