using System.Net;

namespace Lisha.Application.Common.Exceptions
{
    public class ConflictException : CustomException
    {
        public ConflictException(string message)
            : base(message, null, HttpStatusCode.Conflict)
        {
        }
    }
}
