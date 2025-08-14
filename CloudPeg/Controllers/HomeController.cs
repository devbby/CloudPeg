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

    public HomeController(ILogger<HomeController> logger, IFsService fsService)
    {
        _logger = logger;
        _fsService = fsService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Process([FromBody]ProcessFileCommand command)
    {
        var resource = await _fsService.GetFileRealPath(command.FilePath);
        var parentDir = _fsService.GetParentDirectory(resource);
        var mediaInfo = await FFProbe.AnalyseAsync(resource.RealPath); 
        
        Console.WriteLine("V Streams:" + mediaInfo.VideoStreams.Count);
        Console.WriteLine("A Streams:" + mediaInfo.AudioStreams.Count);
        Console.WriteLine("Format:" + mediaInfo.Format.FormatLongName);
        Console.WriteLine("Format:" + mediaInfo.Format.BitRate);
        
        // ffmpeg -loglevel verbose -y -hwaccel vaapi -hwaccel_device /dev/dri/renderD128 -hwaccel_output_format vaapi -i test.mp4 -c:v h264_vaapi tmp.mp4
        // -hwaccel_output_format vaapi -i test.mkv -c:v h264_vaapi tmp.mp4

        var processor =  FFMpegArguments
            .FromFileInput(resource.RealPath, true, options =>
                {
                    if (command.EnableHardwareAcceleration)
                    {
                        options.WithHardwareAcceleration(HardwareAccelerationDevice.VAAPI);
                        options.WithCustomArgument("-hwaccel_output_format vaapi");
                        
                        // options.WithVideoCodec(mediaInfo.Format.FormatLongName);
                    }
                    options.WithCustomArgument("-vaapi_device /dev/dri/renderD128");
                    
                }
            )
            .OutputToFile(Path.Join(parentDir, resource.BaseName+".hevc.converted.mp4"),true, options =>
                {
                    // options.WithVideoCodec(FFMpeg.GetCodec("h264_vaapi"));
                    options.WithVideoCodec(FFMpeg.GetCodec("hevc"));
                    // options.WithVideoCodec(VideoCodec.LibX265);
                   //options.WithCustomArgument("-hwaccel_output_format mp4");
                   // options.WithCustomArgument("-vf \"format=nv12,hwupload,scale_vaapi=w=1920:h=1080\"");
                   // options.WithCustomArgument("-vf \"format=nv12,hwupload \"");
                    options.SelectStreams(
                    mediaInfo.VideoStreams.Select(x=>x.Index)
                        .Union(mediaInfo.AudioStreams.Select(x=>x.Index)
                            //.Union(mediaInfo.SubtitleStreams.Select(x=>x.Index))
                        )
                    );
                    options.WithVideoBitrate((int)mediaInfo.VideoStreams.First().BitRate);
                    options.WithVideoFilters(filter =>
                    {
                        filter.Scale(VideoSize.FullHd);
                    });
                // .WithSpeedPreset(Speed.Fast)
                // .WithVideoBitrate(4000)
                // .WithCustomArgument("-profile:v high")
                // .WithCustomArgument("-bufsize 8000k")
                // .WithCustomArgument("-g 50")
                //options.WithAudioCodec(AudioCodec.Aac);
                options.WithAudioCodec(FFMpeg.GetCodec("libopus"));
                options.WithAudioFilters(filterOptions =>
                    filterOptions.Pan(2, "c0=FL+0.30*FC+0.30*LFE+0.2*BL+0.2*BR|c1=FR+0.30*FC+0.30*LFE+0.2*BL+0.2*BR"));
                // .WithAudioBitrate(128)
                // .WithAudioSamplingRate(44100)
                }
            )
            //.WithLogLevel(FFMpegLogLevel.Error) 
            .NotifyOnProgress((percentage) =>
            {
                Console.WriteLine($"Progress: {percentage}%");
            }, mediaInfo.Duration);
        
        Console.WriteLine($"ffmpeg { processor.Arguments } ");
        
        await processor.ProcessAsynchronously(true, new FFOptions
            {
                
                //LogLevel = FFMpegLogLevel.Error,
                WorkingDirectory = Environment.CurrentDirectory,
                

            });
        
        return Json(mediaInfo);
    }

    public IActionResult Index()
    {
        // return Content(System.IO.File.ReadAllText("/wwwroot/dist/index.html"), "text/html");
        return Redirect("~/dist/index.html"); 
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