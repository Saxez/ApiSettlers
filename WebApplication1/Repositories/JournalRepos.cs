using Microsoft.EntityFrameworkCore;
using Project1.Data;
using Project1.Models;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project1.Repositories
{
    public class JournalRepos
    {

        internal static void InitDays(RecInJournal Rec, string HotelId, string EventId)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == HotelId.ToLower());
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == EventId.ToLower());
                EnteredDataHotel Enter = new EnteredDataHotel { DateIn = Rec.Date, Count = Rec.Count, Price = Rec.Price, Capacity = Rec.Capacity, Type = Rec.Type, Hotel = Hotel };
                DifferenceDataHotel Difference = new DifferenceDataHotel { DateIn = Rec.Date, Count = Rec.Count, Price = Rec.Price, Capacity = Rec.Capacity, Type = Rec.Type, Hotel = Hotel};
                RecordDataHotel Record = new RecordDataHotel { DateIn = Rec.Date, Count = 0, Price = Rec.Price, Capacity = Rec.Capacity, Type = Rec.Type, Hotel = Hotel };
                var copy = Db.EnteredDataHotel.Where(e => e.DateIn == Rec.Date && e.Type == Rec.Type &&  e.Hotel.Id.ToString().ToLower() == HotelId).ToList();
                if (copy.Count == 0)
                {
                    Db.Add(Enter);
                    Db.Add(Record);
                    Db.Add(Difference);
                    Db.SaveChanges();
                }
            }
        }

        internal static void UpdateDays(RecInJournal Rec, string HotelId, string EventId)
        {
            using (var Db = new AppDbContext())
            {
                Hotel Hotel = Db.Hotels.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == HotelId.ToLower());
                MassEvent Event = Db.MassEvents.ToList().FirstOrDefault(p => p.Id.ToString().ToLower() == EventId.ToLower());
                EnteredDataHotel CopyEnter = Db.EnteredDataHotel.Where(e => e.DateIn == Rec.Date && e.Type == Rec.Type && e.Hotel.Id.ToString().ToLower() == HotelId).FirstOrDefault();
                var copyTest = Db.EnteredDataHotel.Where(e => e.DateIn == Rec.Date && e.Type == Rec.Type && e.Hotel.Id.ToString().ToLower() == HotelId).ToList();
                if (copyTest.Count == 0)
                {
                    return;
                }
                DifferenceDataHotel CopyDif = Db.DifferenceDataHotel.Where(e => e.DateIn == Rec.Date && e.Type == Rec.Type && e.Hotel.Id.ToString().ToLower() == HotelId).FirstOrDefault();
                var CopyRec = Db.RecordDataHotel.Where(e => e.DateIn == Rec.Date && e.Type == Rec.Type  && e.Hotel.Id.ToString().ToLower() == HotelId).FirstOrDefault();
                CopyEnter.Capacity = Rec.Capacity;
                CopyEnter.Price = Rec.Price;
                CopyEnter.Type = Rec.Type;
                CopyEnter.Count = Rec.Count;

                CopyDif.Capacity = Rec.Capacity;
                CopyDif.Price = Rec.Price;
                CopyDif.Type = Rec.Type;
                CopyDif.Count = Rec.Count - CopyRec.Count;

                CopyRec.Capacity = Rec.Capacity;
                CopyRec.Price = Rec.Price;
                CopyRec.Type = Rec.Type;

                Db.Update(CopyRec);
                Db.Update(CopyEnter);
                Db.Update(CopyDif);

                Db.SaveChanges();
            }
        }

        internal static List<EnteredDataHotel> GetEnteredDataWithType(string HotelId, string EventId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.EnteredDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId && e.Type == Type).ToList();
            }
        }

        internal static List<EnteredDataHotel> GetEnteredDataWithoutType(string HotelId, string EventId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.EnteredDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId).ToList();
            }
        }

        internal static List<DifferenceDataHotel> GetDifferenceDataWithType(string HotelId, string EventId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.DifferenceDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId && e.Type == Type).ToList();
            }
        }

        internal static List<DifferenceDataHotel> GetDifferenceDataWithoutType(string HotelId, string EventId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.DifferenceDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId).ToList();
            }
        }


        internal static List<RecordDataHotel> GetRecordDataWithType(string HotelId, string EventId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.RecordDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId && e.Type == Type).ToList();
            }
        }

        internal static List<RecordDataHotel> GetRecordDataWithoutType(string HotelId, string EventId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.RecordDataHotel.Include(e => e.Hotel).Where(e => e.Hotel.Id.ToString().ToLower() == HotelId).ToList();
            }
        }

        internal static void DelDays(string HotelId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                var EntDaysToDel = Db.EnteredDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Type == Type).ToList();
                var DifDaysToDel = Db.DifferenceDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Type == Type).ToList();
                var RecDaysToDel = Db.RecordDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Type == Type).ToList();

                for(int i = 0; i < EntDaysToDel.Count; i++)
                {
                    Db.EnteredDataHotel.Remove(EntDaysToDel[i]);
                    Db.DifferenceDataHotel.Remove(DifDaysToDel[i]);
                    Db.RecordDataHotel.Remove(RecDaysToDel[i]);
                }

                Db.SaveChanges();
                
            }
        }

        internal static bool isDaysRel(string HotelId, int Count, DateTime DayStart, DateTime DayEnd)
        {
            using (var Db = new AppDbContext())
            {
                while (DayStart != DayEnd.AddDays(1))
                {
                    bool check = false;
                    var days = Db.DifferenceDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == DayStart && d.Hotel.Id.ToString().ToLower() == HotelId).ToList();
                    foreach (var day in days)
                    {
                        if ((day == null) || (day.Count >= Count))
                        {
                            check = true;
                        }
                    }
                    if (!check )
                    {
                        return false;
                    }
                    DayStart = DayStart.AddDays(1);
                }
                return true;
            }
        }

        internal static void CreateRecord(string GroupId, string Type, string HotelId) 
        {
            using (var Db = new AppDbContext())
            {

                Groups Group = Db.Groups.Include(e => e.MassEvent).Where(d => d.Id.ToString().ToLower() == GroupId).First();
                DateTime IterDay = Group.DateOfStart;
                int Price = 0;
                Hotel Hotel = Db.Hotels.Where(d => d.Id.ToString().ToLower() == HotelId).First();
                MassEvent Event = Group.MassEvent;
                while (IterDay != Group.DateOfEnd.AddDays(1))
                {
                    DifferenceDataHotel Dif = Db.DifferenceDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Type == Type && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                    RecordDataHotel Rec = Db.RecordDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Type == Type && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                    Rec.Count = Rec.Count + Group.Count;
                    Dif.Count = Dif.Count - Group.Count;
                    IterDay = IterDay.AddDays(1);
                    Price += Rec.Price * Group.Count;
                    Db.Update(Dif);
                    Db.Update(Rec);
                }
                Record Record = new Record { Price= Price, DateOfCheckIn = Group.DateOfStart, DateOfCheckOut = Group.DateOfEnd, Capacity = Group.Count, Hotel = Hotel, Group = Group};
                Db.Add(Record);
                Db.SaveChanges();
            }
        }

    }
}
