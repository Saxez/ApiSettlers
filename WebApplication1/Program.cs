using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Linq;
using Project1.Database;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using Microsoft.OpenApi.Models;
using Project1.Data;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Net;
using System.Text;
using System.Globalization;
using Azure.Core;
using Project1.Repositories;
using System.Text.Json.Nodes;
using WebApplication1.Data;
using WebApplication1.Data.Email;
using Project1.Email;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Tls.Crypto;

const string LOGIN_MAP = "/login";
const string LOGOUT_PATH = "/logout";
const string API = "/api";
const string ID = "/{Id}";
const string SEND_CODE = "/send_code";
const string VERIFY_CODE = "/verify_code";
const string RESET_PASS = "/reset_pass";
const string USER = "/user";
const string USERS = "/users";
const string UPDATE_PASS = "/upd_pass";
const string HOTEL = "/hotel";
const string HOTELS = "/hotels";
const string MANAGERS = "/managers";
const string HOTEL_USERS = "/hotel_users";
const string SETTLER = "/settler";
const string SETTLERS = "/settlers";
const string EVENT = "/event";
const string EVENTS = "/events";
const string GROUP = "/group";
const string GROUPS = "/groups";
const string DAYS = "/days";
const string RECORD = "/record";

const string LOGOUT_SIGN = "Data deleted";

const string ADMIN_ROLE = "admin";
const string AMBAS_ROLE = "hotel";
const string MANAGER_ROLE = "manager";
const string SENIOR_MANAGER_ROLE = "senior manager";


WebApplicationBuilder Builder = WebApplication.CreateBuilder();
Builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(Options =>
    {
        Options.LoginPath = LOGIN_MAP;
    });
string ConnectionS = Builder.Configuration.GetConnectionString("Default");
Builder.Services.AddDbContext<AppDbContext>(Options =>
    Options.UseSqlServer(ConnectionS));
ServiceProvider provider = new ServiceCollection().AddMemoryCache().BuildServiceProvider();
IMemoryCache cache = provider.GetService<IMemoryCache>();

Builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

Builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy",
    Builder =>
    {
        Builder.AllowAnyHeader();
        Builder.AllowAnyMethod();
        Builder.AllowAnyOrigin();
    });
});

Builder.Services.AddControllersWithViews();



WebApplication App = Builder.Build();

App.UseAuthentication();
App.UseAuthorization();
App.UseSession();
App.UseCors("CORSPolicy");


InitData Init = new InitData(new AppDbContext());

App.MapPost(API + LOGIN_MAP, async (HttpRequest Request) =>
{
    using (AppDbContext Db = new AppDbContext())
    {
        StreamReader Body = new StreamReader(Request.Body);
        string PostData = await Body.ReadToEndAsync();
        JsonNode Json = JsonNode.Parse(PostData);

        string Email = Json["email"].ToString();
        string Password = Json["password"].ToString();
        string RememberMe = Json["rememberMe"].ToString();
        User? User = Db.Users.ToList().FirstOrDefault(p => p.Email == Email && p.Password == Coder.Encrypt(Password));

        if (User is null) return Results.Unauthorized();
        TimeSpan Expiration = new TimeSpan();
        if (RememberMe == "true")
        {
            Expiration = TimeSpan.FromDays(30);
        }
        else
        {
            Expiration = TimeSpan.FromMinutes(1440);
        }
        cache.Set(User.Id.ToString(), User.Role + "&" + User.Id.ToString(), Expiration);
        return Results.Ok(User);
    }
});

App.MapGet(API + LOGOUT_PATH, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };

    cache.Remove(token);
    return Results.Ok(LOGOUT_SIGN);
});


App.MapPost(API + SEND_CODE, async (HttpRequest Request) =>
{
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string Email = Json["email"].ToString();
    string code = Passworder.GeneratePass(5);
    TimeSpan Expiration = TimeSpan.FromMinutes(5);
    cache.Set(Email, code, Expiration);
    PassSender.SendMessage(Email, code, 2);

    return Results.Ok();
});

App.MapPost(API + VERIFY_CODE, async (HttpRequest Request) =>
{
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string Email = Json["email"].ToString();
    string Code = Json["code"].ToString();
    cache.TryGetValue(Email, out String? CodeToVerif);

    if (CodeToVerif == null)
    {
        return Results.BadRequest();
    }
    if (CodeToVerif != Code)
    {
        return Results.BadRequest();
    }

    return Results.Ok();
});

