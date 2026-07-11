using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IServiceRepository
{
    void Add(Service service);
}
