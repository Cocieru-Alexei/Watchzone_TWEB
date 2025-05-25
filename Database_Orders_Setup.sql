-- Database setup script for WatchZone order functionality
-- Run this script to create the Orders and OrderItems tables

USE WatchZone;
GO

-- Create Orders table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
BEGIN
    CREATE TABLE Orders (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        TotalAmount DECIMAL(10,2) NOT NULL,
        OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Confirmed, Shipped, Delivered, Cancelled
        ShippingAddress NVARCHAR(500) NOT NULL,
        BillingAddress NVARCHAR(500) NOT NULL,
        PaymentMethod NVARCHAR(100) NOT NULL,
        PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, Failed, Refunded
        ShippedDate DATETIME2 NULL,
        DeliveredDate DATETIME2 NULL,
        TrackingNumber NVARCHAR(100) NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        -- Foreign key constraint (assuming UDbTable exists)
        CONSTRAINT FK_Orders_UDbTable 
            FOREIGN KEY (UserId) REFERENCES UDbTable(Id) 
            ON DELETE CASCADE
    );
    
    -- Create indexes for better performance
    CREATE INDEX IX_Orders_UserId ON Orders(UserId);
    CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
    CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
    CREATE INDEX IX_Orders_PaymentStatus ON Orders(PaymentStatus);
    
    PRINT 'Orders table created successfully.';
END
ELSE
BEGIN
    PRINT 'Orders table already exists.';
END
GO

-- Create OrderItems table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
BEGIN
    CREATE TABLE OrderItems (
        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ListingId INT NOT NULL,
        ProductName NVARCHAR(255) NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        TotalPrice DECIMAL(10,2) NOT NULL,
        ProductImageUrl NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create indexes for better performance
    CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ListingId ON OrderItems(ListingId);
    
    PRINT 'OrderItems table created successfully.';
END
ELSE
BEGIN
    PRINT 'OrderItems table already exists.';
END
GO

-- Add foreign key constraints after both tables are created
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrderItems_Orders')
BEGIN
    ALTER TABLE OrderItems
    ADD CONSTRAINT FK_OrderItems_Orders 
        FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) 
        ON DELETE CASCADE;
    PRINT 'Foreign key constraint FK_OrderItems_Orders added successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_OrderItems_Orders already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrderItems_Listings')
BEGIN
    ALTER TABLE OrderItems
    ADD CONSTRAINT FK_OrderItems_Listings 
        FOREIGN KEY (ListingId) REFERENCES Listings(Listings_Id) 
        ON DELETE NO ACTION; -- Don't delete order items if listing is deleted
    PRINT 'Foreign key constraint FK_OrderItems_Listings added successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_OrderItems_Listings already exists.';
END
GO

-- Create a trigger to update the UpdatedAt column in Orders table
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Orders_UpdatedAt')
BEGIN
    EXEC('
    CREATE TRIGGER TR_Orders_UpdatedAt
    ON Orders
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE Orders 
        SET UpdatedAt = GETUTCDATE()
        FROM Orders o
        INNER JOIN inserted i ON o.OrderId = i.OrderId;
    END
    ');
    PRINT 'Orders UpdatedAt trigger created successfully.';
END
ELSE
BEGIN
    PRINT 'Orders UpdatedAt trigger already exists.';
END
GO

-- Optional: Add some sample data for testing
-- Note: Make sure you have existing users and listings before running this
/*
-- Sample order data (uncomment to use)
INSERT INTO Orders (UserId, TotalAmount, OrderStatus, ShippingAddress, BillingAddress, PaymentMethod, PaymentStatus, Notes)
VALUES 
    (1, 1299.99, 'Pending', '123 Main St, City, State 12345', '123 Main St, City, State 12345', 'Credit Card', 'Pending', 'Please handle with care'),
    (1, 899.50, 'Shipped', '456 Oak Ave, City, State 67890', '456 Oak Ave, City, State 67890', 'PayPal', 'Paid', NULL);

-- Sample order items data (uncomment to use)
INSERT INTO OrderItems (OrderId, ListingId, ProductName, UnitPrice, Quantity, TotalPrice, ProductImageUrl)
VALUES 
    (1, 1, 'Rolex Submariner', 1299.99, 1, 1299.99, '/Images/rolex1.jpg'),
    (2, 2, 'Omega Speedmaster', 899.50, 1, 899.50, '/Images/omega1.jpg');
*/

PRINT 'Order functionality database setup completed successfully.';
GO 