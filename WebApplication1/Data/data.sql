
DROP DATABASE HotelDB

use HotelDB
create database HotelDB

CREATE TABLE MassEvents(Id UNIQUEIDENTIFIER  PRIMARY KEY DEFAULT NEWID(), Name NVARCHAR(50), DateOfStart DATE , DateOfEnd DATE );

CREATE TABLE Users( Id UNIQUEIDENTIFIER  PRIMARY KEY DEFAULT NEWID(),FullName NVARCHAR(50) ,Email NVARCHAR(50) ,Role nvarchar(50),Password NVARCHAR(50));

CREATE TABLE Hotels(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),Name NVARCHAR(50) ,Adress NVARCHAR(100),CancelCondition NVARCHAR(1000),CheckIn NVARCHAR(50),CheckOut NVARCHAR(50),Phone NVARCHAR(50),Email NVARCHAR(50),Link NVARCHAR(50),Stars INT,MassEventId UNIQUEIDENTIFIER ,FOREIGN KEY(MassEventId) REFERENCES MassEvents,HotelUserId UNIQUEIDENTIFIER ,FOREIGN KEY(HotelUserId) REFERENCES Users);

CREATE TABLE UserXHotels(UserId UNIQUEIDENTIFIER, HotelId UNIQUEIDENTIFIER,PRIMARY KEY (UserId ,HotelId), FOREIGN KEY(UserId)  REFERENCES Users, FOREIGN KEY(HotelId) REFERENCES Hotels);

CREATE TABLE Groups(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),Count INT,Name NVARCHAR(50),DateOfStart DATE ,DateOfEnd DATE,Status BIT,PreferredType INT,ManagerId UNIQUEIDENTIFIER,MassEventId UNIQUEIDENTIFIER,FOREIGN KEY (ManagerId) REFERENCES Users,FOREIGN KEY (MassEventId) REFERENCES MassEvents);

CREATE TABLE Settler(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),FullName NVARCHAR(50),Contact NVARCHAR(50),GroupsId UNIQUEIDENTIFIER,HotelId UNIQUEIDENTIFIER,FOREIGN KEY (GroupsId) REFERENCES Groups,FOREIGN KEY (HotelId) REFERENCES Hotels);

CREATE TABLE EnteredDataHotel(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),Name NVARCHAR(50),DateIn Date,Capacity INT,Price INT,Count INT,HotelId UNIQUEIDENTIFIER,Type int,FOREIGN KEY(HotelId) REFERENCES Hotels);

CREATE TABLE DifferenceDataHotel(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),Name NVARCHAR(50),DateIn Date,Capacity INT,Price INT,Count INT,HotelId UNIQUEIDENTIFIER,Type int,FOREIGN KEY(HotelId) REFERENCES Hotels);

CREATE TABLE RecordDataHotel(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),Name NVARCHAR(50),DateIn Date,Capacity INT,Price INT,Count INT,HotelId UNIQUEIDENTIFIER, Type int, FOREIGN KEY(HotelId) REFERENCES Hotels);

CREATE TABLE Records(Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),HotelId UNIQUEIDENTIFIER,GroupId UNIQUEIDENTIFIER,Price Int,Capacity INT, Count Int, DateOfCheckIn DATE,DateOfCheckOut DATE, Name NVARCHAR(50), FOREIGN KEY(HotelId) REFERENCES Hotels,FOREIGN KEY(GroupId) REFERENCES Groups);

CREATE TABLE TypesOfDays(HotelId UNIQUEIDENTIFIER, Name NVARCHAR(50) PRIMARY KEY, Type int);

select * from Users


DROP TABLE TypesOfDays; 
DROP TABLE Records; 
DROP TABLE RecordDataHotel; 
DROP TABLE DifferenceDataHotel; 
DROP TABLE EnteredDataHotel; 
DROP TABLE Settler; 
DROP TABLE Groups; 
DROP TABLE UserXHotels; 
DROP TABLE Hotels;
DROP TABLE MassEvents; 
DROP TABLE Users; 

delete Users where Id = 'E36F4229-89F3-4798-944A-08DB4B29CADF' 

select * FROM TypesOfDays

select * from MassEvents

select * from Settler

select * from Hotels

select * from Groups

select * from Records

select * from UserXHotels

select * from EnteredDataHotel
select * from DifferenceDataHotel
select * from RecordDataHotel

select * from TypesOfDays


delete  from MassEvents

delete  from EnteredDataHotel
delete  from DifferenceDataHotel
delete  from RecordDataHotel
delete  from TypesOfDays

delete  from UserXHotels


delete  from Users

delete  from Hotels

delete  from UserXHotels


delete  from Groups

delete  from Settler

delete  from MassEvents


delete Settler where GroupsId = 'F25B7073-614E-481D-019D-08DB4647DDAE' 


drop table Groups

drop table Records

drop table Hotels


drop table RecordDataHotel
drop table DifferenceDataHotel
drop table EnteredDataHotel

drop table TypesOfDays

delete Settler where FirstName = 'Set'