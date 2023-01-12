using Lisha.Application.Common.Interfaces;

namespace Lisha.Infrastructure.Common.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
