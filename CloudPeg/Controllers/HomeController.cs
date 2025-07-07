using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloudPeg.Models;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.AspNetCore.Components;

namespace CloudPeg.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public async Task<IActionResult> Process()
    {
        
         
        
        var mediaInfo = await FFProbe.AnalyseAsync("test.mkv"); 
        
        Console.WriteLine("V Streams:" + mediaInfo.VideoStreams.Count);
        Console.WriteLine("A Streams:" + mediaInfo.AudioStreams.Count);
        Console.WriteLine("Format:" + mediaInfo.Format.FormatLongName);
        Console.WriteLine("Format:" + mediaInfo.Format.BitRate);
        
        // ffmpeg -loglevel verbose -y -hwaccel vaapi -hwaccel_device /dev/dri/renderD128 -hwaccel_output_format vaapi -i test.mp4 -c:v h264_vaapi tmp.mp4
        // -hwaccel_output_format vaapi -i test.mkv -c:v h264_vaapi tmp.mp4

        var processor =  FFMpegArguments
            .FromFileInput("test.mkv", true, options =>
                {
                    options.WithHardwareAcceleration(HardwareAccelerationDevice.VAAPI);
                    options.WithCustomArgument("-hwaccel_output_format vaapi");
                }
            )
            .OutputToFile("converted2.mp4",true, options => options
                       
                .WithVideoCodec(FFMpeg.GetCodec("h264_vaapi"))
                
                
                // .WithSpeedPreset(Speed.Fast)
                // .WithVideoBitrate(4000)
                // .WithCustomArgument("-profile:v high")
                // .WithCustomArgument("-bufsize 8000k")
                // .WithCustomArgument("-g 50")
                // .WithAudioCodec(AudioCodec.Aac)
                // .WithAudioBitrate(128)
                // .WithAudioSamplingRate(44100)
            ).WithLogLevel(FFMpegLogLevel.Debug) 
            .NotifyOnProgress((percentage) =>
            {
                Console.WriteLine($"Progress: {percentage}%");
            }, mediaInfo.Duration);
        
        Console.WriteLine($"ffmpeg { processor.Arguments } ");
        
        await processor.ProcessAsynchronously(true, new FFOptions
            {
                // LogLevel = FFMpegLogLevel.Verbose,
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