using Project1.Models;

namespace Project1.Repositories
{
    public class EventRepos
    {
        internal static string CreateEvent(string Name, DateTime DateOfStart, DateTime DateOfEnd)
        {
            using (var Db = new AppDbContext())
            {
                MassEvents Event = new MassEvents { Name = Name, DateOfStart = DateOfStart, DateOfEnd = DateOfEnd };

                Db.MassEvents.AddRange(Event);
                Db.SaveChanges();
                return Event.Id.ToString();
            }
        }
        internal static List<MassEvents> GetAllEvents()
        {
            using (var Db = new AppDbContext())
            {
                return Db.MassEvents.ToList();
            }
        }
        internal static MassEvents GetEventById(string Id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
            }
        }
        internal static MassEvents GetEventByName(string Name)
        {
            using (var Db = new AppDbContext())
            {
                return Db.MassEvents.ToList().FirstOrDefault(p => p.Name.ToString().ToLower() == Name.ToLower());
            }
        }
        internal static void UpdateEventInfo(string Id, string Name, DateTime DateOfStart, DateTime DateOfEnd)
        {
            using (var Db = new AppDbContext())
            {
                MassEvents Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                Event.Name = Name;
                Event.DateOfStart = DateOfStart;
                Event.DateOfEnd = DateOfEnd;
                Db.MassEvents.Update(Event);
                Db.SaveChanges();
            }
        }
        internal static void DeleteEvent(string Id)
        {
            using (var Db = new AppDbContext())
            {
                MassEvents Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                Db.MassEvents.Remove(Event);
                Db.SaveChanges();
            }
        }
    }
}
