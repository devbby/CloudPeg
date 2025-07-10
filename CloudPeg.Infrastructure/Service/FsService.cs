using System.Reflection.Metadata.Ecma335;
using CloudPeg.Application;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudPeg.Infrastructure.Service;

public class FsService : IFsService
{
    public async Task<FsResponse> ProcessRequest(string query, string adapter, string path, string name,
        List<FsResource> items, VFPostRequest postData, byte[]? fileContent = null, string filter = "")
    {
        Task<FsResponse> response = query switch
        {
            "index" => GetIndex(adapter, path),
            "preview" => GetFilePreview(adapter, path),
            "download" => GetFilePreview(adapter, path),
            
            "subfolders" => GetSubfolders(adapter, path),
            "download_archive" => GetAsArchive(adapter, path),
            "search" => GetSearchResults(adapter, path, filter),
            
            "newfolder" => CreateNewFolder(adapter, path, postData.Name),
            "newfile" => CreateNewFile(adapter, path, postData),
            "move" =>  MoveResource(adapter, path, postData),
            "rename" =>  RenameResource(adapter, path, postData),
            "save" => SaveResource(adapter, path, postData),
            "delete" => DeleteResource(adapter, path, postData),
            "upload" => UploadResource(adapter, path, postData, fileContent),
            "archive"=> ArchiveResources(adapter, path, postData),
            "unarchive" => UnarchiveResources(adapter, path, postData),
            _ => Task.FromResult<FsResponse>(new FsBadResponse("Not supported"))


        };
        
        return await response;
    }

    private async Task<FsResponse> GetSearchResults(string adapter, string path, string filter)
    {
        
        var storages = GetStorages();
        
        if(storages.Count == 0) return new FsResponse();
        
         
        var storage = storages.FirstOrDefault(x =>  x.Name.Equals(adapter, StringComparison.InvariantCultureIgnoreCase) ) ??
                      storages.First();

        return new FsResponse
        {
            
            Adapter = storage.Name,
            Storages = storages.Select(x=> x.Name).ToList(),
            Dirname = path,
            Files = SearchResource(filter)
        }; 
         
    }

    private List<FsResource> SearchResource(string filter)
    {
        var storages = GetStorages();
        var list = new List<FsResource>();
        foreach (var storage in storages)
        {
            var path = storage.Path;
            var files = FindInPath(path, filter);

            foreach (var file in files)
            {
                var info = new FileInfo(file.BaseName);
                new FileExtensionContentTypeProvider().TryGetContentType(info.FullName, out var contentType);
                
                file.Storage = storage.Name;
                file.Adapter = storage.Name;
                file.RealPath = file.BaseName;
                file.BaseName = info.Name;
                file.Extension = info.Extension;
                file.Visibility = "public";
                file.LastModified = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds();
                file.Path = storage.GetStoragePath(info.FullName);
                file.MimeType = contentType;
                file.Type = "file";
                file.Dir = storage.GetStoragePath(info.DirectoryName);
            }
            
            list.AddRange(files);
        }

        return list;
    }
    
    private List<FsResource> FindInPath(string startPath, string filter)
    {
        var list = new List<FsResource>();
        if (!Directory.Exists(startPath))
            return list;

        try
        {
            string[] files = Directory.GetFiles(startPath);
            string[] subdirectories = Directory.GetDirectories(startPath);

            foreach (string subdirectory in subdirectories)
            {
                var result = FindInPath(subdirectory, filter);
                list.AddRange(result);
            }
            
            var matchingFiles = files
                .Where(x=>x.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                .Select(x=>new FsResource
                {
                    Path = null,
                    Type = null,
                    FileSize = 0,
                    Visibility = null,
                    LastModified = 0,
                    Extension = null,
                    Storage = null,
                    BaseName = x,
                    MimeType = null,
                    RealPath = null,
                    Adapter = null
                })
                
                .ToList();
            list.AddRange(matchingFiles);
            return list;
        }
        catch (UnauthorizedAccessException ex)
        {
        }
        catch (PathTooLongException ex)
        {
        }
        catch (Exception ex)
        {
        }

        return list;
    }

    private Task<FsResponse> GetAsArchive(string adapter, string path)
    {
        throw new NotImplementedException();
    }

    private async Task<FsResponse> GetSubfolders(string adapter, string path)
    {
        var files = GetFilesForStorage(GetStorage(adapter), path);
        var list = new List<FsResource>();
        foreach (var file in files)
        {
            if(file.Type == "dir")
                list.Add(file);
        }

        return new FsSubfoldersResponse()
        {
            Folders = list
        };
    }

    private Task<FsResponse> UnarchiveResources(string adapter, string path, VFPostRequest postData)
    {
        throw new NotImplementedException();
    }

    private Task<FsResponse> ArchiveResources(string adapter, string path, VFPostRequest postData)
    {

        var zip = new FastZip();
        // zip.CreateZip();
        // using (var fs = File.Create("html-archive.zip"))
        // using (var outStream = new ZipOutputStream(fs))
        // {
        //     foreach (var item in postData.Items)
        //     {
        //         var resourceItem = GetResourceFromStorage(adapter, item.Path);
        //         using var memoryStream = new FileStream(resourceItem.RealPath, FileMode.Open);
        //         outStream.PutNextEntry(new ZipEntry(resourceItem.BaseName));
        //         memoryStream.CopyTo(outStream);
        //     }
        //         
        // }
        //     
            
        
         
        
        
        
        throw new NotImplementedException();
    }

    private async Task<FsResponse> UploadResource(string adapter, string path, VFPostRequest postData, byte[]? fileContent)
    {
        var fileLocation = CleanPathStructure(postData.Name);
        var target = GetPathFromStorage(adapter, Path.Join(path, fileLocation));
        if(File.Exists(target))
            return new FsBadResponse("File already exists");

        if (fileContent is null) 
            return new FsBadResponse("No data to upload");

        var dirPath = Path.GetDirectoryName(target);
        if(dirPath is null)
            return new FsBadResponse("Ensuring directory structure failed");
            
        Directory.CreateDirectory(dirPath);
        
        await File.WriteAllBytesAsync(target, fileContent);
        return await GetIndex(adapter, path);
    }

    private string CleanPathStructure(string path)
    {
        var forward = path.Split('/');
        if (forward.Length > 1)
        {
            path = Path.Join(forward);
        }
        
        var back = path.Split('\\');
        if (back.Length > 1)
        {
            path = Path.Join(back);
        }

        return path;
    }

    private async Task<FsResponse> DeleteResource(string adapter, string path, VFPostRequest postData)
    {
        foreach (var item in postData.Items)
        {
            var resource = GetResourceFromStorage(adapter, item.Path);
            if (resource is null) return new FsBadResponse("File/Directory not found in storage");
            if ((File.GetAttributes(resource.RealPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(resource.RealPath, true);
            }
            else
            {
                File.Delete(resource.RealPath);
            }
        }
        return await GetIndex(adapter, path);
    }

    private async Task<FsResponse> SaveResource(string adapter, string path, VFPostRequest postData)
    {
        var resource = GetResourceFromStorage(adapter, path);
        if (resource is null) return new FsBadResponse("File/Directory not found in storage");
        await File.WriteAllTextAsync(resource.RealPath, postData.Content);
        return await GetFilePreview(adapter, path);
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
                if (!x.Name.StartsWith("."))
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

                return null;
            }
        ).Where(x=> x!= null).ToList();


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
        #if DEBUG
        list.Add(new  FsStorage("debug", "/home/danny"));
        #endif

        return list;
    }
}