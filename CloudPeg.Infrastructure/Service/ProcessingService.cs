using CloudPeg.Application.Command;
using CloudPeg.Application.Service;
using CloudPeg.Domain.Model;
using FFMpegCore;
using FFMpegCore.Enums;
using MediatR;

namespace CloudPeg.Infrastructure.Service;


public class ProcessingService : IProcessingService
{
    private readonly IMediator _mediator;
    private readonly IProcessingQueue _processingQueue;
    private Timer? _timer;

    public ProcessingService(IMediator mediator, IProcessingQueue processingQueue)
    {
        _mediator = mediator;
        _processingQueue = processingQueue;
    }

    public async Task BeginProcessing()
    {
        _timer = new Timer(async state =>
            {
                var queue = _processingQueue.GetQueue();
                var toProcess = queue
                    .Where(x => x.Status == ProcessingStatus.Enqueued)
                    .ToList();
                
                if(queue.Count(x => x.Status == ProcessingStatus.Processing) <1)
                    await ProcessItems(toProcess);    
                    
                await _mediator.Send(new NotifyProcessingStatusCommand(_processingQueue.GetQueue()));
            },
        null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public async Task ProcessItems(List<ProcessingInfo> items)
    {

        foreach (var item in items)
        {
            try
            {
                await ProcessItem(item);

            }
            catch (Exception e)
            {
                item.SetError(e.Message);
            }
        }
        
        
    }

    private async Task ProcessItem(ProcessingInfo item)
    {
        item.Status =  ProcessingStatus.Processing;
        var template = item.ProcessRequest.Template;
        
        var outputName = item.ProcessRequest.Resource.BaseName.Split(item.ProcessRequest.Resource.Extension)[0] + " "+item.ProcessRequest.Template.EncoderVideoCodec+".mkv";
        
        var processor = FFMpegArguments
            .FromFileInput(item.ProcessRequest.Resource.RealPath, true, options =>
            {
                
                // options.WithVideoCodec(item.ProcessRequest.MediaInfo.PrimaryVideoStream.CodecName);
                
                if (template.UseHardwareAcceleration)
                {
                    options.WithHardwareAcceleration(
                        Enum.Parse<HardwareAccelerationDevice>(template.HwDevice));

                    if(template.HwDecoderArguments != null)
                    {
                        foreach (var argument in template.HwDecoderArguments)
                        {
                            options.WithCustomArgument(argument);
                        }
                    }
                    // options.WithCustomArgument("-hwaccel_output_format vaapi");
                    // options.WithCustomArgument("-vaapi_device /dev/dri/renderD128");
                    
                    // options.WithVideoCodec(mediaInfo.Format.FormatLongName);
                    // options.Resize(1280, 720);
                }
                if(template.DecoderArguments != null)
                {
                    foreach (var argument in template.DecoderArguments)
                    {
                        options.WithCustomArgument(argument);
                    }
                }
                
            })
            .OutputToFile(Path.Join(item.ProcessRequest.ParentDir, outputName),true, options =>
            {
                
                if (template.UseHardwareAcceleration)
                {
                    if(template.HwEncoderArguments != null)
                        foreach (var argument in template.HwEncoderArguments)
                        {
                            options.WithCustomArgument(argument);
                        }
                }
                
                
                options
                    // .WithVideoCodec("hevc_vaapi")
                    .WithVideoCodec(template.EncoderVideoCodec)
                    .ForceFormat("matroska");          
                    // .WithVideoFilters(filter =>
                    // {
                    //     // software only filters
                    //     // filter.Scale(VideoSize.Ed);
                    // });
                // options.WithCustomArgument("-vf scale_vaapi=w=-1:h=480");
                // foreach (var streamIndex in item.ProcessRequest.AudioStreams)
                // {
                //     options.SelectStream(streamIndex, 0 , Channel.Audio);
                // }
                // foreach (var streamIndex in item.ProcessRequest.VideoStreams)
                // {
                //     options.SelectStream(streamIndex, 0 , Channel.Video);
                // }
                // foreach (var streamIndex in item.ProcessRequest.SubtitleStreams)
                // {
                //     options.SelectStream(streamIndex, 0 , Channel.Subtitle);
                // }

                
                
                
                options.SelectStreams(
                    item.ProcessRequest.VideoStreams
                        .Union(item.ProcessRequest.AudioStreams)
                        .Union(item.ProcessRequest.SubtitleStreams)
                    // item.ProcessRequest.MediaInfo.VideoStreams.Select(x=>x.Index)
                    // .Union(item.ProcessRequest.MediaInfo.AudioStreams.Select(x=>x.Index)
                    // .Union(item.ProcessRequest.MediaInfo.SubtitleStreams.Select(x=>x.Index))
                    // )
                );
            })
            .NotifyOnProgress((percentage) =>
            {
                if (item.ProcessRequest.ProcessingStarted == default)
                {
                    item.ProcessRequest.ProcessingStarted = DateTime.Now;
                }
                
                item.ProcessRequest.Progress = percentage;
                // Console.WriteLine($"Progress: {percentage}%");
                if (percentage >= 100)
                {
                    item.Status = ProcessingStatus.Completed;
                    item.ProcessRequest.ProcessingEnded = DateTime.Now;
                }
                
            }, item.ProcessRequest.MediaInfo.Duration) 
            .CancellableThrough(item.ProcessRequest.Token);

        var ffmpegLine = $"ffmpeg {processor.Arguments} ";
        Console.WriteLine(ffmpegLine);

        item.Info += $"{Environment.NewLine}{Environment.NewLine} {ffmpegLine} {Environment.NewLine}";
        
        await processor.ProcessAsynchronously();
        
        // var processor =  FFMpegArguments
        //     .FromFileInput(item.ProcessRequest.Resource.RealPath, true, options =>
        //         {
        //             if (item.ProcessRequest.Template.UseHardwareAcceleration)
        //             {
        //                 options.WithHardwareAcceleration(HardwareAccelerationDevice.VAAPI);
        //                 options.WithCustomArgument("-hwaccel_output_format vaapi");
        //                 
        //                 // options.WithVideoCodec(mediaInfo.Format.FormatLongName);
        //             }
        //             options.WithCustomArgument("-vaapi_device /dev/dri/renderD128");
        //             
        //         }
        //     )
        //     // .OutputToFile(Path.Join(processRequest.ParentDir, resource.BaseName+".hevc.converted.mp4"),true, options =>
        //     .OutputToFile(Path.Join(item.ProcessRequest.ParentDir, "conerted.hevc.converted.mp4"),true, options =>
        //         {
        //             // options.WithVideoCodec(FFMpeg.GetCodec("h264_vaapi"));
        //             // options.WithVideoCodec(FFMpeg.GetCodec("hevc"));
        //             options.WithVideoCodec(  FFMpeg.GetCodec("hevc_qsv"));
        //             // options.WithVideoCodec(VideoCodec.LibX265);
        //            //options.WithCustomArgument("-hwaccel_output_format mp4");
        //            // options.WithCustomArgument("-vf \"format=nv12,hwupload,scale_vaapi=w=1920:h=1080\"");
        //            // options.WithCustomArgument("-vf \"format=nv12,hwupload \"");
        //             options.SelectStreams(
        //                 item.ProcessRequest.MediaInfo.VideoStreams.Select(x=>x.Index)
        //                 .Union(item.ProcessRequest.MediaInfo.AudioStreams.Select(x=>x.Index)
        //                     //.Union(mediaInfo.SubtitleStreams.Select(x=>x.Index))
        //                 )
        //             );
        //             options.WithVideoBitrate((int)item.ProcessRequest.MediaInfo.VideoStreams.First().BitRate);
        //             options.WithVideoFilters(filter =>
        //             {
        //                 filter.Scale(VideoSize.FullHd);
        //             });
        //         // .WithSpeedPreset(Speed.Fast)
        //         // .WithVideoBitrate(4000)
        //         // .WithCustomArgument("-profile:v high")
        //         // .WithCustomArgument("-bufsize 8000k")
        //         // .WithCustomArgument("-g 50")
        //         //options.WithAudioCodec(AudioCodec.Aac);
        //         options.WithAudioCodec(FFMpeg.GetCodec("libopus"));
        //         options.WithAudioFilters(filterOptions =>
        //             filterOptions.Pan(2, "c0=FL+0.30*FC+0.30*LFE+0.2*BL+0.2*BR|c1=FR+0.30*FC+0.30*LFE+0.2*BL+0.2*BR"));
        //         // .WithAudioBitrate(128)
        //         // .WithAudioSamplingRate(44100)
        //         }
        //     )
        //     //.WithLogLevel(FFMpegLogLevel.Error) 
        //     .NotifyOnProgress((percentage) =>
        //     {
        //         item.ProcessRequest.Progress = percentage;
        //         Console.WriteLine($"Progress: {percentage}%");
        //     }, item.ProcessRequest.MediaInfo.Duration);
        //
        //
        // Console.WriteLine($"ffmpeg { processor.Arguments } ");
        // processor.CancellableThrough(item.ProcessRequest.Token);
        // await processor.ProcessAsynchronously(true, new FFOptions
        // {
        //     //LogLevel = FFMpegLogLevel.Error,
        //     WorkingDirectory = Environment.CurrentDirectory,
        // });
    }
}