using CloudPeg.Application;
using CloudPeg.Application.Service;
using CloudPeg.Attributes;
using CloudPeg.Domain.Model;
using CloudPeg.Model;
using Microsoft.AspNetCore.Mvc;

namespace CloudPeg.Controllers;

public class FsController : Controller
{
    private readonly IFsService _fsService;

    public FsController(IFsService fsService)
    {
        _fsService = fsService;
    }
    
    [HttpGet("fs/")]
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items, string filter)
    {
        FsResponse response = null;
        try
        {
            response = await _fsService.ProcessRequest(q, adapter, path, name, items, null,null, filter);

            if (response is FsBadResponse badResponse)
                return BadRequest(badResponse);
        
            if (response is FileFsResponse resp)
            { 
                return new FileStreamResult(resp.Stream, "application/octet-stream"){FileDownloadName = resp.Name};
            }        
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
         
        return Json(response);
        
    }
    
    [HttpPost("fs/")]
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items,
        [FromBody] VFPostRequest postData )
    {
        FsResponse response = null;
        try
        {
            response = await _fsService.ProcessRequest(q, adapter, path, name, items, postData);

            if (response is FsBadResponse badResponse)
                return BadRequest(badResponse);
        
            if (response is FileFsResponse resp)
                return new FileStreamResult(resp.Stream, "application/octet-stream"){FileDownloadName = resp.Name};
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
         
        return Json(response);
    }
    

    [HttpPost("fs/")]
    [FormContentType]
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items,
        [FromForm] VueFinderRequest postData)
    {
        FsResponse response = null;
        try
        {
            await using var ms = new MemoryStream();
            await postData.File.CopyToAsync(ms);
            response = await _fsService.ProcessRequest(q, adapter, path, name, items, postData, ms.ToArray());

            if (response is FsBadResponse badResponse)
                return BadRequest(badResponse);
        
            if (response is FileFsResponse resp)
                return new FileStreamResult(resp.Stream, "application/octet-stream"){FileDownloadName = resp.Name};
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
         
        return Json(response);
    }
    
   
}