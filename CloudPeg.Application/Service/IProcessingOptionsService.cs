using CloudPeg.Domain.Model;

namespace CloudPeg.Application.Service;

public interface IProcessingOptionsService
{
    Task<List<ConversionTemplate>>GetConversionTemplates();
}