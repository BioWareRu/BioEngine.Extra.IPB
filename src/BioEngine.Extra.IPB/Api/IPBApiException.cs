using System;
using System.Net;

namespace BioEngine.Extra.IPB.Api
{
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