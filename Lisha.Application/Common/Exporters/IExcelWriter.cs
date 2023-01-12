using Lisha.Application.Common.Interfaces;

namespace Lisha.Application.Common.Exporters
{
    public interface IExcelWriter : ITransientService
    {
        Stream WriteToStream<T>(IList<T> data);
    }
}
