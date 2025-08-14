using MediatR;

namespace CloudPeg.Application.Command;

public class ProcessFileCommand : IRequest
{
    public string FilePath { get; set; }

    public bool EnableHardwareAcceleration { get; set; }
}