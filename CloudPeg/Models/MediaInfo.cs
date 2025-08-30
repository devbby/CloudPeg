using FFMpegCore;

namespace CloudPeg.Models;

public class MediaInfo
{
    public int PrimaryVideoIndex { get; set; }
    public int PrimaryAudioIndex { get; set; }
    public int PrimarySubtitleIndex { get; set; }
    public List<MediaVideoStream> VideoStreams { get; set; }
    public List<MediaAudioStream> AudioStreams { get; set; }
    public List<MediaSubtitleStream> SubtitleStreams { get; set; }
    
    public MediaInfo(IMediaAnalysis mediaAnalysis)
    {
        PrimaryVideoIndex = mediaAnalysis.PrimaryVideoStream?.Index?? -1;
        PrimaryAudioIndex = mediaAnalysis.PrimaryAudioStream?.Index?? -1;
        PrimarySubtitleIndex = mediaAnalysis.PrimarySubtitleStream?.Index ?? -1;
        
        VideoStreams = new List<MediaVideoStream>();
        AudioStreams = new List<MediaAudioStream>();
        SubtitleStreams = new List<MediaSubtitleStream>();
 
        
        
        foreach (var video in mediaAnalysis.VideoStreams)
        {
            VideoStreams.Add(new MediaVideoStream(video));
        }
        foreach (var audio in mediaAnalysis.AudioStreams)
        {
            AudioStreams.Add(new MediaAudioStream(audio));
        }
        foreach (var sub in mediaAnalysis.SubtitleStreams)
        {
            SubtitleStreams.Add(new MediaSubtitleStream(sub));
        }
    }
}