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
using Project1.Email;
using System.Text.Json.Nodes;
using WebApplication1.Data;

const string ACCESS_DENIED_PATH = "/accessdenied";
const string LOGIN_MAP = "/login";
const string LOGOUT_PATH = "/logout";
const string ADMIN_MAP = "/admin";
const string ALL_HOTELS = "/hotels";
const string ALL_EVENTS = "/events";
const string MY_GROUPS_MAP = "/my_groups";
const string INFO_MAP = "/info";
const string REG_USERS = "/reg_users";
const string REG_SETTLERS = "/reg_settlers";
const string MY_INFO = "/my_info";
const string INFO_USER = "/user/{id}";
const string UPDATE_HOTEL = "/update_hotel/{id}";
const string ROLE_CHECK = "/role_check";
const string ONE_HOTEL = "/hotel/{id}";
const string REGISTRATION = "/registration";
const string ADD_HOTELS_TO_MANAGER = "/hotel_x_manager";
const string CREATE_HOTEL = "/create_hotel";
const string HOTELS_BY_EV_ID = "/hotels_by_event/{id}";
const string HOTELS_BY_MAN_ID = "/hotels_by_manager/{id}";
const string DEL_HOTEL = "/delete_hotel/{id}";
const string SETTLER_BY_ID = "/settler/{id}";
const string SETTLER_BY_HOT_ID = "/settlers_from_hotel/{id}";
const string ALL_SETTLERS = "/all_settlers";
const string SET_HOTEL_TO_SET = "/set_hotel_to_settler";
const string UPD_SETTLER = "/update_settler/{id}";
const string DEL_SET = "/delete_settler/{id}";
const string CREATE_EVENT = "/create_event";
const string EVENT_BY_ID = "/event/{id}";
const string UPD_EVENT = "/update_event/{id}";
const string DEL_EVENT = "/delete_event/{id}";

const string LOGOUT_SIGN = "Data deleted";
const string ACCESS_DENIED = "Access Denied";
const string BAD_REQUEST_EMAIL_OR_PASSWORD = "Email and/or password are not set";
const string EMAIL = "email";
const string PASSWORD = "password";
const string FIRST_NAME = "first_name";
const string LAST_NAME = "last_name";
const string GENDER = "gender";
const string ADDITIONAL_PEOPLE = "additional_people";
const string PREFFERED_TYPE = "preffered_type";

const string ADMIN_ROLE = "admin";
const string AMBAS_ROLE = "hotel_ambas";
const string MANAGER_ROLE = "manager";
const string MAIN_MANAGER_ROLE = "main_manager";



var Builder = WebApplication.CreateBuilder();
Builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(Options =>
    {
        Options.LoginPath = LOGIN_MAP;
        Options.AccessDeniedPath = ACCESS_DENIED_PATH;
    });

string ConnectionS = Builder.Configuration.GetConnectionString("Default");

Builder.Services.AddDbContext<AppDbContext>(Options =>
    Options.UseSqlServer(ConnectionS));





var provider = new ServiceCollection().AddMemoryCache().BuildServiceProvider();

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



var App = Builder.Build();

App.UseAuthentication();
App.UseAuthorization();
App.UseSession();
App.UseCors("CORSPolicy");


InitData Init = new InitData(new AppDbContext());

App.MapGet(ROLE_CHECK, async (HttpRequest Request) =>
{
    cache.TryGetValue("Test", out String? test);
    cache.TryGetValue(Request.Headers.Authorization.ToString(), out String? Role);
    if (Role != null) return $"Role: {Role}";
    return $"role not found";
});


App.MapPost(LOGIN_MAP, async (HttpRequest Request) =>
{
    using (var Db = new AppDbContext())
    {
        var Body = new StreamReader(Request.Body);
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

App.MapGet(LOGOUT_PATH, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };

    cache.Remove(token);
    return Results.Ok(LOGOUT_SIGN);
});

App.MapPost(REGISTRATION, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"].ToString();
    var Email = Json["email"].ToString();
    var Role = Json["role"].ToString();
    var Password = Passworder.GeneratePass(5);
    if (UserRepos.GetUserByEmailAndPassword(Email, Password) != null)
    { return Results.BadRequest(); };
    PassSender.SendMessage(Email, Password, "Регистрация в системе");
    User User = UserRepos.CreateUser(FullName, Email, Coder.Encrypt(Password), Role);
    return Results.Ok(User.Id);
});

App.MapPost("/send_code", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Email = Json["email"].ToString();
    var code = Passworder.GeneratePass(5);
    TimeSpan Expiration = TimeSpan.FromMinutes(5);
    cache.Set(Email, code, Expiration);
    PassSender.SendMessage(Email, "Код для восстановления пароля:" + code + ". Действует только 5 минут", "Код для восстановления пароля");

    return Results.Ok();
});