App.MapPost(API + RESET_PASS, async (HttpRequest Request) =>
{
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string Email = Json["email"].ToString();
    string Password = Json["password"].ToString();
    if (UserRepos.GetUserByEmail(Email) == null)
    {
        return Results.BadRequest();
    }

    User User = UserRepos.GetUserByEmail(Email);
    UserRepos.ResetPassword(User.Id.ToString(), Coder.Encrypt(Password));
    return Results.Ok();
});


App.MapPost(API + USER, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string RoleA = RoleAndId.Split("&")[0];
    if ((RoleA != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    string FullName = Json["fullName"].ToString();
    string Email = Json["email"].ToString();
    string Role = Json["role"].ToString();
    string Password = Passworder.GeneratePass(5);
    if (UserRepos.GetUserByEmail(Email) != null)
    { return Results.BadRequest(); };
    User User = UserRepos.CreateUser(FullName, Email, Coder.Encrypt(Password), Role);
    string Reg = "Регистрация в системе";
    PassSender.SendMessage(Email, Password, 1);
    var JsonOut = new { id = User.Id };
    return Results.Ok(JsonOut);
});

App.MapPut(API + USER + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string RoleA = RoleAndId.Split("&")[0];
    if ((RoleA != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    string FullName = Json["fullName"].ToString();
    string Email = Json["email"].ToString();
    string Role = Json["role"].ToString();
    UserRepos.UpdateUser(Id, FullName, Email, Role);
    return Results.Ok();
});

App.MapDelete(API + USER + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    UserRepos.DeleteUser(Id);
    return Results.Ok();
});

App.MapGet(API + USER, (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    string Id = RoleAndId.Split("&")[1];
    return Results.Ok(UserRepos.GetUserById(Id));
});


App.MapGet(API + USER + ID, (string Id, HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    return Results.Ok(UserRepos.GetUserById(Id));
});

App.MapGet(API + USERS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    return Results.Ok(UserRepos.GetAllUsers());
});

App.MapPut(API + UPDATE_PASS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string OldPassword = Json["oldPassword"].ToString();
    string NewPassword = Json["newPassword"].ToString();
    string Id = RoleAndId.Split("&")[1];
    User User = UserRepos.GetUserById(Id);
    if (User.Password != Coder.Encrypt(OldPassword)) { return Results.BadRequest(); };
    UserRepos.ResetPassword(User.Id.ToString(), Coder.Encrypt(NewPassword));
    return Results.Ok();

});
App.MapGet(API + MANAGERS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    List<User> Users = UserRepos.GetAllUsers();
    List<User> Managers = new List<User>();
    foreach (User User in Users)
    {
        if (User.Role == "manager") { Managers.Add(User); };
    }

    return Results.Ok(Managers);
});

App.MapGet(API + HOTEL_USERS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    List<User> Users = UserRepos.GetAllUsers();
    List<User> Managers = new List<User>();
    foreach (User User in Users)
    {
        if (User.Role == "hotel") { Managers.Add(User); };
    }

    return Results.Ok(Managers);
});


App.MapPost(API + HOTEL, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string EventId = Json["eventId"].ToString();
    string Name = Json["name"].ToString();
    string CheckIn = Json["checkin"].ToString();
    string CheckOut = Json["checkout"].ToString();
    string CancelCondition = Json["cancelCondition"].ToString();
    string HotelUserId = Json["hotelUserId"]?.ToString();
    JsonNode ManagerUsersIdJson = Json["managerUsersId"];
    string Phone = Json["phone"].ToString();
    string Email = Json["email"].ToString();
    string Link = Json["link"].ToString();
    string Adress = Json["adress"].ToString();
    int Stars = Int32.Parse(Json["stars"].ToString());
    string IdHotel = HotelRepos.CreateHotel(Name, Adress, CancelCondition, CheckIn, CheckOut, Stars, EventId, HotelUserId, Phone, Email, Link);
    string[] ManagerUsersId = ManagerUsersIdJson.Deserialize<string[]>();
    SettlerRepos.BindHotels(IdHotel, ManagerUsersId);
    var JsonOut = new { id = IdHotel };
    return Results.Ok(JsonOut);
});

