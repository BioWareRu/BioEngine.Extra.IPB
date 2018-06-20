using JetBrains.Annotations;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiError
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}