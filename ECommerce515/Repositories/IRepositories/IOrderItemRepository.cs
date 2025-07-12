using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace ECommerce515.Repositories.IRepositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task CreateRangeAsync(List<OrderItem> orderItems);
    }
}
