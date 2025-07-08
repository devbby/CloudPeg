using CloudPeg.Application;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudPeg.Infrastructure.Service;

public class FsService : IFsService
{
    public async Task<FsResponse> ProcessRequest(string query, string adapter, string path, string name, List<FsResource> items)
    {
        FsResponse response = query switch
        {
            "index" => await GetIndex(adapter, path),
            "preview" => await GetFilePreview(adapter, path),
            "download" => await GetFilePreview(adapter, path),
            _ => new ()


        };
        
        return response;
    }

    private async Task<FsResponse> GetFilePreview(string adapter, string path)
    {
        var storage = GetStorages().FirstOrDefault(x => x.Name == adapter);
        string targetPath = storage.GetRealPath(path);
        var bytes = await File.ReadAllBytesAsync(targetPath);
        return new FileFsResponse
        {
            Adapter = null,
            Storages = null,
            Dirname = null,
            Files = null,
            Bytes = bytes,
            Name = new FileInfo(targetPath).Name,
        };
        
    }

    private async Task<FsResponse> GetIndex(string adapter, string path)
    {
        var storages = GetStorages();
        if (string.IsNullOrEmpty(adapter) || adapter.Contains("null"))
        {
            return new FsResponse
            {
                Adapter = storages.First().Name,
                Storages = storages.Select(x=>x.Name).ToList(),
                Dirname = $"{storages.First().Name}://",
                Files = GetFilesForStorage(storages.First(), null)
            };
        }
        var storage = storages.FirstOrDefault(x => x.Name == adapter);
        if (storage == null) return new FsResponse();
        return new FsResponse
        {
            Adapter = storage.Name,
            Storages = storages.Select(x=> x.Name).ToList(),
            Dirname = path?.Replace("\\", "/") ?? storage.Prefix,
            Files = GetFilesForStorage(storage, path)
        };

        return new();
    }

    private List<FsResource> GetFilesForStorage(FsStorage storage, string path)
    {
        var targetPath = storage.Path;

        if (!string.IsNullOrEmpty(path) && path != "null" && path.StartsWith(storage.Prefix))
        {
            targetPath = Path.Join(targetPath, $"{path.Substring(storage.Prefix.Length)}");
        }

        var files = new DirectoryInfo(targetPath).GetFileSystemInfos().Select(x =>
            {
                new FileExtensionContentTypeProvider().TryGetContentType(x.Name, out var contentType);
                var fsr = new FsResource
                {
                    Path = x.FullName.Replace(storage.Path, storage.Prefix),
                    Type = (x.Attributes & FileAttributes.Directory) != 0 ? "dir" : "file",
                    FileSize = FileHelper.GetSize(x.FullName),
                    LastModified = new DateTimeOffset(x.LastWriteTime).ToUnixTimeSeconds(),
                    Extension = x.Extension,
                    Storage = storage.Name,
                    BaseName = x.Name,
                    MimeType = contentType
                };
                
                return fsr;
            }
        ).ToList();


        return files;
    }

    private List<FsStorage> GetStorages()
    {
        return [
            new FsStorage("Storage1", new DirectoryInfo("../../").FullName){},
            new FsStorage("Storage2", new DirectoryInfo("../../../../").FullName),
        ];
    }
}