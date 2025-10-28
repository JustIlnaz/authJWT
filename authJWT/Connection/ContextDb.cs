using Microsoft.EntityFrameworkCore;

namespace authJWT.Connection
{
    public class ContextDb : DbContext
    {

        public ContextDb(DbContextOptions options) : base(options)
        {

        }
    }
}
