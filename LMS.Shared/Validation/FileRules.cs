namespace LMS.Shared.Validation;

public static class FileRules
{
    public const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    public static readonly string[] AllowedExtensions =
        [".pdf", ".docx", ".pptx", ".xlsx", ".txt", ".jpg", ".jpeg", ".png"];

    public static string? Validate(string fileName, long size)
    {
        if (size <= 0)
            return "The file is empty.";
        if (size > MaxFileSize)
            return "The file is larger than 10 MB.";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return $"File type '{extension}' is not allowed.";

        return null;
    }
}
