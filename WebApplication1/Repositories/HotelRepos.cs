using Microsoft.EntityFrameworkCore;
using Project1.Models;

namespace Project1.Repositories
{
    public class HotelRepos
    {
        internal static string CreateHotel(string Name, string Adress, string CancelCondition, string CheckIn, string CheckOut, int Stars, string MassEventId, string HotelUserId, string Phone, string Email, string Link)
        {
            using (var Db = new AppDbContext())
            {
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == MassEventId.ToLower());
                User HotelUser = null;
                if (HotelUserId != null)
                {
                     HotelUser = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == HotelUserId.ToLower());
                }
                Hotel Hotel = new Hotel { Name = Name, Adress = Adress, CancelCondition = CancelCondition, CheckIn = CheckIn, CheckOut = CheckOut, Stars = Stars, MassEvent = Event, HotelUser = HotelUser, Phone = Phone, Email = Email, Link = Link};
                Db.AddRange(Hotel);
                Db.SaveChanges();
                return Hotel.Id.ToString();
            }
        }
        internal static List<Hotel> GetAllHotels()
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.Include(u => u.MassEvent).ToList();
            }
        }
        internal static List<Hotel> GetAllHotelsToManager(string IdManager,string EventId)
        {
            using (var Db = new AppDbContext())
            {
                var binds = Db.UserXHotels.Include(u => u.User).Include(u => u.Hotel).Where(u => u.UserId.ToString().ToLower() == IdManager.ToLower()).ToList();
                List<Hotel> hotels = new List<Hotel>();
                foreach (var bind in binds)
                {
                    Hotel Hotel = HotelRepos.GetHotelById(bind.HotelId.ToString());
                    if (Hotel.MassEventId.ToString().ToLower() == EventId.ToLower())
                    {
                        hotels.Add(Hotel);
                    }
                }
                return hotels;
            }
        }

        internal static List<Hotel> GetAllHotelsToAmbas(string IdAmbas,string EventId)
        {
            using (var Db = new AppDbContext())
            {
                List<Hotel> Hotels = Db.Hotels.Include(h => h.HotelUser).Where(h => h.HotelUser.Id.ToString().ToLower() == IdAmbas.ToLower() && h.MassEventId.ToString().ToLower() == EventId.ToLower()).ToList();
                
                return Hotels;
            }
        }

        internal static List<User> GetAllManagersToHotel(string IdHotel)
        {
            using (var Db = new AppDbContext())
            {
                var binds = Db.UserXHotels.Include(u => u.User).Include(u => u.Hotel).Where(u => u.HotelId.ToString().ToLower() == IdHotel.ToLower()).ToList();
                List<User> managers = new List<User>();
                foreach (var bind in binds)
                {
                    managers.Add(UserRepos.GetUserById(bind.UserId.ToString()));
                }
                return managers;
            }
        }
        internal static List<Hotel> GetAllHotelsByEventId(string IdEvent)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.Include(h => h.MassEvent).Where(h => h.MassEvent.Id.ToString().ToLower() == IdEvent.ToLower()).ToList();
            }
        }

        internal static Hotel GetHotelById(string Id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
            }
        }

        internal static void UpdateHotelInfo(string Id, string Name, string Adress, string CancelCondition, string CheckIn, string CheckOut, int Stars, string HotelUserId, string Phone, string Email, string Link)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                User HotelUser = null;
                if (HotelUserId != null)
                {
                    HotelUser = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == HotelUserId.ToLower());
                }
                Hotel.Name = Name;
                Hotel.Adress = Adress;
                Hotel.CancelCondition = CancelCondition;
                Hotel.CheckIn = CheckIn;
                Hotel.CheckOut = CheckOut;
                Hotel.Stars = Stars;
                Hotel.Phone = Phone;
                Hotel.Email = Email;
                Hotel.Link = Link;
                Db.Hotels.Update(Hotel);
                Db.SaveChanges();
            }
        }
        internal static void DeleteHotel(string Id)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                var Settlers = Db.Settler.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach (Settler Settler in Settlers)
                {
                    Settler.HotelId = null;
                }
                var Enter = Db.EnteredDataHotel.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach(var ent in Enter) 
                {
                    Db.EnteredDataHotel.Remove(ent);
                }

                var Difference = Db.DifferenceDataHotel.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach (var dif in Difference)
                {
                    Db.DifferenceDataHotel.Remove(dif);
                }

                var RecData = Db.RecordDataHotel.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach (var rec in RecData)
                {
                    Db.RecordDataHotel.Remove(rec);
                }

                var Records = Db.Records.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach (var Record in Records)
                {
                    Db.Records.Remove(Record);
                }

                var Binds = Db.UserXHotels.Include(s => s.Hotel).Where(s => s.Hotel == Hotel).ToList();
                foreach (var Bind in Binds)
                {
                    Db.UserXHotels.Remove(Bind);
                }

                Db.Hotels.Remove(Hotel);
                Db.SaveChanges();
            }
        }
        internal static void DeleteHotelsByEventId(string EventId)
        {
            using (var Db = new AppDbContext())
            {
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == EventId.ToLower());
                List<Hotel> DelHotels = Db.Hotels.Include(u => u.MassEvent).Where(u => u.MassEvent.Id.ToString().ToLower() == EventId).ToList();
                foreach (Hotel DelHotel in DelHotels)
                {
                    var Settlers = Db.Settler.Include(s => s.Hotel).Where(s => s.Hotel == DelHotel).ToList();
                    foreach (Settler Settler in Settlers)
                    {
                        Settler.HotelId = null;
                    }
                    var Enter = Db.EnteredDataHotel.Include(s => s.Hotel).Where(s => s.HotelId.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();
                    foreach (var ent in Enter)
                    {
                        Db.EnteredDataHotel.Remove(ent);
                    }

                    var Difference = Db.DifferenceDataHotel.Include(s => s.Hotel).Where(s => s.Hotel.Id.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();
                    foreach (var dif in Difference)
                    {
                        Db.DifferenceDataHotel.Remove(dif);
                    }

                    var RecData = Db.RecordDataHotel.Include(s => s.Hotel).Where(s => s.Hotel.Id.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();
                    foreach (var rec in RecData)
                    {
                        Db.RecordDataHotel.Remove(rec);
                    }

                    var Records = Db.Records.Include(s => s.Hotel).Where(s => s.Hotel == DelHotel).ToList();
                    foreach (var Record in Records)
                    {
                        Db.Records.Remove(Record);
                    }

                    var Binds = Db.UserXHotels.Include(s => s.Hotel).Where(s => s.Hotel == DelHotel).ToList();
                    foreach (var Bind in Binds)
                    {
                        Db.UserXHotels.Remove(Bind);
                    }
                    var DelRecoords = Db.Records.Include(s => s.Hotel).Where(s => s.Hotel == DelHotel).ToList();
                    foreach (var record in DelRecoords)
                    {
                        Db.Records.Remove(record);
                    }
                    Db.Hotels.Remove(DelHotel);
                }
                Db.SaveChanges();
            }
        }
    }
}
