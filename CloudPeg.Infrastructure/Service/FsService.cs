using CloudPeg.Application;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudPeg.Infrastructure.Service;

public class FsService : IFsService
{
    public async Task<FsResponse> ProcessRequest(string query, string adapter, string path, string name,
        List<FsResource> items, VFPostRequest postData)
    {
        Task<FsResponse> response = query switch
        {
            "index" => GetIndex(adapter, path),
            "preview" => GetFilePreview(adapter, path),
            "download" => GetFilePreview(adapter, path),
            
            "newfolder" => CreateNewFolder(adapter, path, name),
            "newfile" => CreateNewFile(adapter, path, postData),
            "move" =>  MoveResource(adapter, path, postData),
            "rename" =>  RenameResource(adapter, path, postData),
            "save" => SaveResource(adapter, path, postData),
            _ => Task.FromResult<FsResponse>(new FsBadResponse("Not supported"))


        };
        
        return await response;
    }

    private async Task<FsResponse> SaveResource(string adapter, string path, VFPostRequest postData)
    {
        var resource = GetResourceFromStorage(adapter, path);
        if (resource is null) return new FsBadResponse("File/Directory not found in storage");

        await File.WriteAllTextAsync(resource.RealPath, postData.Content);
        
        return await GetIndex(adapter, "Storage1://\\daniel\\");

    }

    private async Task<FsResponse> RenameResource(string adapter, string path, VFPostRequest postData)
    { 
        var resource = GetResourceFromStorage(adapter, postData.Item);
        if (resource is null) return new FsBadResponse("File/Directory not found in storage");

        var newPath = Path.Join( new FileInfo(resource.RealPath).DirectoryName, postData.Name);
        if (resource.Type == "dir")
        {
            Directory.Move(resource.RealPath, newPath );
        }else if (resource.Type == "file")
        {
            File.Move(resource.RealPath, newPath);
        }
        
        
        
        return await GetIndex(adapter, path);
    }

    private async Task<FsResponse> MoveResource(string adapter, string path, VFPostRequest postData)
    {
        var targetDir = GetPathFromStorage(adapter, postData.Item);
        if(!Directory.Exists(targetDir))
            throw new DirectoryNotFoundException(targetDir);

        foreach (var resource in postData.Items)
        {
            var filePath = GetPathFromStorage(adapter, resource.Path);
            var attributes = File.GetAttributes(filePath);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if(!Directory.Exists(filePath))
                    throw new FileNotFoundException(filePath);
                var dirInfo = new DirectoryInfo(filePath);
                Directory.Move(filePath, Path.Join(targetDir ,  dirInfo.Name));
            }
            else
            {
                if(!File.Exists(filePath))
                    throw new FileNotFoundException(filePath);
                var fileInfo = new FileInfo(filePath);
                File.Move(filePath, Path.Join(targetDir ,  fileInfo.Name));
            }
            
            
        }

        return await GetIndex(adapter, path);
    }

    private async Task<FsResponse> CreateNewFile(string adapter, string path, VFPostRequest postData)
    {
        var target = GetPathFromStorage(adapter, Path.Join(path, postData.Name));
        if(File.Exists(target))
            return new FsBadResponse("File already exists");

        await File.WriteAllTextAsync(target, "");
        return await GetIndex(adapter, path);
    }
    
    private async Task<FsResponse> CreateNewFolder(string adapter, string path, string dirName)
    {
        var storage = GetStorage(adapter);
        if(storage is null)
            return new FsBadResponse("Could not find storage");

        var target = storage.GetRealPath(Path.Join(path, dirName));
        if(File.Exists(target))
            return new FsBadResponse("Directory already exists");

        try
        {
            Directory.CreateDirectory(target);
        }
        catch (Exception e)
        {
            return new FsBadResponse($"Could not create directory. Error:  {e.Message}");
        }

        return await GetIndex(adapter, path);
    }
    
    private async Task<FsResponse> GetFilePreview(string adapter, string path)
    {
        var storage = GetStorages().FirstOrDefault(x => path.StartsWith(x.Prefix));
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
        
        if(storages.Count == 0) return new FsResponse();
        
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
        var storage = storages.FirstOrDefault(x =>  x.Name.Equals(adapter, StringComparison.InvariantCultureIgnoreCase) ) ??
                      storages.First();

        return new FsResponse
        {
            Adapter = storage.Name,
            Storages = storages.Select(x=> x.Name).ToList(),
            Dirname = path?.Replace("\\", "/") ?? storage.Prefix,
            Files = GetFilesForStorage(storage, path)
        }; 
    }
    

    private string GetPathFromStorage(string adapter, string path)
    {
        var storage = GetStorage(adapter);
        if(storage is null)
            throw new Exception("Could not find storage");

        var target = storage.GetRealPath(path);
        return target;
    }
    
    private FsStorage? GetStorage(string adapter)
    {
        var storage = GetStorages()
            .FirstOrDefault(x=>x.Name.Equals(adapter, StringComparison.InvariantCultureIgnoreCase));
        return storage;
    }

    public FsResource? GetResourceFromStorage(string adapter, string path)
    {
        var storage = GetStorage(adapter);
        if (storage is null) return null;
        
        var realPath = storage.GetRealPath(path);
        // var target = System.IO.Path.Combine(Path, realPath);

        FileSystemInfo? info;
        var attributes = File.GetAttributes(realPath);
        info = (attributes & FileAttributes.Directory) == FileAttributes.Directory
            ? new DirectoryInfo(realPath)
            : new FileInfo(realPath);
        
        new FileExtensionContentTypeProvider().TryGetContentType(info.FullName, out var contentType);
        
        var fsr = new FsResource
        {
            Path = info.FullName.Replace(storage.Path, storage.Prefix),
            RealPath = info.FullName,
            Type = (info.Attributes & FileAttributes.Directory) != 0 ? "dir" : "file",
            FileSize = FileHelper.GetSize(info.FullName),
            LastModified = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds(),
            Extension = info.Extension,
            Storage = storage.Name,
            BaseName = info.Name,
            MimeType = contentType
        };

        return fsr;

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
        var dir = new DirectoryInfo("fsroot");
        var list = new List<FsStorage>();
        foreach (var file in dir.GetFileSystemInfos())
        {
            if (file.Attributes.HasFlag(FileAttributes.Directory))
            {
                list.Add(new FsStorage(file.Name, file.FullName));
            }
        }

        return list;
    }
}