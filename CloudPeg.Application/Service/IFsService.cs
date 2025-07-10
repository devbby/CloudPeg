using CloudPeg.Domain.Model;

namespace CloudPeg.Application.Service;

public interface IFsService
{ 
    Task<FsResponse> ProcessRequest(string query, string adapter, string path, string name, List<FsResource> items,
        VFPostRequest postData, byte[]? fileContent = null, string filter = "");
}