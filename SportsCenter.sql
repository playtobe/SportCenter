/* =====================================================================
   Sports Center Management System - Database script (SQL Server)
   Group Project Assignment - PRN212
   ---------------------------------------------------------------------
   Tai khoan mau (mat khau chung: 123456, luu dang SHA-256 hex IN HOA):
     manager1   (Manager)       reception1 (Receptionist)
     coach1..3  (Coach)         member1..5 (Member)
   Trong C#: Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pw)))
   ===================================================================== */
/* --- Co the chay lai nhieu lan --------------------------------------------
   B1. Chuyen ve master TRUOC khi xoa: neu cua so query dang dung san trong
       SportsCenter thi khong the tu xoa chinh database minh dang dung (loi 3702).
   B2. Xoa database cu (ROLLBACK IMMEDIATE se ngat cac ket noi khac).
   B3. Du phong: neu B2 khong xoa duoc, van xoa tung bang roi tao lai,
       nen du lieu cu va cau truc cu (vd thieu SlotId) khong con sot lai.
   ------------------------------------------------------------------------- */
USE master;
GO
IF DB_ID(N'SportsCenter') IS NOT NULL
BEGIN
    ALTER DATABASE [SportsCenter] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [SportsCenter];
END
GO
IF DB_ID(N'SportsCenter') IS NULL
    CREATE DATABASE [SportsCenter];
GO
USE [SportsCenter];
GO
ALTER DATABASE [SportsCenter] SET MULTI_USER;
GO
SET QUOTED_IDENTIFIER ON;   -- bat buoc cho filtered index
GO

