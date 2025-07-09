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
    
    [HttpGet("fs/")]
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items, string item)
    {
        FsResponse response = null;
        try
        {
            response = await _fsService.ProcessRequest(q, adapter, path, name, items, null);

            if (response is FsBadResponse badResponse)
                return BadRequest(badResponse);
        
            if (response is FileFsResponse resp)
                return File(resp.Bytes, "application/octet-stream");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
         
        return Json(response);
        
    }

    [HttpPost("fs/")]
    public async Task<IActionResult> Index(string q, string adapter, string path, string name, List<FsResource> items,
        [FromBody] VFPostRequest postData)
    {
        FsResponse response = null;
        try
        {
            response = await _fsService.ProcessRequest(q, adapter, path, name, items, postData);

            if (response is FsBadResponse badResponse)
                return BadRequest(badResponse);
        
            if (response is FileFsResponse resp)
                return File(resp.Bytes, "application/octet-stream");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        
        
        return Json(response);
    }
}