-- Smart Expense Tracker Database Setup Script for SQL Server / LocalDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SmartExpenseTrackerDB')
BEGIN
    CREATE DATABASE SmartExpenseTrackerDB;
END
GO

USE SmartExpenseTrackerDB;
GO

-- Drop tables if needed for clean re-creation (in proper dependency order)
IF OBJECT_ID('dbo.Expenses', 'U') IS NOT NULL DROP TABLE dbo.Expenses;
IF OBJECT_ID('dbo.Budgets', 'U') IS NOT NULL DROP TABLE dbo.Budgets;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- Users Table with Role-Based Access Control (Admin / Client)
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(120) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    MonthlyIncome DECIMAL(18,2) DEFAULT 0.00,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Client', -- 'Client' or 'Admin'
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Categories Table
CREATE TABLE dbo.Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL,
    CategoryType NVARCHAR(20) NOT NULL DEFAULT 'Expense', -- 'Expense' or 'Income'
    Color NVARCHAR(20) DEFAULT '#0d6efd',
    Icon NVARCHAR(50) DEFAULT 'bi-tag'
);
GO

-- Budgets Table (Category monthly budget limits)
CREATE TABLE dbo.Budgets (
    BudgetId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES dbo.Categories(CategoryId) ON DELETE CASCADE,
    BudgetLimit DECIMAL(18,2) NOT NULL,
    BudgetMonth INT NOT NULL, -- 1-12
    BudgetYear INT NOT NULL   -- e.g. 2026
);
GO

-- Expenses / Transactions Table
CREATE TABLE dbo.Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES dbo.Categories(CategoryId),
    Title NVARCHAR(150) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ExpenseDate DATE NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'Cash', -- Cash, Credit Card, Debit Card, UPI, Net Banking
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Seed Default Categories
INSERT INTO dbo.Categories (CategoryName, CategoryType, Color, Icon) VALUES
('Food & Dining', 'Expense', '#ff6b6b', 'bi-cup-hot'),
('Groceries', 'Expense', '#4ecdc4', 'bi-cart'),
('Transportation', 'Expense', '#45b7d1', 'bi-car-front'),
('Housing & Rent', 'Expense', '#96ceb4', 'bi-house-door'),
('Utilities & Bills', 'Expense', '#ffeead', 'bi-lightning-charge'),
('Entertainment', 'Expense', '#d4a5a5', 'bi-film'),
('Shopping', 'Expense', '#9b5de5', 'bi-bag-check'),
('Healthcare', 'Expense', '#f15bb5', 'bi-heart-pulse'),
('Education', 'Expense', '#00bbf9', 'bi-mortarboard'),
('Salary & Income', 'Income', '#00f5d4', 'bi-cash-coin'),
('Investment Returns', 'Income', '#2ec4b6', 'bi-graph-up-arrow'),
('Other', 'Expense', '#6c757d', 'bi-three-dots');
GO

-- Seed Demo Users (Client & Admin)
INSERT INTO dbo.Users (FullName, Email, Password, MonthlyIncome, Role) VALUES
('John Doe', 'john@example.com', 'demo123', 50000.00, 'Client'),
('System Administrator', 'admin@smarttracker.com', 'admin123', 100000.00, 'Admin');
GO

-- Seed Demo Budgets for user 1 (Current month/year)
INSERT INTO dbo.Budgets (UserId, CategoryId, BudgetLimit, BudgetMonth, BudgetYear)
VALUES 
(1, 1, 8000.00, MONTH(GETDATE()), YEAR(GETDATE())),
(1, 2, 7000.00, MONTH(GETDATE()), YEAR(GETDATE())),
(1, 3, 4000.00, MONTH(GETDATE()), YEAR(GETDATE())),
(1, 5, 3000.00, MONTH(GETDATE()), YEAR(GETDATE())),
(1, 6, 2500.00, MONTH(GETDATE()), YEAR(GETDATE())),
(1, 7, 5000.00, MONTH(GETDATE()), YEAR(GETDATE()));
GO

-- Seed Demo Expenses for user 1
INSERT INTO dbo.Expenses (UserId, CategoryId, Title, Amount, ExpenseDate, PaymentMethod, Notes) VALUES
(1, 1, 'Weekend Dinner with friends', 1450.00, DATEADD(DAY, -2, GETDATE()), 'Credit Card', 'Italian restaurant downtown'),
(1, 2, 'Monthly Supermarket Haul', 4320.50, DATEADD(DAY, -5, GETDATE()), 'Debit Card', 'Vegetables, milk, pulses and snacks'),
(1, 3, 'Fuel / Petrol Refill', 2100.00, DATEADD(DAY, -4, GETDATE()), 'UPI', 'Full tank petrol'),
(1, 5, 'High-speed Fiber Internet Bill', 999.00, DATEADD(DAY, -10, GETDATE()), 'Net Banking', 'Quarterly rental'),
(1, 6, 'IMAX Movie Tickets', 750.00, DATEADD(DAY, -1, GETDATE()), 'Credit Card', '2 Tickets + Popcorn'),
(1, 7, 'Winter Jacket & Shoes', 3200.00, DATEADD(DAY, -8, GETDATE()), 'Credit Card', 'Sale shopping'),
(1, 1, 'Office Cafeteria Lunches', 680.00, DATEADD(DAY, -3, GETDATE()), 'UPI', 'Weekly lunch expenses'),
(1, 8, 'Routine Dental Checkup', 1200.00, DATEADD(DAY, -12, GETDATE()), 'Cash', 'Consultation & medicine');
GO
