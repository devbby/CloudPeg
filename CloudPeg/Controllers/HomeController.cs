using System.Diagnostics;
using CloudPeg.Application.Command;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using CloudPeg.Models;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.StaticFiles;

namespace CloudPeg.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFsService _fsService;
    private readonly IProcessingOptionsService _processingOptionsService;
    private readonly IProcessingQueue _processingQueueService;

    public HomeController(ILogger<HomeController> logger, IFsService fsService, IProcessingOptionsService processingOptionsService, IProcessingQueue processingQueueService)
    {
        _logger = logger;
        _fsService = fsService;
        _processingOptionsService = processingOptionsService;
        _processingQueueService = processingQueueService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Process([FromBody]ProcessFileCommand command)
    {
        var resource = await _fsService.GetFileRealPath(command.FilePath);
        var parentDir = _fsService.GetParentDirectory(resource);
        var mediaInfo = await FFProbe.AnalyseAsync(resource.RealPath); 
        var processRequest = new ProcessingRequest(resource, 
            command.Template, parentDir, mediaInfo, command.VideoStreams, command.AudioStreams, command.SubtitleStreams);
         
        await _processingQueueService.EnqueueForProcessing(processRequest);
       
        
        return Json(mediaInfo);
    }

    [HttpPost]
    public async Task<IActionResult> CancelProcessing([FromBody] ProcessingInfo processRequest)
    {
        
        await _processingQueueService.CancelProcessing(processRequest.ProcessRequest.Id);
        
       
        return Json(new {});
    }
    
    [HttpPost]
    public async Task<IActionResult> RemoveEnqueuedItem([FromBody] ProcessingInfo processRequest)
    {
        _processingQueueService.RemoveFromQueue(processRequest.ProcessRequest.Id);
        return Json(new {});
    }
    
    [HttpPost]
    public async Task<IActionResult> GetMediaInfo([FromBody] ProcessFileCommand processRequest)
    {
        if (processRequest?.FilePath is null)
        {
            return Json(new {});
        }
        var resource = await _fsService.GetFileRealPath(processRequest.FilePath);

        if (resource.Type != "file")
            return Json(new { });

        try
        {
            var mediaInfo = await FFProbe.AnalyseAsync(resource.RealPath);
            var info = new MediaInfo(mediaInfo);
            return Json(info);
        }
        catch (Exception e)
        {
            
        } 
        return Json(new { });
    }

    public IActionResult Index()
    {
        // return Content(System.IO.File.ReadAllText("/wwwroot/dist/index.html"), "text/html");
        return Redirect("~/dist/index.html"); 
    }

    public async Task<IActionResult> GetConversionTemplates()
    {
        var templates = await  _processingOptionsService.GetConversionTemplates();
        return Json(templates);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}