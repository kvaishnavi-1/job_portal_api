CREATE DATABASE JobPortal;
GO

USE JobPortal;
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) CHECK (Role IN ('Employer', 'Candidate')) NOT NULL
);
GO

CREATE TABLE Jobs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Company NVARCHAR(200) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EmployerId INT NOT NULL,
    FOREIGN KEY (EmployerId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Applications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    JobId INT NOT NULL,
    CandidateId INT NOT NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Pending', 'Accepted', 'Rejected')) DEFAULT 'Pending',
    FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE NO ACTION,
    FOREIGN KEY (CandidateId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO
