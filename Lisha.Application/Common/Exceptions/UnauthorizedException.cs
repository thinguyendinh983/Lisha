using System.Net;

namespace Lisha.Application.Common.Exceptions
{
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message)
           : base(message, null, HttpStatusCode.Unauthorized)
        {
        }
    }
}
