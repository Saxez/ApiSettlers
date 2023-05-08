using Project1.Data;
using Project1.Models;
using Project1.Repositories;
using System.Text.RegularExpressions;

namespace Project1.Database
{
    public class InitData
    {
        public InitData(AppDbContext db)
        {
            if (UserRepos.GetUserByEmailAndPassword("admin@gmail.com", Coder.Encrypt("1234")) == null)
            {
                User User1 = new User { FullName = "admin", Email = "admin@gmail.com", Password = Coder.Encrypt("1234"), Role = "admin" };
                db.Add(User1);
                db.SaveChanges();
            }
        }
    }
}