App.MapGet(API + HOTELS + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    string EventId = Id;
    List<Hotel> ListHot = new List<Hotel>();
    if (Role == SENIOR_MANAGER_ROLE || Role == ADMIN_ROLE)
    {
        ListHot = HotelRepos.GetAllHotelsByEventId(EventId);
    }
    else if (Role == MANAGER_ROLE)
        ListHot = HotelRepos.GetAllHotelsToManager(RoleAndId.Split("&")[1], EventId);
    else if (Role == AMBAS_ROLE)
    {
        ListHot = HotelRepos.GetAllHotelsToAmbas(RoleAndId.Split("&")[1], EventId);
    }
    else
        return Results.BadRequest();
    List<object> OutHot = new List<object>();
    foreach (Hotel Hotel in ListHot)
    {
        object Out = new { Id = Hotel.Id.ToString().ToLower(), Name = Hotel.Name };
        OutHot.Add(Out);
    }
    return Results.Ok(OutHot);
});

App.MapGet(API + HOTEL + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }

    Hotel Hotel = HotelRepos.GetHotelById(Id);
    List<User> Managers = HotelRepos.GetAllManagersToHotel(Id);
    List<Settler> Guests = SettlerRepos.GetAllSettlersFromHotel(Id.ToString());
    List<object> Data = new List<object>();
    foreach (Settler Settler in Guests)
    {
        Groups Group = GroupRepos.GetGroupById(Settler.GroupsId.ToString());
        DateTime IterDay = Group.DateOfStart;
        List<DateTime> slots = new List<DateTime>();
        while (Group.DateOfEnd.AddDays(1) > IterDay)
        {
            slots.Add(IterDay);
            IterDay = IterDay.AddDays(1);
        }
        Record Record = JournalRepos.GetRecord(Settler.GroupsId.ToString());
        object DataSet = new { id = Settler.Id, groupName = Group.Name, guestFullName = Settler.FullName, capacity = Group.Count, checkIn = Group.DateOfStart, checkOut = Group.DateOfEnd, slots = slots, dayNumber = slots.Count, price = Record.Price / slots.Count, total = Record.Price, categoryName = Record.Name };
        Data.Add(DataSet);
    }
    var Types = JournalRepos.GetAllTypes(Hotel.Id.ToString());

    var EntRecs = new List<object>();
    var DifRecs = new List<object>();
    var RecRecs = new List<object>();
    foreach (var Type in Types)
    {
        List<int> CounterEnt = new List<int>();
        List<int> CounterDif = new List<int>();
        List<int> CounterRec = new List<int>();
        var EntData = JournalRepos.GetEnterDataByNameAndHotelId(Type.Name, Hotel.Id.ToString());
        var DifData = JournalRepos.GetDifDataByNameAndHotelId(Type.Name, Hotel.Id.ToString());
        var RecData = JournalRepos.GetRecDataByNameAndHotelId(Type.Name, Hotel.Id.ToString());
        for (int i = 0; i < EntData.Count; i++)
        {
            CounterEnt.Add(EntData[i].Count);
            CounterDif.Add(DifData[i].Count);
            CounterRec.Add(RecData[i].Count);
        }
        EntRecs.Add(new { categoryName = Type.Name, categoryType = Type.Type, capacity = EntData[0].Capacity, slots = CounterEnt, price = EntData[0].Price });
        DifRecs.Add(new { categoryName = Type.Name, categoryType = Type.Type, capacity = DifData[0].Capacity, slots = CounterDif, price = DifData[0].Price });
        RecRecs.Add(new { categoryName = Type.Name, categoryType = Type.Type, capacity = RecData[0].Capacity, slots = CounterRec, price = RecData[0].Price });
    }
    var Json = new { name = Hotel.Name, checkin = Hotel.CheckIn, checkout = Hotel.CheckOut, cancelCondition = Hotel.CancelCondition, hotelUser = Hotel.HotelUser, managerUsers = Managers, phone = Hotel.Phone, email = Hotel.Email, link = Hotel.Link, address = Hotel.Adress, stars = Hotel.Stars, guestsData = Data, hotelBlockData = EntRecs, factBlockData = RecRecs, difBlockData = DifRecs };
    return Results.Ok(Json);
});

