using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CloudPeg.Domain.Model;

public class FsResource
{
    public string Path { get; set; }
    public string Type { get; set; }
    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    public string Visibility { get; set; } =  "public";
    [JsonPropertyName("last_modified")]
    public long LastModified { get; set; }

    public string Extension { get; set; }
    public string Storage { get; set; }
    [JsonPropertyName("basename")]
    public string BaseName { get; set; }
    
    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; }

    public string RealPath { get; set; }
}