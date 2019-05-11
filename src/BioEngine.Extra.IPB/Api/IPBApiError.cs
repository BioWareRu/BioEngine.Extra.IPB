using JetBrains.Annotations;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class IPBApiError
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
