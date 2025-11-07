-- Generation Plan MLM Database Script
-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GenerationPlanMLM')
BEGIN
    CREATE DATABASE GenerationPlanMLM;
END
GO

USE GenerationPlanMLM;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        MobileNumber NVARCHAR(15) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        UserId NVARCHAR(20) NOT NULL UNIQUE,
        SponsorId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsAdmin BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (SponsorId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_UserId ON Users(UserId);
    CREATE INDEX IX_Users_MobileNumber ON Users(MobileNumber);
    CREATE INDEX IX_Users_SponsorId ON Users(SponsorId);
END
GO

-- Create IncomeRecords Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IncomeRecords')
BEGIN
    CREATE TABLE IncomeRecords (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        FromUserId INT NOT NULL,
        Level INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (FromUserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_IncomeRecords_UserId ON IncomeRecords(UserId);
    CREATE INDEX IX_IncomeRecords_FromUserId ON IncomeRecords(FromUserId);
    CREATE INDEX IX_IncomeRecords_Level ON IncomeRecords(Level);
END
GO


PRINT 'Database created successfully!';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Run the application';
PRINT '2. Register a new user through the registration page';
PRINT '3. Update the first user to admin in database:';
PRINT '   UPDATE Users SET IsAdmin = 1 WHERE Id = 1;';
PRINT '';
GO