App.MapGet("/verify_code", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Email = Json["email"].ToString();
    var Code = Json["code"].ToString();
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

App.MapPost("/reset_pass", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Email = Json["email"].ToString();
    var Password = Json["password"].ToString();
    if (UserRepos.GetUserByEmail(Email) == null)
    {
        return Results.BadRequest();
    }

    User User = UserRepos.GetUserByEmail(Email);
    UserRepos.ResetPassword(User.Id.ToString(), Coder.Encrypt(Password));
    return Results.Ok();
});

App.MapPost("/upd_user/{id}", async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"].ToString();
    var Email = Json["email"].ToString();
    var Role = Json["role"].ToString();
    UserRepos.UpdateUser(Id, FullName, Email, Role);
    return Results.Ok();
});

App.MapDelete("/del_user/{id}", async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    UserRepos.DeleteUser(Id);
    return Results.Ok();
});

App.MapGet(MY_INFO, (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    string Id = RoleAndId.Split("&")[1];
    return Results.Ok(UserRepos.GetUserById(Id));
});

App.MapPost("/upd_pass", async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var OldPassword = Json["oldPassword"].ToString();
    var NewPassword = Json["newPassword"].ToString();
    var Id = RoleAndId.Split("&")[1];
    User User = UserRepos.GetUserById(Id);
    if (User.Password != Coder.Encrypt(OldPassword)) { return Results.BadRequest(); };
    UserRepos.ResetPassword(User.Id.ToString(), Coder.Encrypt(NewPassword));
    return Results.Ok();

});

App.MapGet(INFO_USER, (string Id, HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    return Results.Ok(UserRepos.GetUserById(Id));
});

App.MapGet(ADMIN_MAP, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    return Results.Ok(UserRepos.GetAllUsers());
});

App.MapGet("/get_all_managers", async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.Unauthorized(); };
    List<User> Users = UserRepos.GetAllUsers();
    List<User> Managers = new List<User>();
    foreach (User User in Users)
    {
        if (User.Role == "manager") { Managers.Add(User); };
    }

    return Results.Ok(Managers);
});


App.MapPost(CREATE_HOTEL, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var EventId = Json["eventId"].ToString();
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
    var IdHotel = HotelRepos.CreateHotel(Name, Adress, CancelCondition, CheckIn, CheckOut, Stars, EventId, HotelUserId, Phone, Email, Link);
    var ManagerUsersId = ManagerUsersIdJson.Deserialize<string[]>();
    SettlerRepos.BindHotels(IdHotel, ManagerUsersId);

    return Results.Ok(IdHotel);
});

App.MapGet(ALL_HOTELS, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.Unauthorized(); };
    var role = RoleAndId.Split("&")[0];
    List<Hotel> ListHot = new List<Hotel>();
    if (role == MAIN_MANAGER_ROLE || role == ADMIN_ROLE)
    {
        ListHot = HotelRepos.GetAllHotels();
    }
    else if (role == MANAGER_ROLE)
        ListHot = HotelRepos.GetAllHotelsToManager(RoleAndId.Split("&")[1]);
    else
        return Results.BadRequest();
    List<HotelToOut> OutHot = new List<HotelToOut>();
    foreach(var Hotel in ListHot)
    {
        HotelToOut Out = new HotelToOut { Id = Hotel.Id.ToString().ToLower(), Name = Hotel.Name };
        OutHot.Add(Out);
    }
    return Results.Ok(OutHot);
});

App.MapGet(ONE_HOTEL, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };


    return Results.Ok(HotelRepos.GetHotelById(Id));
});

App.MapPost(UPDATE_HOTEL, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    var Body = new StreamReader(Request.Body);
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

App.MapGet(HOTELS_BY_EV_ID, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };

    return Results.Ok(HotelRepos.GetAllHotelsByEventId(Id));
});

App.MapGet(HOTELS_BY_MAN_ID, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };

    return Results.Ok(HotelRepos.GetAllHotelsToManager(Id));
});

App.MapDelete(DEL_HOTEL, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    HotelRepos.DeleteHotel(Id);
    return Results.Ok();
});


App.MapPost(REG_SETTLERS, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"].ToString();
    var Contact = Json["contact"].ToString();
    var IdGroup = Json["groupId"].ToString();
    return Results.Ok(SettlerRepos.CreateSettler(FullName, Contact, IdGroup));
});

App.MapGet(SETTLER_BY_ID, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    return Results.Ok(SettlerRepos.GetSettlerById(Id));
});