App.MapPut(API + HOTEL + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Name = Json["name"].ToString();
    var CheckIn = Json["checkin"].ToString();
    var CheckOut = Json["checkout"].ToString();
    var CancelCondition = Json["cancelCondition"].ToString();
    var HotelUserId = Json["hotelUserId"]?.ToString();
    var ManagerUsersIdJson = Json["managerUsersId"];
    var Phone = Json["phone"].ToString();
    var Email = Json["email"].ToString();
    var Link = Json["link"].ToString();
    var Adress = Json["adress"].ToString();
    var Stars = Int32.Parse(Json["stars"].ToString());
    var ManagerUsersId = ManagerUsersIdJson.Deserialize<string[]>();
    HotelRepos.UpdateHotelInfo(Id, Name, Adress, CancelCondition, CheckIn, CheckOut, Stars, HotelUserId, Phone, Email, Link);
    SettlerRepos.DeleteBinds(Id);
    SettlerRepos.BindHotels(Id, ManagerUsersId);
    return Results.Ok();
});

App.MapDelete(API + HOTEL + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    HotelRepos.DeleteHotel(Id);
    return Results.Ok();
});


App.MapPost(API + SETTLER, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"].ToString();
    var Contact = Json["contact"].ToString();
    var IdGroup = Json["groupId"].ToString();
    var SettlerId = SettlerRepos.CreateSettler(FullName, Contact, IdGroup);
    var JsonOut = new { id = SettlerId };
    return Results.Ok(JsonOut);

});

App.MapGet(API + SETTLER + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    return Results.Ok(SettlerRepos.GetSettlerById(Id));
});

App.MapGet(API + SETTLERS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    return Results.Ok(SettlerRepos.GetAllSettlers());
});

App.MapPut(API + SETTLER + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"]?.ToString();
    var Contact = Json["contact"]?.ToString();
    SettlerRepos.UpdateSettler(Id, FullName, Contact);
    return Results.Ok();
});

App.MapDelete(API + SETTLER + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    SettlerRepos.DeleteSettler(Id);
    return Results.Ok();
});



App.MapPost(API + EVENT, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Name = Json["name"].ToString();
    var DateOfStart = DateTime.ParseExact(Json["start"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var DateOfEnd = DateTime.ParseExact(Json["end"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

    if (EventRepos.GetEventByName(Name) != null)
    {
        return Results.BadRequest();
    }
    var EventId = EventRepos.CreateEvent(Name, DateOfStart, DateOfEnd);
    var JsonOut = new { id = EventId };
    return Results.Ok(JsonOut);

});

App.MapPut(API + EVENT + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Name = Json["name"].ToString();
    var DateOfStart = DateTime.ParseExact(Json["start"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var DateOfEnd = DateTime.ParseExact(Json["end"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

    EventRepos.UpdateEventInfo(Id, Name, DateOfStart, DateOfEnd);
    return Results.Ok();
});

App.MapGet(API + EVENTS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    return Results.Ok(EventRepos.GetAllEvents());
});

App.MapGet(API + EVENT + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    return Results.Ok(EventRepos.GetEventById(Id));
});

App.MapDelete(API + EVENT + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE))
    {
        return Results.Unauthorized();
    }
    GroupRepos.DeleteGroupsByEventId(Id);
    HotelRepos.DeleteHotelsByEventId(Id);
    EventRepos.DeleteEvent(Id);
    return Results.Ok();
});


App.MapPost(API + GROUP, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    string EventId = Json["eventId"].ToString();
    string Name = Json["name"].ToString();
    int PrefferedType = Int32.Parse(Json["preferredCategoryType"].ToString());
    DateTime DateOfStart = DateTime.ParseExact(Json["checkin"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    DateTime DateOfEnd = DateTime.ParseExact(Json["checkout"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    string ManagerId = Json["managerId"]?.ToString();
    var GroupId = GroupRepos.CreateGroup(Name, 0, EventId, ManagerId, PrefferedType, DateOfStart, DateOfEnd);
    var JsonOut = new { id = GroupId };
    return Results.Ok(JsonOut);

});

App.MapGet(API + "/api/all_groups_by_event/{id}", async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }

    return Results.Ok(GroupRepos.GetGroupsByEventId(Id));
});


App.MapGet(API + GROUPS + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    string EventId = Id;
    if (RoleAndId.Split("&")[0] == "admin")
    {
        return Results.Ok(GroupRepos.GetAllGroups(EventId));
    }
    var IdUser = RoleAndId.Split("&")[1];

    return Results.Ok(GroupRepos.GetGroupsByOwnerId(IdUser, EventId));
});

App.MapDelete(API + GROUP + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }

    SettlerRepos.DeleteSettlersByGroupId(Id);

    GroupRepos.DeleteGroup(Id);
    return Results.Ok();
});

