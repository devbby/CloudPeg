using CloudPeg.Domain.Model;

namespace CloudPeg.Application.Service;

public interface IProcessingService
{
    Task BeginProcessing();

    Task ProcessItems(List<ProcessingInfo> items);
}