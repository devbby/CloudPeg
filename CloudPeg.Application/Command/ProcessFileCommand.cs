using System.Security.AccessControl;
using CloudPeg.Domain.Model;
using MediatR;

namespace CloudPeg.Application.Command;

public class ProcessFileCommand : IRequest
{
    public string FilePath { get; set; }

    public ConversionTemplate Template { get; set; }

    public List<int> VideoStreams { get; set; }
    public List<int> AudioStreams { get; set; }
    public List<int> SubtitleStreams { get; set; }
}