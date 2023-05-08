using Microsoft.EntityFrameworkCore;
using Project1.Models;

namespace Project1.Repositories
{
    public class HotelRepos
    {
        internal static Hotel CreateHotel(string Name, string Adress, string Rules, string CheckIn, string CheckOut, int Stars, string MassEventId)
        {
            using (var Db = new AppDbContext())
            {
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == MassEventId.ToLower());
                Hotel Hotel = new Hotel { Name = Name, Adress = Adress, Rules = Rules, CheckIn = CheckIn, CheckOut = CheckOut, Stars = Stars, MassEvent = Event };
                Db.AddRange(Hotel);
                Db.SaveChanges();
                return Hotel;
            }
        }
        internal static List<Hotel> GetAllHotels()
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.Include(u => u.MassEvent).ToList();
            }
        }
        internal static List<Hotel> GetAllHotelsToManager(string IdManager)
        {
            using (var Db = new AppDbContext())
            {
                var binds = Db.UserXHotels.Include(u => u.User).Include(u => u.Hotel).Where(u => u.UserId.ToString().ToLower() == IdManager.ToLower()).ToList();
                List<Hotel> hotels = new List<Hotel>();
                foreach (var bind in binds)
                {
                    hotels.Add(HotelRepos.GetHotelById(bind.HotelId.ToString()));
                }
                return hotels;
            }
        }
        internal static List<Hotel> GetAllHotelsByEventId(string IdEvent)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.Include(h => h.MassEvent).Where(h => h.MassEvent.Id.ToString().ToLower() == IdEvent).ToList();
            }
        }

        internal static Hotel GetHotelById(string Id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
            }
        }
        internal static Hotel GetHotelByName(string Name)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Hotels.ToList().FirstOrDefault(p => p.Name.ToString().ToLower() == Name.ToLower());
            }
        }
        internal static void UpdateHotelInfo(string Id, string? Name, string? Adress, string? Rules, string? CheckIn, string? CheckOut, int Stars, string MassEventId)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == MassEventId.ToLower());
                Hotel.Name = Name;
                Hotel.Adress = Adress;
                Hotel.Rules = Rules;
                Hotel.CheckIn = CheckIn;
                Hotel.CheckOut = CheckOut;
                Hotel.Stars = Stars;
                Hotel.MassEvent = Event;
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
                foreach (var rec in Records)
                {
                    Db.Records.Remove(rec);
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
                    SettlerRepos.DeleteBinds(DelHotel.Id.ToString());
                    Db.Hotels.Remove(DelHotel);
                    
                    var EntDaysToDel = Db.EnteredDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();
                    var DifDaysToDel = Db.DifferenceDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();
                    var RecDaysToDel = Db.RecordDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == DelHotel.Id.ToString().ToLower()).ToList();

                    for (int i = 0; i < EntDaysToDel.Count; i++)
                    {
                        Db.EnteredDataHotel.Remove(EntDaysToDel[i]);
                        Db.DifferenceDataHotel.Remove(DifDaysToDel[i]);
                        Db.RecordDataHotel.Remove(RecDaysToDel[i]);
                    }
                }
                Db.SaveChanges();
            }
        }
    }
}
