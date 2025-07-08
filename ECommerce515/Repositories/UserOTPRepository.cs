namespace ECommerce515.Repositories
{
    public class UserOTPRepository : Repository<UserOTP>, IUserOTPRepository
    {
        private readonly ApplicationDbContext _context;

        public UserOTPRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
