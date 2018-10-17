using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace BioEngine.Extra.IPB.Api
{
    [SuppressMessage("Readability", "RCS1194", Justification = "Reviewed")]
    public class IPBApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public IPBApiError Error { get; }

        public IPBApiException(HttpStatusCode statusCode, IPBApiError error) : base(error.ErrorMessage)
        {
            StatusCode = statusCode;
            Error = error;
        }
    }
}