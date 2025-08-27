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
            command.Template, parentDir, mediaInfo);
        Console.WriteLine("V Streams:" + mediaInfo.VideoStreams.Count);
        Console.WriteLine("A Streams:" + mediaInfo.AudioStreams.Count);
        Console.WriteLine("Format:" + mediaInfo.Format.FormatLongName);
        Console.WriteLine("Format:" + mediaInfo.Format.BitRate);
        
        // ffmpeg -loglevel verbose -y -hwaccel vaapi -hwaccel_device /dev/dri/renderD128 -hwaccel_output_format vaapi -i test.mp4 -c:v h264_vaapi tmp.mp4
        // -hwaccel_output_format vaapi -i test.mkv -c:v h264_vaapi tmp.mp4
        await _processingQueueService.EnqueueForProcessing(processRequest);
       
        
        return Json(mediaInfo);
    }

    [HttpPost]
    public async Task<IActionResult> CancelProcessing([FromBody] ProcessingInfo processRequest)
    {
        var items = _processingQueueService.GetQueue().Where(x =>
            x.ProcessRequest.Resource.RealPath == processRequest.ProcessRequest.Resource.RealPath
            && x.ProcessRequest.Resource.BaseName == processRequest.ProcessRequest.Resource.BaseName);

        foreach (var found in items)
        {
            found.Status = ProcessingStatus.Completed;        
            await found.ProcessRequest.CancellationTokenSource.CancelAsync(); 
        }


    return Json(new {});
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