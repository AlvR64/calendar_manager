using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IAccessTokenService
{
    AccessToken CreateForAdmin(Admin admin);

    AccessToken CreateForCustomer(Customer customer);
}
