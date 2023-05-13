using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project1.Models;

namespace Project1.Repositories
{
    internal static class GroupRepos
    {
        internal static string CreateGroup(string Name, int Count, string MassEventId, string ManagerId, int PrefferedType, DateTime DateOfStart, DateTime DateOfEnd)
        {
            using (var Db = new AppDbContext())
            {
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == MassEventId.ToLower());
                User Manager = null;
                if (ManagerId != null)
                {
                    Manager = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == ManagerId.ToLower());
                }
                Groups Group = new Groups { Name = Name, Count = Count, Manager = Manager, MassEvent = Event, MassEventId = Event.Id, DateOfStart = DateOfStart, DateOfEnd = DateOfEnd, PreferredType = PrefferedType, Status = false };
                Db.AddRange(Group);
                Db.SaveChanges();
                return Group.Id.ToString();
            }
        }
        internal static List<Groups> GetAllGroups()
        {
            using (var Db = new AppDbContext())
            {
                return Db.Groups.Include(g => g.Settlers).Include(group => group.Manager).Include(group => group.MassEvent).ToList();
            }
        }

        internal static List<Groups> GetGroupsByEventId(string id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Groups.Include(g => g.Settlers).Include(group => group.Manager).Include(group => group.MassEvent).Where(group => group!.MassEventId.ToString().ToLower() == id).ToList();
            }
        }
        internal static List<Groups> GetGroupsByOwnerId(string id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Groups.Include(g => g.Settlers).Include(group => group.Manager).Include(group => group.MassEvent).Where(group => group!.Manager.Id.ToString() == id).ToList();
            }
        }
        internal static Groups GetGroupById(string Id)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Groups.Where(p => p.Id.ToString().ToLower() == Id.ToLower()).FirstOrDefault();
            }

        }
        internal static void UpdateGroup(string Id, string Name, string ManagerId, DateTime DateOfStart, DateTime DateOfEnd)
        {
            using (var Db = new AppDbContext())
            {
                Groups Group = Db.Groups.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                User Manager = Db.Users.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == ManagerId.ToLower());
                Group.Name = Name;
                Group.Manager = Manager;
                Group.DateOfStart = DateOfStart;
                Group.DateOfEnd = DateOfEnd;
                Db.Groups.Update(Group);
                Db.SaveChanges();
            }
        }
        internal static void DeleteGroup(string Id)
        {
            using (var Db = new AppDbContext())
            {
                Groups Group = Db.Groups.Include(e => e.MassEvent).ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == Id.ToLower());
                Db.Groups.Remove(Group);
                Db.SaveChanges();
            }
        }
        internal static void DeleteGroupsByEventId(string EventId)
        {
            using (var Db = new AppDbContext())
            {
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == EventId.ToLower());
                List<Groups> DelGroups = Db.Groups.Include(u => u.MassEvent).Where(d => d.MassEventId.ToString().ToLower() == EventId.ToLower()).ToList();
                foreach (Groups DelGroup in DelGroups)
                {
                    if (DelGroup.MassEvent == Event)
                    {
                        Db.Groups.Remove(DelGroup);
                        SettlerRepos.DeleteSettlersByGroupId(DelGroup.Id.ToString().ToLower());
                    }

                }
                Db.SaveChanges();
            }
        }
    }
}
