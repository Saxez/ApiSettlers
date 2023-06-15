using Project1.Data;
using Project1.Models;

namespace Project1.Repositories
{
    public class UserRepos
    {
        internal static Users CreateUser(string FullName,  string Email, string Password, string Role)
        {
            using (var Db = new AppDbContext())
            {
                Users User = new Users { FullName = FullName, Email = Email, Password = Password, Role = Role };
                Db.AddRange(User);
                Db.SaveChanges();
                return User;
            }
        }
        internal static List<Users> GetAllUsers()
        {
            using (var Db = new AppDbContext())
            {
                return Db.Users.ToList();
            }
        }
        internal static Users GetUserByEmailAndPassword(string email, string password)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Users.ToList().FirstOrDefault(p => p.Email == email && p.Password == password);
            }
        }

        internal static Users GetUserByEmail(string email)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Users.ToList().FirstOrDefault(p => p.Email == email);
            }
        }
        internal static Users GetUserById(string id)
        {
            using (var Db = new AppDbContext())
            {
                var use = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == id.ToLower());
                return use;
            }
        }
        internal static void UpdateUser(string Id, string FullName, string Email, string Role)
        {
            using (var Db = new AppDbContext())
            {
                Users User = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                User.FullName = FullName;
                User.Email = Email; 
                User.Role = Role;
                Db.Users.Update(User);
                Db.SaveChanges();
            }
        }

        internal static void ResetPassword(string Id, string Password)
        {
            using (var Db = new AppDbContext())
            {
                Users User = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                User.Password = Password;
                Db.Users.Update(User);
                Db.SaveChanges();
            }
        }

        internal static void DeleteUser(string Id)
        {
            using (var Db = new AppDbContext())
            {
                Users User = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                List<Groups> Groups = Db.Groups.Where(g => g.ManagerId.ToString().ToLower() == Id).ToList();
                foreach (var Group in Groups)
                {
                    Group.Manager = null;
                    Group.ManagerId = null;
                    Db.Update(Group);
                }
                List<UserXHotel> userXHotels = Db.UserXHotels.Where(u => u.UserId.ToString().ToLower() == Id.ToLower()).ToList();
                foreach(var bind in userXHotels)
                {
                    Db.Remove(bind);
                }
                Db.Users.Remove(User);
                Db.SaveChanges();
            }
        }
        internal static Users GetUserByRole(string role)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Users.ToList().FirstOrDefault(p => p.Role == role);
            }
        }
    }
}