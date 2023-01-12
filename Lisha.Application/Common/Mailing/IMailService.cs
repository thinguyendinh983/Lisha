using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Common.Mailing
{
    public interface IMailService : ITransientService
    {
        Task SendAsync(MailRequest request);
    }
}