App.MapPut(API + GROUP + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string Name = Json["name"].ToString();
    int PrefferedType = Int32.Parse(Json["preferredCategoryType"].ToString());
    DateTime DateOfStart = DateTime.ParseExact(Json["checkin"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    DateTime DateOfEnd = DateTime.ParseExact(Json["checkout"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    string ManagerId = Json["managerId"]?.ToString();
    GroupRepos.UpdateGroup(Id, Name, ManagerId, DateOfStart, DateOfEnd);
    return Results.Ok();
});



App.MapPost(API + DAYS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string HotelId = Json["hotelId"].ToString();
    string Name = Json["categoryName"].ToString();
    var Type = Int32.Parse(Json["categoryType"].ToString());
    var Capacity = Int32.Parse(Json["capacity"].ToString());
    var Price = Int32.Parse(Json["price"].ToString());
    var Days = Json["slots"];
    Hotel Hotel = HotelRepos.GetHotelById(HotelId);
    MassEvent Event = EventRepos.GetEventById(Hotel.MassEventId.ToString().ToLower());
    var DateOfStart = Event.DateOfStart;
    var DateOfEnd = Event.DateOfEnd;
    int i = 0;
    while (DateOfEnd.AddDays(1) > DateOfStart)
    {
        if (Days[i].ToString() == null)
        {
            return Results.BadRequest();
        }
        JournalRepos.InitDays(DateOfStart, Int32.Parse(Days[i].ToString()), Price, Capacity, Type, Hotel, Name);
        i += 1;
        DateOfStart = DateOfStart.AddDays(1);
    }
    return Results.Ok();
});


App.MapPut(API + DAYS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string HotelId = Json["hotelId"].ToString();
    string Name = Json["categoryName"].ToString();
    if (!JournalRepos.CheckExist(HotelId, Name))
    {
        return Results.NotFound();
    }
    var Type = Int32.Parse(Json["categoryType"].ToString());
    var Capacity = Int32.Parse(Json["capacity"].ToString());
    var Price = Int32.Parse(Json["price"].ToString());
    var Days = Json["slots"];
    Hotel Hotel = HotelRepos.GetHotelById(HotelId);
    MassEvent Event = EventRepos.GetEventById(Hotel.MassEventId.ToString().ToLower());
    var DateOfStart = Event.DateOfStart;
    var DateOfEnd = Event.DateOfEnd;
    int i = 0;
    while (DateOfEnd.AddDays(1) > DateOfStart)
    {
        if (Days[i].ToString() == null)
        {
            return Results.BadRequest();
        }
        JournalRepos.UpdateDays(DateOfStart, Int32.Parse(Days[i].ToString()), Price, Capacity, Type, Hotel, Name);
        i += 1;
        DateOfStart = DateOfStart.AddDays(1);
    }
    return Results.Ok();
});



App.MapDelete(API + DAYS, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != AMBAS_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string HotelId = Json["hotelId"].ToString();
    string Name = Json["name"].ToString();

    JournalRepos.DelDays(HotelId, Name);
    return Results.Ok();
});

App.MapGet(API + "/relev_hotels/{Id}", async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    var IdUser = RoleAndId.Split("&")[0];
    string IdGroup = Id;
    var Group = GroupRepos.GetGroupById(IdGroup.ToLower());
    var EventId = Group.MassEventId.ToString();
    List<Hotel> Hotels = new List<Hotel>();
    if (Role == MANAGER_ROLE)
    {
        Hotels = HotelRepos.GetAllHotelsToManager(IdUser, EventId);
    }
    else
    {
        Hotels = HotelRepos.GetAllHotelsByEventId(EventId);
    }
    List<object> RelHotels = new List<object>();
    foreach (Hotel Hotel in Hotels)
    {
        List<TypesOfDays> Types = JournalRepos.GetAllTypes(Hotel.Id.ToString());
        foreach (TypesOfDays TypeOfDays in Types)
        {
            if (JournalRepos.isTypeRel(TypeOfDays, Group.Count, Group.DateOfStart, Group.DateOfEnd))
            {
                object rec = new { hotelId = Hotel.Id, hotelName = Hotel.Name, categoryName = TypeOfDays.Name, CategoryType = TypeOfDays.Type };
                RelHotels.Add(rec);
            }
        }
    }
    return Results.Ok(RelHotels);
});


