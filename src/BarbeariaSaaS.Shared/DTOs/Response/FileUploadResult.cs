namespace BarbeariaSaaS.Shared.DTOs.Response;

public class FileUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
} 