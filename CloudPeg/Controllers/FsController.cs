using CloudPeg.Application;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace CloudPeg.Controllers;

public class FsController : Controller
{
    private readonly IFsService _fsService;

    public FsController(IFsService fsService)
    {
        _fsService = fsService;
    }
    
    // GET
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items)
    {
        
        var response = await _fsService.ProcessRequest(q, adapter, path, name, items);

        if (response is FileFsResponse)
        {
            var resp = response as FileFsResponse;
            return File(resp.Bytes, "application/octet-stream");
        }
        
        return Json(response);
        
        // {
        //     "adapter": adapter.key,
        //     "storages": self._get_storages(),
        //     "dirname": self._get_full_path(request),
        //     "files": [
        //         to_vuefinder_resource(adapter.key, path, info) for info in infos
        //         ],
        // }
        
        // file structure
        // {
        //     "type": "dir" if info.is_dir else "file",
        //     "path": f"{storage}:/{path}/{info.name}",
        //     "visibility": "public",
        //     "last_modified": info.modified.timestamp() if info.modified else None,
        //     "mime_type": mimetypes.guess_type(info.name)[0],
        //     "extra_metadata": [],
        //     "basename": info.name,
        //     "extension": info.name.split(".")[-1],
        //     "storage": storage,
        //     "file_size": info.size,
        // }

        var dirInfo = new DirectoryInfo("../../");
        var files = dirInfo.GetFileSystemInfos().Select(x=> new
        {
            type = (x.Attributes & FileAttributes.Directory) != 0 ? "dir" : "file",
            path = x.FullName,
            visibility = "public",
            last_modified = x.LastWriteTime,
            mime_type = "" ,
            extra_metadata = "",
            basename = x.Name,
            extension = x.Extension,
            storage = "Test 1",
            file_size = FileHelper.GetSize(x.FullName) ,
        }).ToList();
        return Json(new
        {
            
            adapter = "Test 2",
            storages = new List<string> (){"Test 1", "Test 2"},
            dirname = "test",
            files = files
            
        });
    }
}