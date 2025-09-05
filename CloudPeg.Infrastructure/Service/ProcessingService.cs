using CloudPeg.Application.Command;
using CloudPeg.Application.Service;
using CloudPeg.Application.Utility;
using CloudPeg.Domain.Model;
using FFMpegCore;
using FFMpegCore.Arguments;
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
        var sampleDuration = 5;
        var outputName = item.ProcessRequest.Resource.BaseName.Split(item.ProcessRequest.Resource.Extension)[0] + " "+item.ProcessRequest.Template.EncoderVideoCodec+".mkv";
        var inputPath = item.ProcessRequest.Resource.RealPath;
        if(item.ProcessRequest.IsSample)
        {
            var info = new FileInfo(item.ProcessRequest.Resource.RealPath);
            var subOutputName = Path.Join(info.Directory.FullName, info.Name.Split(info.Extension)[0] + "_sample" + info.Extension);
            // await FFMpeg.SubVideoAsync(item.ProcessRequest.Resource.RealPath,
            //     subOutputName,
            //     TimeSpan.FromMinutes(20),
            //     TimeSpan.FromMinutes(25)
            // );





            var sampleProcessor = FFMpegArguments
                .FromFileInput(inputPath, true, options =>
                {
                    options.WithCustomArgument("-ss 00:05:50");
                    options.WithCustomArgument("-t 00:05:00");

                })
                .OutputToFile(subOutputName, true, options =>
                {
                    options.WithCustomArgument("-map 0");
                    options.WithCustomArgument("-c copy");
                    
                })
                .NotifyOnProgress((percentage) =>
                {
                    Console.WriteLine($"Creating sample: {percentage}%");

                }, TimeSpan.FromMinutes(sampleDuration))
                .CancellableThrough(item.ProcessRequest.Token);
            
            var sampleFfmpegLine = $"ffmpeg {sampleProcessor.Arguments} ";
            Console.WriteLine(sampleFfmpegLine);
            await sampleProcessor.ProcessAsynchronously();
            inputPath = subOutputName;
            outputName = item.ProcessRequest.Resource.BaseName.Split(item.ProcessRequest.Resource.Extension)[0] + " "+item.ProcessRequest.Template.EncoderVideoCodec+"_sample.mkv";
        }
        
        var processor = FFMpegArguments
            .FromFileInput(inputPath, true, options =>
            {
                
                // options.WithVideoCodec(item.ProcessRequest.MediaInfo.PrimaryVideoStream.CodecName);
                
                if (template.UseHardwareDecoding)
                {
                    options.WithHardwareAcceleration(
                        Enum.Parse<HardwareAccelerationDevice>(template.HwDevice));

                    if(template.HwDecoderArguments != null)
                    {
                        foreach (var argument in template.HwDecoderArguments)
                        {
                            options.WithCustomArgument(argument.Argument);
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
                        options.WithCustomArgument(argument.Argument);
                    }
                }
                
            })
            .OutputToFile(Path.Join(item.ProcessRequest.ParentDir, outputName),true, options =>
            {
                
                if (template.UseHardwareEncoding)
                {
                    if(template.HwEncoderArguments != null)
                        foreach (var argument in template.HwEncoderArguments)
                        {
                            if (argument is IScaleCodecArgument scaleArg)
                            {
                                if(item.ProcessRequest.MediaInfo?.PrimaryVideoStream.Height == scaleArg.Height && 
                                   item.ProcessRequest.MediaInfo?.PrimaryVideoStream.Width == scaleArg.Width)
                                {
                                    Console.WriteLine("Skipping scale filter, original and target size match!");
                                    continue;
                                }
                            }
                            options.WithCustomArgument(argument.Argument);
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

                var passed = DateTime.Now - item.ProcessRequest.ProcessingStarted;
                item.ProcessRequest.Progress = percentage;
                var eta = EtaUtility.EstimateRemainingTime(percentage, passed);
                item.ProcessRequest.Eta =
                    $"{eta.Hours:00}:{eta.Minutes:00}:{eta.Seconds:00} ";
                
                // Console.WriteLine($"Progress: {percentage}%");
                if (percentage >= 100)
                {
                    item.Status = ProcessingStatus.Completed;
                    item.ProcessRequest.ProcessingEnded = DateTime.Now;
                }
                
            }, item.ProcessRequest.IsSample? TimeSpan.FromMinutes(sampleDuration) : item.ProcessRequest.MediaInfo.Duration) 
            .CancellableThrough(item.ProcessRequest.Token);

        var ffmpegLine = $"ffmpeg {processor.Arguments} ";
        Console.WriteLine(ffmpegLine);

        item.Info += $"{Environment.NewLine}{Environment.NewLine} {ffmpegLine} {Environment.NewLine}";
        
        await processor.ProcessAsynchronously();
        
        
    }
}