-- Xoa bang theo thu tu bang con truoc, bang cha sau (chi co tac dung khi database cu con ton tai)
IF OBJECT_ID(N'dbo.AuditLogs',          N'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID(N'dbo.Attendances',        N'U') IS NOT NULL DROP TABLE dbo.Attendances;
IF OBJECT_ID(N'dbo.Payments',           N'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID(N'dbo.Registrations',      N'U') IS NOT NULL DROP TABLE dbo.Registrations;
IF OBJECT_ID(N'dbo.Classes',            N'U') IS NOT NULL DROP TABLE dbo.Classes;
IF OBJECT_ID(N'dbo.Subscriptions',      N'U') IS NOT NULL DROP TABLE dbo.Subscriptions;
IF OBJECT_ID(N'dbo.TimeSlots',          N'U') IS NOT NULL DROP TABLE dbo.TimeSlots;
IF OBJECT_ID(N'dbo.Rooms',              N'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID(N'dbo.Sports',             N'U') IS NOT NULL DROP TABLE dbo.Sports;
IF OBJECT_ID(N'dbo.MembershipPackages', N'U') IS NOT NULL DROP TABLE dbo.MembershipPackages;
IF OBJECT_ID(N'dbo.Coaches',            N'U') IS NOT NULL DROP TABLE dbo.Coaches;
IF OBJECT_ID(N'dbo.Members',            N'U') IS NOT NULL DROP TABLE dbo.Members;
IF OBJECT_ID(N'dbo.Users',              N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ------------------------- 1. Users & people ------------------------- */
CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64)  NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(150) NULL,          -- khong bat buoc (he thong khong gui mail)
    Phone        NVARCHAR(20)  NULL,
    Role         NVARCHAR(20)  NOT NULL
                 CONSTRAINT CK_Users_Role CHECK (Role IN (N'Manager', N'Receptionist', N'Coach', N'Member')),
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO
-- Member co the dang ky tai quay (chua co tai khoan) => UserId cho phep NULL
CREATE TABLE Members (
    MemberId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NULL,
    FullName    NVARCHAR(100) NOT NULL,
    Phone       NVARCHAR(20)  NULL,
    Email       NVARCHAR(150) NULL,
    DateOfBirth DATE          NULL,
    Gender      NVARCHAR(10)  NULL,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Members_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
CREATE UNIQUE INDEX UX_Members_UserId ON Members(UserId) WHERE UserId IS NOT NULL;
GO
CREATE TABLE Coaches (
    CoachId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NULL,
    FullName  NVARCHAR(100) NOT NULL,
    Email     NVARCHAR(150) NOT NULL,
    Phone     NVARCHAR(20)  NULL,
    Specialty NVARCHAR(100) NULL,
    CONSTRAINT FK_Coaches_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
CREATE UNIQUE INDEX UX_Coaches_UserId ON Coaches(UserId) WHERE UserId IS NOT NULL;
GO

/* ----------------------- 2. Membership packages ---------------------- */
CREATE TABLE MembershipPackages (
    PackageId    INT IDENTITY(1,1) PRIMARY KEY,
    PackageName  NVARCHAR(100)  NOT NULL,
    DurationDays INT            NOT NULL CHECK (DurationDays > 0),
    Price        DECIMAL(18,2)  NOT NULL CHECK (Price >= 0),
    Description  NVARCHAR(300)  NULL,
    IsActive     BIT            NOT NULL DEFAULT 1
);
GO
CREATE TABLE Subscriptions (
    SubscriptionId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId       INT NOT NULL,
    PackageId      INT NOT NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT N'Active'
                   CONSTRAINT CK_Subscriptions_Status CHECK (Status IN (N'Active', N'Expired', N'Cancelled')),
    CONSTRAINT CK_Subscriptions_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT FK_Subscriptions_Members  FOREIGN KEY (MemberId)  REFERENCES Members(MemberId),
    CONSTRAINT FK_Subscriptions_Packages FOREIGN KEY (PackageId) REFERENCES MembershipPackages(PackageId)
);
GO

/* ----------------------- 3. Sports, rooms, classes -------------------- */
CREATE TABLE Sports (
    SportId   INT IDENTITY(1,1) PRIMARY KEY,
    SportName NVARCHAR(100) NOT NULL UNIQUE
);
GO
CREATE TABLE Rooms (
    RoomId   INT IDENTITY(1,1) PRIMARY KEY,
    RoomName NVARCHAR(100) NOT NULL UNIQUE,
    Capacity INT NOT NULL CHECK (Capacity > 0),
    IsActive BIT NOT NULL DEFAULT 1
);
GO
-- Ca hoc co dinh trong ngay (moi lop hoc dung dung 1 ca)
CREATE TABLE TimeSlots (
    SlotId    INT IDENTITY(1,1) PRIMARY KEY,
    SlotName  NVARCHAR(50) NOT NULL UNIQUE,
    StartTime TIME NOT NULL,
    EndTime   TIME NOT NULL,
    CONSTRAINT CK_TimeSlots_Time CHECK (EndTime > StartTime)
);
GO
-- Lich hoc co dinh hang tuan: DayOfWeek 1=Thu Hai ... 7=Chu Nhat
-- Kiem tra trung phong / trung HLV: cung DayOfWeek + cung SlotId + khoang ngay giao nhau (lam trong code BLL)
CREATE TABLE Classes (
    ClassId    INT IDENTITY(1,1) PRIMARY KEY,
    ClassCode  NVARCHAR(50)  NOT NULL UNIQUE,
    ClassName  NVARCHAR(150) NOT NULL,
    SportId    INT NOT NULL,
    CoachId    INT NOT NULL,
    RoomId     INT NOT NULL,
    DayOfWeek  TINYINT NOT NULL CHECK (DayOfWeek BETWEEN 1 AND 7),
    SlotId     INT NOT NULL,
    Capacity   INT  NOT NULL CHECK (Capacity > 0),
    StartDate  DATE NOT NULL,
    EndDate    DATE NOT NULL,
    Status     NVARCHAR(20) NOT NULL DEFAULT N'Open'
               CONSTRAINT CK_Classes_Status CHECK (Status IN (N'Open', N'Closed', N'Cancelled')),
    CONSTRAINT CK_Classes_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT FK_Classes_Sports  FOREIGN KEY (SportId) REFERENCES Sports(SportId),
    CONSTRAINT FK_Classes_Coaches FOREIGN KEY (CoachId) REFERENCES Coaches(CoachId),
    CONSTRAINT FK_Classes_Rooms   FOREIGN KEY (RoomId)  REFERENCES Rooms(RoomId),
    CONSTRAINT FK_Classes_TimeSlots FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId)
);
GO
CREATE TABLE Registrations (
    RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId       INT NOT NULL,
    ClassId        INT NOT NULL,
    RegisteredAt   DATETIME NOT NULL DEFAULT GETDATE(),
    CancelledAt    DATETIME NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT N'Registered'
                   CONSTRAINT CK_Registrations_Status CHECK (Status IN (N'Registered', N'Cancelled')),
    RegisteredBy   INT NULL,   -- UserId nguoi thao tac (Member tu dang ky hoac Receptionist)
    CONSTRAINT FK_Registrations_Members FOREIGN KEY (MemberId)     REFERENCES Members(MemberId),
    CONSTRAINT FK_Registrations_Classes FOREIGN KEY (ClassId)      REFERENCES Classes(ClassId),
    CONSTRAINT FK_Registrations_Users   FOREIGN KEY (RegisteredBy) REFERENCES Users(UserId)
);
-- Moi member chi co 1 dang ky "Registered" cho 1 lop (huy roi van dang ky lai duoc)
CREATE UNIQUE INDEX UX_Registrations_Active ON Registrations(MemberId, ClassId) WHERE Status = N'Registered';
GO

/* --------------------------- 4. Payments ----------------------------- */
-- Thanh toan MO PHONG: chi ghi nhan, khong noi cong thanh toan that
CREATE TABLE Payments (
    PaymentId      INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNo      NVARCHAR(30)  NOT NULL UNIQUE,
    MemberId       INT NOT NULL,
    SubscriptionId INT NULL,
    Amount         DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    PaymentMethod  NVARCHAR(20)  NOT NULL
                   CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN (N'Cash', N'BankTransfer', N'Card')),
    PaidAt         DATETIME NOT NULL DEFAULT GETDATE(),
    ReceivedBy     INT NULL,   -- UserId nhan vien ghi nhan
    Status         NVARCHAR(20) NOT NULL DEFAULT N'Paid'
                   CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Paid', N'Refunded')),
    Note           NVARCHAR(200) NULL,
    CONSTRAINT FK_Payments_Members       FOREIGN KEY (MemberId)       REFERENCES Members(MemberId),
    CONSTRAINT FK_Payments_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(SubscriptionId),
    CONSTRAINT FK_Payments_Users         FOREIGN KEY (ReceivedBy)     REFERENCES Users(UserId)
);
GO

/* ---------------- 5. Optional: attendance & audit log ----------------- */
CREATE TABLE Attendances (
    AttendanceId   INT IDENTITY(1,1) PRIMARY KEY,
    RegistrationId INT NOT NULL,
    SessionDate    DATE NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT N'Present'
                   CONSTRAINT CK_Attendances_Status CHECK (Status IN (N'Present', N'Absent')),
    CheckedInBy    INT NULL,
    Note           NVARCHAR(200) NULL,
    CONSTRAINT UQ_Attendances UNIQUE (RegistrationId, SessionDate),
    CONSTRAINT FK_Attendances_Registrations FOREIGN KEY (RegistrationId) REFERENCES Registrations(RegistrationId),
    CONSTRAINT FK_Attendances_Users         FOREIGN KEY (CheckedInBy)    REFERENCES Users(UserId)
);
GO
CREATE TABLE AuditLogs (
    LogId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId     INT NULL,
    Action     NVARCHAR(50)  NOT NULL,   -- Login, CreateClass, RegisterClass, Payment, ...
    EntityName NVARCHAR(50)  NULL,
    EntityId   INT NULL,
    Details    NVARCHAR(300) NULL,
    CreatedAt  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =============================== SEED DATA ============================ */
-- Dung varchar (khong dung N'') de hash khop UTF-8 cua C#
DECLARE @pw NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2);

INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, Role) VALUES
(N'manager1',   @pw, N'Nguyễn Văn Quân',  N'manager@center.vn',   N'0900000001', N'Manager'),
(N'reception1', @pw, N'Trần Thị Lan',     N'lan@center.vn',       N'0900000002', N'Receptionist'),
(N'coach1',     @pw, N'Lê Minh Hùng',     N'hung@center.vn',      N'0900000003', N'Coach'),
(N'coach2',     @pw, N'Phạm Thu Hà',      N'ha@center.vn',        N'0900000004', N'Coach'),
(N'coach3',     @pw, N'Hoàng Đức Tài',    N'tai@center.vn',       N'0900000005', N'Coach'),
(N'member1',    @pw, N'Vũ Anh Tuấn',      N'tuan@mail.com',       N'0910000001', N'Member'),
(N'member2',    @pw, N'Đỗ Thị Mai',       N'mai@mail.com',        N'0910000002', N'Member'),
(N'member3',    @pw, N'Bùi Quốc Bảo',     N'bao@mail.com',        N'0910000003', N'Member'),
(N'member4',    @pw, N'Ngô Thị Hoa',      N'hoa@mail.com',        N'0910000004', N'Member'),
(N'member5',    @pw, N'Đặng Văn Khoa',    N'khoa@mail.com',       N'0910000005', N'Member');

INSERT INTO Coaches (UserId, FullName, Email, Phone, Specialty) VALUES
(3, N'Lê Minh Hùng',  N'hung@center.vn', N'0900000003', N'Boxing, Gym'),
(4, N'Phạm Thu Hà',   N'ha@center.vn',   N'0900000004', N'Yoga, Zumba'),
(5, N'Hoàng Đức Tài', N'tai@center.vn',  N'0900000005', N'Bơi lội');

INSERT INTO Members (UserId, FullName, Phone, Email, DateOfBirth, Gender) VALUES
(6,  N'Vũ Anh Tuấn',   N'0910000001', N'tuan@mail.com', '2000-03-15', N'Male'),
(7,  N'Đỗ Thị Mai',    N'0910000002', N'mai@mail.com',  '1998-07-20', N'Female'),
(8,  N'Bùi Quốc Bảo',  N'0910000003', N'bao@mail.com',  '2001-11-02', N'Male'),
(9,  N'Ngô Thị Hoa',   N'0910000004', N'hoa@mail.com',  '1995-01-30', N'Female'),
(10, N'Đặng Văn Khoa', N'0910000005', N'khoa@mail.com', '2002-09-09', N'Male'),
(NULL, N'Lê Hải', N'0920000001', NULL,   '1999-05-05', N'Male');   -- dang ky tai quay, chua co tai khoan

INSERT INTO MembershipPackages (PackageName, DurationDays, Price, Description) VALUES
(N'Gói 1 tháng',  30,  500000,  N'Sử dụng trung tâm 30 ngày'),
(N'Gói 3 tháng',  90,  1350000, N'Tiết kiệm 10% so với gói tháng'),
(N'Gói 6 tháng',  180, 2400000, N'Tiết kiệm 20%'),
(N'Gói 12 tháng', 365, 4200000, N'Tiết kiệm 30%');

INSERT INTO Sports (SportName) VALUES
(N'Yoga'), (N'Boxing'), (N'Bơi lội'), (N'Zumba'), (N'Gym');

INSERT INTO Rooms (RoomName, Capacity) VALUES
(N'Studio A', 25), (N'Studio B', 20), (N'Võ đài Boxing', 15), (N'Hồ bơi', 30);

INSERT INTO TimeSlots (SlotName, StartTime, EndTime) VALUES
(N'Ca 1', '06:00', '07:30'),
(N'Ca 2', '07:30', '09:00'),
(N'Ca 3', '09:00', '10:30'),
(N'Ca 4', '16:30', '18:00'),
(N'Ca 5', '18:00', '19:30'),
(N'Ca 6', '19:30', '21:00');

-- DayOfWeek: 1=Mon ... 7=Sun. Khong co lop nao trung phong/HLV cung thu + cung ca.
INSERT INTO Classes (ClassCode, ClassName, SportId, CoachId, RoomId, DayOfWeek, SlotId, Capacity, StartDate, EndDate) VALUES
(N'YOGA01',  N'Yoga cơ bản sáng',   1, 2, 1, 1, 2, 20, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'YOGA02',  N'Yoga nâng cao tối',  1, 2, 1, 3, 5, 15, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'BOX01',   N'Boxing căn bản',     2, 1, 3, 2, 5, 12, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'BOX02',   N'Boxing đối kháng',   2, 1, 3, 4, 5, 10, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'SWIM01',  N'Bơi lội cho người mới', 3, 3, 4, 2, 1, 25, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'ZUMBA01', N'Zumba năng động',    4, 2, 2, 5, 6, 20, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE))),
(N'GYM01',   N'Gym tăng cơ cơ bản', 5, 1, 2, 6, 3, 3, DATEADD(DAY,-14,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE)));
-- GYM01 chi 3 cho de demo "lop day" (xem Registrations ben duoi)

-- Subscriptions: member6 (khach vang lai) chua mua goi; member 4 het han de demo
INSERT INTO Subscriptions (MemberId, PackageId, StartDate, EndDate, Status) VALUES
(1, 2, DATEADD(DAY,-30,CAST(GETDATE() AS DATE)), DATEADD(DAY,60,CAST(GETDATE() AS DATE)), N'Active'),
(2, 1, DATEADD(DAY,-10,CAST(GETDATE() AS DATE)), DATEADD(DAY,20,CAST(GETDATE() AS DATE)), N'Active'),
(3, 3, DATEADD(DAY,-60,CAST(GETDATE() AS DATE)), DATEADD(DAY,120,CAST(GETDATE() AS DATE)), N'Active'),
(4, 1, DATEADD(DAY,-50,CAST(GETDATE() AS DATE)), DATEADD(DAY,-20,CAST(GETDATE() AS DATE)), N'Expired'),
(5, 4, DATEADD(DAY,-5,CAST(GETDATE() AS DATE)),  DATEADD(DAY,360,CAST(GETDATE() AS DATE)), N'Active');

-- Payments: rai deu trong vai thang gan day de demo bao cao doanh thu
INSERT INTO Payments (InvoiceNo, MemberId, SubscriptionId, Amount, PaymentMethod, PaidAt, ReceivedBy, Note) VALUES
(N'INV-0001', 1, 1, 1350000, N'BankTransfer', DATEADD(DAY,-30,GETDATE()), 2, N'Mua gói 3 tháng'),
(N'INV-0002', 2, 2, 500000,  N'Cash',         DATEADD(DAY,-10,GETDATE()), 2, N'Mua gói 1 tháng'),
(N'INV-0003', 3, 3, 2400000, N'Card',         DATEADD(DAY,-60,GETDATE()), 2, N'Mua gói 6 tháng'),
(N'INV-0004', 4, 4, 500000,  N'Cash',         DATEADD(DAY,-50,GETDATE()), 2, N'Mua gói 1 tháng'),
(N'INV-0005', 5, 5, 4200000, N'BankTransfer', DATEADD(DAY,-5,GETDATE()),  2, N'Mua gói 12 tháng');

-- Registrations: GYM01 (Capacity 3) da day (3 nguoi) de demo chan dang ky
INSERT INTO Registrations (MemberId, ClassId, RegisteredBy) VALUES
(1, 1, 2), (1, 3, 6),
(2, 1, 7), (2, 6, 7),
(3, 3, 2), (3, 7, 8),
(5, 5, 10),
(1, 7, 6), (2, 7, 2);

-- Mot dang ky da huy de demo trang thai Cancelled
INSERT INTO Registrations (MemberId, ClassId, Status, CancelledAt, RegisteredBy) VALUES
(3, 2, N'Cancelled', DATEADD(DAY,-3,GETDATE()), 8);

-- Attendances: diem danh mau. Ngay diem danh PHAI trung thu cua lop thi moi hien dung o trong Lich cua toi.
-- @mon = thu Hai cua tuan nay (1900-01-01 la thu Hai, nen so ngay tu moc do chia 7 lay du = so ngay da qua ke tu thu Hai)
-- Registration 1 = member1/YOGA01 (thu Hai), 2 = member1/BOX01 (thu Ba), 3 = member2/YOGA01 (thu Hai)
-- CheckedInBy: 4 = coach2 (HLV YOGA01), 3 = coach1 (HLV BOX01)
DECLARE @mon DATE = DATEADD(DAY, -(DATEDIFF(DAY, '19000101', CAST(GETDATE() AS DATE)) % 7), CAST(GETDATE() AS DATE));
INSERT INTO Attendances (RegistrationId, SessionDate, Status, CheckedInBy) VALUES
(1, @mon,                  N'Present', 4),   -- tuan nay, thu Hai
(1, DATEADD(DAY,-7,@mon),  N'Absent',  4),   -- tuan truoc, thu Hai
(1, DATEADD(DAY,-14,@mon), N'Present', 4),   -- 2 tuan truoc, thu Hai
(2, DATEADD(DAY,-6,@mon),  N'Present', 3),   -- tuan truoc, thu Ba (BOX01)
(3, @mon,                  N'Present', 4);   -- member2, tuan nay, thu Hai

INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Details) VALUES
(1, N'CreateClass',   N'Classes',       1, N'Tạo lớp YOGA01'),
(2, N'RegisterClass', N'Registrations', 1, N'Đăng ký member1 vào YOGA01'),
(2, N'Payment',       N'Payments',      1, N'Thu INV-0001 1.350.000 ₫');
GO

/* ============================ Kiem tra nhanh ========================== */
SELECT N'Users' AS TableName, COUNT(*) AS Total FROM Users
UNION ALL SELECT N'Members', COUNT(*) FROM Members
UNION ALL SELECT N'Coaches', COUNT(*) FROM Coaches
UNION ALL SELECT N'MembershipPackages', COUNT(*) FROM MembershipPackages
UNION ALL SELECT N'Subscriptions', COUNT(*) FROM Subscriptions
UNION ALL SELECT N'Sports', COUNT(*) FROM Sports
UNION ALL SELECT N'Rooms', COUNT(*) FROM Rooms
UNION ALL SELECT N'Classes', COUNT(*) FROM Classes
UNION ALL SELECT N'Registrations', COUNT(*) FROM Registrations
UNION ALL SELECT N'Payments', COUNT(*) FROM Payments
UNION ALL SELECT N'Attendances', COUNT(*) FROM Attendances
UNION ALL SELECT N'AuditLogs', COUNT(*) FROM AuditLogs;
GO
