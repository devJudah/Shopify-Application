using Microsoft.EntityFrameworkCore;
using ShopifyBilling.App.Models;

namespace ShopifyBilling.App.Data
{
    public class DataContext : DbContext
    {
        //this class specifies which of our classes act as db models

        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<OAuthState> LoginStates { get; set; }


    }
}
