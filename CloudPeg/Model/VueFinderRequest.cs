using CloudPeg.Domain.Model;

namespace CloudPeg.Model;

public class VueFinderRequest : VFPostRequest
{
    public IFormFile File { get; set; }
}