App.MapGet(ALL_SETTLERS, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    return Results.Ok(SettlerRepos.GetAllSettlers());
});

App.MapPost(UPD_SETTLER, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    var FullName = Json["fullName"].ToString();
    var Additional = Json["additionalPeople"].ToString();
    var Contact = Json["contact"].ToString();
    SettlerRepos.UpdateSettler(Id, FullName, Int32.Parse(Additional), Contact);
    return Results.Ok();
});

App.MapDelete(DEL_SET, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    SettlerRepos.DeleteSettler(Id);
    return Results.Ok();
});



App.MapPost(CREATE_EVENT, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Name = Json["name"].ToString();
    var DateOfStart = DateTime.ParseExact(Json["start"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var DateOfEnd = DateTime.ParseExact(Json["end"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

    if (EventRepos.GetEventByName(Name) != null)
    {
        return Results.BadRequest();
    }


    return Results.Ok(EventRepos.CreateEvent(Name, DateOfStart, DateOfEnd));
});

App.MapPost(UPD_EVENT, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    var Name = Json["name"].ToString();
    var DateOfStart = DateTime.ParseExact(Json["start"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var DateOfEnd = DateTime.ParseExact(Json["end"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);

    EventRepos.UpdateEventInfo(Id, Name, DateOfStart, DateOfEnd);
    return Results.Ok();
});

App.MapGet(ALL_EVENTS, async (HttpRequest Request) =>
{
    return Results.Ok(EventRepos.GetAllEvents());
});

App.MapGet(EVENT_BY_ID, async (HttpRequest Request, string Id) =>
{
    return EventRepos.GetEventById(Id);
});

App.MapDelete(DEL_EVENT, async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    SettlerRepos.DeleteSettlersByGroupId(Id);
    GroupRepos.DeleteGroupsByEventId(Id);
    HotelRepos.DeleteHotelsByEventId(Id);
    EventRepos.DeleteEvent(Id);
    return Results.Ok();
});


App.MapPost("/create_group", async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);

    string EventId = Json["eventId"].ToString();
    string Name = Json["name"].ToString();
    int PrefferedType = Int32.Parse(Json["preferredCategoryType"].ToString());
    DateTime DateOfStart = DateTime.ParseExact(Json["checkin"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    DateTime DateOfEnd = DateTime.ParseExact(Json["checkout"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    string ManagerId = Json["managerId"].ToString();
    return Results.Ok(GroupRepos.CreateGroup(Name, 0, EventId, ManagerId, PrefferedType, DateOfStart, DateOfEnd));
});

App.MapGet("/all_groups_by_event/{id}", async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }

    return Results.Ok(GroupRepos.GetGroupsByEventId(Id));
});

App.MapGet("/get_all_groups", async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }

    return Results.Ok(GroupRepos.GetAllGroups());
});

App.MapGet(MY_GROUPS_MAP, async (HttpRequest Request) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] == "admin")
    {
        return Results.Ok(GroupRepos.GetAllGroups());
    }
    var Id = RoleAndId.Split("&")[1];

    return Results.Ok(GroupRepos.GetGroupsByOwnerId(Id));
});

App.MapDelete("/del_group/{id}", async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }

    SettlerRepos.DeleteSettlersByGroupId(Id);

    GroupRepos.DeleteGroup(Id);
    return Results.Ok();
});

