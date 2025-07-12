namespace ECommerce515.Repositories
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task CreateRangeAsync(List<OrderItem> orderItems)
        {
            await _context.OrderItems.AddRangeAsync(orderItems);
        }
    }
}