App.MapPost(API + RECORD, async (HttpRequest Request) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    StreamReader Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string IdGroup = Json["groupId"].ToString();
    string HotelId = Json["hotelId"].ToString();
    string CategoryName = Json["categoryName"].ToString();


    JournalRepos.CreateRecord(IdGroup, CategoryName, HotelId);
    return Results.Ok();

});

App.MapDelete(API + RECORD + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }

    JournalRepos.DeleteRecord(Id);
    return Results.Ok();
});

App.MapGet(API + RECORD + ID, async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    string EventId = Id;
    List<Hotel> Hotels = HotelRepos.GetAllHotelsByEventId(EventId);
    List<object> Recs = new List<object>();
    foreach (Hotel Hotel in Hotels)
    {
        List<Record> Records = JournalRepos.GetRecByHotelId(Hotel.Id.ToString());
        foreach (Record Record in Records)
        {
            Groups Group = GroupRepos.GetGroupById(Record.GroupId.ToString());
            var DifDays = (Record.DateOfCheckOut - Record.DateOfCheckIn).TotalDays;
            object Rec = new { id = Record.Id, hotelName = Hotel.Name, groupName = Group.Name, capacity = Record.Capacity, slots = Record.Count, categoryName = Record.Name, checkin = Record.DateOfCheckIn, checkout = Record.DateOfCheckOut, price = Record.Price, dayNumber = DifDays, total = Record.Price * Record.Count * DifDays };
            Recs.Add(Rec);
        }
    }
    return Results.Ok(Recs);
});


App.MapGet(API + "/get_journal_statistic/{Id}", async (HttpRequest Request, string Id) =>
{
    string token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Role = RoleAndId.Split("&")[0];
    if ((Role != ADMIN_ROLE) && (Role != MANAGER_ROLE) && (Role != SENIOR_MANAGER_ROLE))
    {
        return Results.Unauthorized();
    }
    string IdEvent = Id;

    List<object> AllHotelsData = new List<object>();
    List<Hotel> Hotels = HotelRepos.GetAllHotelsByEventId(IdEvent);
    foreach (Hotel Hotel in Hotels)
    {
        var Types = JournalRepos.GetAllTypes(Hotel.Id.ToString());
        List<object> EnterStrings = new List<object>();
        List<object> DifStrings = new List<object>();
        List<object> RecStrings = new List<object>();
        foreach (var TypeRec in Types)
        {
            List<int> Enter = new List<int>();
            List<int> Dif = new List<int>();
            List<int> Rec = new List<int>();
            int cap = 0;
            int price = 0;
            var EntData = JournalRepos.GetEnterDataByNameAndHotelId(TypeRec.Name, Hotel.Id.ToString());
            var DifData = JournalRepos.GetDifDataByNameAndHotelId(TypeRec.Name, Hotel.Id.ToString());
            var RecData = JournalRepos.GetRecDataByNameAndHotelId(TypeRec.Name, Hotel.Id.ToString());
            foreach (var EntDate in EntData)
            {
                Enter.Add(EntDate.Count);
                cap = EntDate.Capacity;
                price = EntDate.Price;
            }

            foreach (var DifDate in DifData)
            {
                Dif.Add(DifDate.Count);
            }

            foreach (var RecDate in RecData)
            {
                Rec.Add(RecDate.Count);
            }
            object EnterSt = new { hotelName = Hotel.Name, categoryName = TypeRec.Name, block = 0, capacity = cap, slots = Enter, price = price };
            object DifSt = new { hotelName = Hotel.Name, categoryName = TypeRec.Name, block = 1, capacity = cap, slots = Dif, price = price };
            object RecSt = new { hotelName = Hotel.Name, categoryName = TypeRec.Name, block = 2, capacity = cap, slots = Rec, price = price };
            EnterStrings.Add(EnterSt);
            DifStrings.Add(DifSt);
            RecStrings.Add(RecSt);
        }
        foreach (var OneSt in EnterStrings)
        {
            AllHotelsData.Add(OneSt);
        }
        foreach (var OneSt in DifStrings)
        {
            AllHotelsData.Add(OneSt);
        }
        foreach (var OneSt in RecStrings)
        {
            AllHotelsData.Add(OneSt);
        }
    }
    return Results.Ok(AllHotelsData);
});
App.Map("test", () => "Test");

App.Run();