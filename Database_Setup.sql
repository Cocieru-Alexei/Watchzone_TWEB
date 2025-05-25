-- Database setup script for WatchZone photo functionality
-- Run this script to create the ListingPhotos table

USE WatchZone;
GO

-- Create ListingPhotos table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ListingPhotos' AND xtype='U')
BEGIN
    CREATE TABLE ListingPhotos (
        PhotoId INT IDENTITY(1,1) PRIMARY KEY,
        ListingId INT NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        IsPrimary BIT NOT NULL DEFAULT 0,
        UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        DisplayOrder INT NOT NULL DEFAULT 1,
        
        -- Foreign key constraint
        CONSTRAINT FK_ListingPhotos_Listings 
            FOREIGN KEY (ListingId) REFERENCES Listings(Listings_Id) 
            ON DELETE CASCADE
    );
    
    -- Create index for better performance
    CREATE INDEX IX_ListingPhotos_ListingId ON ListingPhotos(ListingId);
    CREATE INDEX IX_ListingPhotos_IsPrimary ON ListingPhotos(ListingId, IsPrimary);
    
    PRINT 'ListingPhotos table created successfully.';
END
ELSE
BEGIN
    PRINT 'ListingPhotos table already exists.';
END
GO

-- Optional: Add some sample data if you want to test
-- Note: Make sure you have existing listings before running this
/*
INSERT INTO ListingPhotos (ListingId, FileName, FilePath, IsPrimary, DisplayOrder)
VALUES 
    (1, 'sample1.jpg', '/Images/sample1.jpg', 1, 1),
    (1, 'sample2.jpg', '/Images/sample2.jpg', 0, 2),
    (2, 'sample3.jpg', '/Images/sample3.jpg', 1, 1);
*/ 