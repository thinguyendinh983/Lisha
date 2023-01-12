namespace Lisha.Application.Common.Interfaces
{
    public interface IDateTime : ITransientService
    {
        DateTime Now { get; }
    }
}
