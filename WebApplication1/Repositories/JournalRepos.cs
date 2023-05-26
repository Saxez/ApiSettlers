using Microsoft.EntityFrameworkCore;
using Project1.Data;
using Project1.Models;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project1.Repositories
{
    public class JournalRepos
    {

        internal static void InitDays(DateTime Date, int Count, int Price, int Capacity, int Type, Hotel Hotel, string Name )
        {
            using (var Db = new AppDbContext())
            {
                
                EnteredDataHotel Enter = new EnteredDataHotel { DateIn = Date, Count = Count, Price = Price, Capacity = Capacity, Type = Type, HotelId = Hotel.Id, Name = Name };
                DifferenceDataHotel Difference = new DifferenceDataHotel { DateIn = Date, Count = Count, Price = Price, Capacity = Capacity, Type = Type, HotelId = Hotel.Id, Name = Name };
                RecordDataHotel Record = new RecordDataHotel { DateIn = Date, Count = 0, Price = Price, Capacity = Capacity, Type = Type, HotelId = Hotel.Id, Name = Name };
                TypesOfDays Types = new TypesOfDays { HotelId = Hotel.Id, Name = Name, Type = Type };
                var copy = Db.EnteredDataHotel.Where(e => e.DateIn == Date && e.Name == Name &&  e.Hotel.Id.ToString().ToLower() == Hotel.Id.ToString().ToLower()).ToList();
                var CopyOfDays = Db.TypesOfDays.Where(c => c.Hotel.Id.ToString().ToLower() == Hotel.Id.ToString().ToLower() && c.Name == Name).ToList();
                if (copy.Count == 0)
                {
                    if (CopyOfDays.Count == 0)
                    {
                        Db.TypesOfDays.Add(Types);
                    }
                    Db.EnteredDataHotel.Add(Enter);
                    Db.RecordDataHotel.Add(Record);
                    Db.DifferenceDataHotel.Add(Difference);
                    Db.SaveChanges();
                }
            }
        }
        internal static bool CheckExist(string HotelId, string Name)
        {
            using (var Db = new AppDbContext())
            {
                var rec = Db.EnteredDataHotel.Include(r => r.Hotel).Where(r => r.HotelId.ToString().ToLower() == HotelId.ToLower() && r.Name == Name).ToList();
                if(rec.Count > 0)
                {
                    return true;
                }
                return false;
            } 
        }
        internal static void UpdateDays(DateTime Date, int Count, int Price, int Capacity, int Type, Hotel Hotel, string Name)
        {
            using (var Db = new AppDbContext())
            {
                
                
                EnteredDataHotel CopyEnter = Db.EnteredDataHotel.Where(e => e.DateIn == Date && e.Name == Name && e.Hotel.Id == Hotel.Id).FirstOrDefault();
                var copyTest = Db.EnteredDataHotel.Where(e => e.DateIn == Date && e.Name == Name && e.Hotel.Id == Hotel.Id).ToList();
                if (copyTest.Count == 0)
                {
                    return;
                }
                DifferenceDataHotel CopyDif = Db.DifferenceDataHotel.Where(e => e.DateIn == Date && e.Name == Name && e.Hotel.Id == Hotel.Id).FirstOrDefault();
                RecordDataHotel CopyRec = Db.RecordDataHotel.Where(e => e.DateIn == Date && e.Name == Name && e.Hotel.Id == Hotel.Id).FirstOrDefault();
                TypesOfDays CopyType = Db.TypesOfDays.Where(e => e.Name == Name && e.Hotel.Id == Hotel.Id).FirstOrDefault();
                CopyEnter.Capacity = Capacity;
                CopyEnter.Price = Price;
                CopyEnter.Type = Type;
                CopyEnter.Count = Count;

                CopyDif.Capacity = Capacity;
                CopyDif.Price = Price;
                CopyDif.Type = Type;
                CopyDif.Count = Count - CopyRec.Count;

                CopyRec.Capacity = Capacity;
                CopyRec.Price = Price;
                CopyRec.Type = Type;

                CopyType.Type = Type;


                Db.Update(CopyType);
                Db.Update(CopyRec);
                Db.Update(CopyEnter);
                Db.Update(CopyDif);

                Db.SaveChanges();
            }
        }
        internal static void DelDays(string HotelId, string Name)
        {
            using (var Db = new AppDbContext())
            {
                var EntDaysToDel = Db.EnteredDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Name == Name).ToList();
                var DifDaysToDel = Db.DifferenceDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Name == Name).ToList();
                var RecDaysToDel = Db.RecordDataHotel.Include(e => e.Hotel).Where(e => e.HotelId.ToString().ToLower() == HotelId && e.Name == Name).ToList();
                Hotel Hotel = Db.Hotels.Where(h => h.Id.ToString().ToLower() == HotelId.ToLower()).FirstOrDefault();
                TypesOfDays Types = Db.TypesOfDays.Where(e => e.Hotel == Hotel && e.Name == Name).FirstOrDefault();
                
                for (int i = 0; i < EntDaysToDel.Count; i++)
                {
                    if (i == 0)
                    {
                        Db.TypesOfDays.Remove(Types);
                    }
                    Record RecToDel = Db.Records.Where(r => r.DateOfCheckIn == EntDaysToDel[i].DateIn).FirstOrDefault();
                    if(RecToDel != null) 
                    {
                        Db.Records.Remove(RecToDel);
                    }
                    Db.EnteredDataHotel.Remove(EntDaysToDel[i]);
                    Db.DifferenceDataHotel.Remove(DifDaysToDel[i]);
                    Db.RecordDataHotel.Remove(RecDaysToDel[i]);
                }

                Db.SaveChanges();

            }
        }


        internal static bool isTypeRel(TypesOfDays TypeOfDays, int Count, DateTime DayStart, DateTime DayEnd)
        {
            using (var Db = new AppDbContext())
            {
                while(DayEnd.AddDays(1) > DayStart) 
                {
                    var CandToRec = Db.DifferenceDataHotel.Include(d => d.Hotel).Where(d => d.Name == TypeOfDays.Name && d.DateIn == DayStart && d.HotelId == TypeOfDays.HotelId).FirstOrDefault();
                    if ((CandToRec.Count * CandToRec.Capacity) < Count)
                    {
                        return false;
                    }
                    DayStart = DayStart.AddDays(1);
                }
                return true;
            }
        }


        internal static List<TypesOfDays> GetAllTypes(string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.TypesOfDays.Include(t => t.Hotel).Where(t=>t.HotelId.ToString().ToLower() == HotelId.ToLower()).ToList();
            }
        }
        internal static void CreateRecord(string GroupId, string Name, string HotelId) 
        {
            using (var Db = new AppDbContext())
            {

                Groups Group = Db.Groups.Include(e => e.MassEvent).Where(d => d.Id.ToString().ToLower() == GroupId).First();
                Group.Status = true;
                List<Settler> Sets = Db.Settler.Include(e => e.Groups).Where(e => e.GroupsId.ToString().ToLower() == GroupId.ToLower()).ToList();
                
                DateTime IterDay = Group.DateOfStart;
                int Price = 0;
                Hotel Hotel = Db.Hotels.Where(d => d.Id.ToString().ToLower() == HotelId).First();
                foreach (Settler Set in Sets)
                {
                    Set.Hotel = Hotel;
                    Db.Update(Set);
                }
                MassEvent Event = Group.MassEvent;
                int Count = 0;
                int Capacity = 0;
                while (IterDay != Group.DateOfEnd.AddDays(1))
                {
                    DifferenceDataHotel Dif = Db.DifferenceDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Name == Name && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                    RecordDataHotel Rec = Db.RecordDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Name == Name && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                    Rec.Count = Rec.Count + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Group.Count) / Convert.ToDouble(Rec.Capacity)));
                    Dif.Count = Dif.Count - Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Group.Count) / Convert.ToDouble(Dif.Capacity)));
                    Count = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Group.Count) / Convert.ToDouble(Rec.Capacity)));
                    Capacity = Rec.Capacity;
                    IterDay = IterDay.AddDays(1);
                    Price = Rec.Price;
                    Db.Update(Dif);
                    Db.Update(Rec);
                }
                Record Record = new Record { Price = Price, DateOfCheckIn = Group.DateOfStart, DateOfCheckOut = Group.DateOfEnd, Capacity = Capacity, Count = Count, Hotel = Hotel, Group = Group, Name = Name};
                Db.Update(Group);
                Db.Add(Record);
                Db.SaveChanges();
            }
        }

        internal static void DeleteRecord(string GroupId)
        {
            using (var Db = new AppDbContext())
            {
                
                Record Record = Db.Records.Include(r => r.Group).Where(r => r.GroupId.ToString().ToLower() == GroupId.ToLower()).FirstOrDefault();
                if (Record != null)
                {
                    Groups Group = Db.Groups.Include(e => e.MassEvent).Where(d => d.Id.ToString().ToLower() == GroupId).First();
                    Group.Status = false;
                    DateTime IterDay = Group.DateOfStart;
                    string Name = Record.Name;
                    string HotelId = Record.HotelId.ToString().ToLower();
                    while (IterDay != Group.DateOfEnd.AddDays(1))
                    {
                        DifferenceDataHotel Dif = Db.DifferenceDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Name == Name && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                        RecordDataHotel Rec = Db.RecordDataHotel.Include(d => d.Hotel).Where(d => d.DateIn == IterDay && d.Name == Name && d.Hotel.Id.ToString().ToLower() == HotelId).First();
                        Rec.Count = Rec.Count - Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Group.Count) / Convert.ToDouble(Rec.Capacity)));
                        Dif.Count = Dif.Count + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Group.Count) / Convert.ToDouble(Dif.Capacity)));
                        IterDay = IterDay.AddDays(1);
                        Db.Update(Dif);
                        Db.Update(Rec);
                    }
                    Db.Update(Group);
                    Db.Remove(Record);
                    Db.SaveChanges();
                }
            }
        }

        internal static List<Record> GetRecByHotelId(string HotelId)
        {
            using(var Db = new AppDbContext())
            {
                return Db.Records.Include(r => r.Group).Include(r => r.Hotel).Where(r => r.Hotel.Id.ToString().ToLower() == HotelId.ToLower()).ToList();
            }
        }

        internal static List<EnteredDataHotel> GetEnterDataByNameAndHotelId(string Name, string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.EnteredDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Name).ToList();
            }
        }
        internal static List<DifferenceDataHotel> GetDifDataByNameAndHotelId(string Name, string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.DifferenceDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Name).ToList();
            }
        }
        internal static List<RecordDataHotel> GetRecDataByNameAndHotelId(string Name, string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.RecordDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Name).ToList();
            }
        }
        internal static Record GetRecord(string GroupId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.Records.Include(r => r.Group).Where(r => r.GroupId.ToString().ToLower() == GroupId.ToLower()).FirstOrDefault();
            }
        }

        internal static List<EnteredDataHotel> GetEnterDataByHotelId(string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.EnteredDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId ).ToList();
            }
        }
        internal static List<DifferenceDataHotel> GetDifDataByHotelId( string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.DifferenceDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId).ToList();
            }
        }
        internal static List<RecordDataHotel> GetRecDataByHotelId( string HotelId)
        {
            using (var Db = new AppDbContext())
            {
                return Db.RecordDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId).ToList();
            }
        }
        internal static List<EnteredDataHotel> GetEnterDataByHotelIdAndType(string HotelId,string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.EnteredDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Type).ToList();
            }
        }
        internal static List<DifferenceDataHotel> GetDifDataByHotelIdAndType(string HotelId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.DifferenceDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Type).ToList();
            }
        }
        internal static List<RecordDataHotel> GetRecDataByHotelIdAndType(string HotelId, string Type)
        {
            using (var Db = new AppDbContext())
            {
                return Db.RecordDataHotel.Include(h => h.Hotel).Where(h => h.HotelId.ToString().ToLower() == HotelId && h.Name == Type).ToList();
            }
        }
    }
}
