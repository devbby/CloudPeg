using MediatR;

namespace CloudPeg.Application.Command;

public class ProcessFileCommand : IRequest
{
    public List<string> FilePaths { get; set; }
}