App.MapPost("/upd_group/{id}", async (HttpRequest Request, string Id) =>
{
    var token = Request.Headers.Authorization.ToString();
    cache.TryGetValue(token, out String? RoleAndId);
    if (RoleAndId == null) { return Results.BadRequest(); };
    if (RoleAndId.Split("&")[0] != "admin")
    {
        return Results.BadRequest();
    }
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    JsonNode Json = JsonNode.Parse(PostData);
    string Name = Json["name"].ToString();
    int PrefferedType = Int32.Parse(Json["preferredCategoryType"].ToString());
    DateTime DateOfStart = DateTime.ParseExact(Json["checkin"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    DateTime DateOfEnd = DateTime.ParseExact(Json["checkout"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
    string ManagerId = Json["managerId"].ToString();
    GroupRepos.UpdateGroup(Id, Name, ManagerId, DateOfStart, DateOfEnd);
    return Results.Ok();
});



App.MapPost("/add_days", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"", "\", \"Capacity\": \"", "\", \"Price\": \"", "\", \"Days\":" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var DirtDays = Data[5].Replace("\\", "").Replace("\r\n", "").Replace(" ", "").Replace("[{", "").Replace("]}", "").Replace("\"", "");
    var HotelId = Data[1];
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    var Type = Data[2];
    var Capacity = Data[3];
    var Price = Data[4];
    string[] Days = DirtDays.Split("},{");
    foreach (string Day in Days)
    {
        string[] DayData = Day.Replace("Date:", "").Replace("Count:", "").Replace("}", "").Split(",");
        DateTime Date = DateTime.Parse(DayData[0].Replace(".", "/"));
        RecInJournal Rec = new RecInJournal { Type = Type, Capacity = Int32.Parse(Capacity), Count = Int32.Parse(DayData[1]), Date = Date, Price = Int32.Parse(Price) };
        JournalRepos.InitDays(Rec, HotelId.ToString().ToLower(), EventId.ToString().ToLower());
    }
});


App.MapPost("/upd_days", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"", "\", \"Capacity\": \"", "\", \"Price\": \"", "\", \"Days\":" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var DirtDays = Data[5].Replace("\\", "").Replace("\r\n", "").Replace(" ", "").Replace("[{", "").Replace("]}", "").Replace("\"", "");
    var HotelId = Data[1];
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    var Type = Data[2];
    var Capacity = Data[3];
    var Price = Data[4];
    string[] Days = DirtDays.Split("},{");
    foreach (string Day in Days)
    {
        string[] DayData = Day.Replace("Date:", "").Replace("Count:", "").Replace("}", "").Split(",");
        DateTime Date = DateTime.Parse(DayData[0].Replace(".", "/"));
        RecInJournal Rec = new RecInJournal { Type = Type, Capacity = Int32.Parse(Capacity), Count = Int32.Parse(DayData[1]), Date = Date, Price = Int32.Parse(Price) };
        JournalRepos.UpdateDays(Rec, HotelId.ToString().ToLower(), EventId.ToString().ToLower());
    }
});

App.MapGet("/get_enter_data_with_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1];
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    var Type = Data[2].Replace("\"}", "");
    return Results.Ok(JournalRepos.GetEnteredDataWithType(HotelId, EventId.ToString(), Type));
});

App.MapGet("/get_enter_data_without_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1].Replace("\"}", "");
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    return Results.Ok(JournalRepos.GetEnteredDataWithoutType(HotelId, EventId.ToString()));
});



App.MapGet("/get_dif_data_with_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1];
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    var Type = Data[2].Replace("\"}", "");
    return Results.Ok(JournalRepos.GetDifferenceDataWithType(HotelId, EventId.ToString(), Type));

});

App.MapGet("/get_dif_data_without_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1].Replace("\"}", "");
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    return Results.Ok(JournalRepos.GetDifferenceDataWithoutType(HotelId, EventId.ToString()));
});

App.MapGet("/get_record_data_with_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1];
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    var Type = Data[2].Replace("\"}", "");
    return Results.Ok(JournalRepos.GetRecordDataWithType(HotelId, EventId.ToString(), Type));
});

App.MapGet("/get_record_data_without_type", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var HotelId = Data[1].Replace("\"}", "");
    var Hotel = HotelRepos.GetHotelById(HotelId);
    var EventId = Hotel.MassEventId;
    return Results.Ok(JournalRepos.GetRecordDataWithoutType(HotelId, EventId.ToString()));
});

App.MapDelete("/del_days", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdHotel\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

    JournalRepos.DelDays(Data[1], Data[2].Replace("\" }", ""));
    return Results.Ok();
});

App.MapGet("/get_relev_hotels", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdGroup\": \"", "\", \"Type\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

    var IdGroup = Data[1];
    var Type = Data[2].Replace("\"}", "");
    var Group = GroupRepos.GetGroupById(IdGroup.ToLower());
    var Hotels = HotelRepos.GetAllHotelsByEventId(Group.MassEventId.ToString().ToLower());
    List<Hotel> RelHotels = new List<Hotel>();
    foreach (Hotel Hotel in Hotels)
    {
        if (JournalRepos.isDaysRel(Hotel.Id.ToString().ToLower(), Group.Count, Group.DateOfStart, Group.DateOfEnd))
        {
            RelHotels.Add(Hotel);
        }
    }
    return Results.Ok(RelHotels);
});


App.MapPost("/record", async (HttpRequest Request) =>
{
    var Body = new StreamReader(Request.Body);
    string PostData = await Body.ReadToEndAsync();
    string[] Separators = { "\"IdGroup\": \"", "\", \"Type\": \"", "\", \"IdHotel\": \"" };
    var Data = PostData.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

    var IdGroup = Data[1];
    var Type = Data[2];
    var IdHotel = Data[3].Replace("\"}", "");
    JournalRepos.CreateRecord(IdGroup, Type, IdHotel);
    return Results.Ok();

});

App.Run();