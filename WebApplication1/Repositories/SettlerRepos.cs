using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project1.Models;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Project1.Repositories
{
    public class SettlerRepos
    {
        internal static string CreateSettler(string FullName, string Contact, string IdGroup)
        {
            using (var Db = new AppDbContext())
            {

                Groups Group = Db.Groups.Include(g => g.MassEvent).Where(g => g.Id.ToString().ToLower() == IdGroup.ToLower() ).First();
                Group.Count += 1;
                Db.Update(Group);
                Settler Settler = new Settler { FullName = FullName, Contact = Contact, Groups = Group, Hotel = null, HotelId = null };
                Db.Settler.Add(Settler);
                Db.SaveChanges();
                return Settler.Id.ToString();
            }
        }
        internal static List<Settler> GetAllSettlers()
        {
            using (var Db = new AppDbContext())
            {
                return Db.Settler.Include(s => s.Groups).ToList();
            }
        }
        internal static Settler GetSettlerById(string Id)
        {

            using (var Db = new AppDbContext())
            {
                return Db.Settler.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
            }

        }
        internal static List<Settler> GetAllSettlersFromHotel(string Id)
        {
            using (var Db = new AppDbContext())
            {

                var Settlers = Db.Settler.Include(s => s.Hotel).Where(s => s.Hotel.Id.ToString().ToLower() == Id).ToList();
                return Settlers;

            }
        }
        internal static List<Settler> GetAllSettlersFromGroup(string Id)
        {
            using (var Db = new AppDbContext())
            {
                var Settlers = Db.Settler.Include(s => s.Groups).Include(s => s.Hotel).Where(s => s.Groups.Id.ToString().ToLower() == Id).ToList();
                return Settlers;

            }
        }

        internal static void SetHotelToSettler(string IdSettler, string IdHotel)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.FirstOrDefault(h => h.Id.ToString().ToLower() == IdHotel.ToLower());
                Settler Settler = Db.Settler.FirstOrDefault(h => h.Id.ToString().ToLower() == IdSettler.ToLower());
                Settler.Hotel = Hotel;
                Db.Settler.Update(Settler);
                Db.SaveChanges();
            }
        }
        internal static void UpdateSettler(string Id, string FullName, string Contact)
        {
            using (var Db = new AppDbContext())
            {
                Settler Settler = Db.Settler.Include(s => s.Groups).ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                Settler.Contact = Contact;

                Groups PastGroup = Settler.Groups;
                Db.Update(PastGroup);
                Settler.FullName = FullName;


                Db.Update(Settler);
                Db.SaveChanges();
            }
        }
        internal static void DeleteSettler(string Id)
        {
            using (var Db = new AppDbContext())
            {
                Settler Settler = Db.Settler.Include(s => s.Groups).ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                Db.Settler.Remove(Settler);
                Groups Group = Settler.Groups;
                Group.Count -= 1;
                Db.Update(Group);
                Db.SaveChanges();
            }
        }
        internal static void DeleteSettlersByGroupId(string GroupId)
        {
            using (var Db = new AppDbContext())
            {
                Groups Group = Db.Groups.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == GroupId.ToLower());
                List<Settler> DelSettlers = Db.Settler.Include(u => u.Groups).ToList();
                foreach (Settler DelSettler in DelSettlers)
                {
                    if (DelSettler.Groups == Group)
                    {
                        Db.Settler.Remove(DelSettler);
                    }

                }
                Db.SaveChanges();
            }
        }
        internal static void BindHotels(string IdHotel, string[] IdManagers)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == IdHotel.ToLower());
                if (IdManagers == null)
                {
                    return;
                }
                foreach (string IdManager in IdManagers)
                {
                    User User = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == IdManager.ToLower());
                    var UserXHotel = Db.UserXHotels.Include(e => e.User).Where(e => e.User.Id.ToString().ToLower() == IdManager.ToLower() && e.Hotel.Id.ToString().ToLower() == IdHotel.ToLower()).ToList();
                    if (UserXHotel.Count == 0)
                    {
                        UserXHotel UserXHotels = new UserXHotel { Hotel = Hotel, User = User };
                        Db.AddRange(UserXHotels);
                        Db.SaveChanges();
                    }
                }
            }
        }
        internal static void DeleteBinds(string IdHotel)
        {
            using (var Db = new AppDbContext())
            {
                List<UserXHotel> UserXHotels = Db.UserXHotels.Include(u => u.Hotel).Where(u => u.Hotel.Id.ToString().ToLower() == IdHotel.ToLower()).ToList();
                foreach (UserXHotel bind in UserXHotels)
                {
                    Db.Remove(bind);
                }
                Db.SaveChanges();
            }
        }
    }
}
