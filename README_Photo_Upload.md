# WatchZone Photo Upload Implementation

This document describes the implementation of multiple photo upload functionality for listings in the WatchZone application.

## Overview

The photo upload system allows users to:
- Upload multiple photos per listing
- Set a primary photo for each listing
- Delete individual photos
- View photos in a gallery format
- Manage photos through a user-friendly interface

## Architecture

### Domain Layer
- **`ListingPhoto`** model: Represents individual photos with metadata
- **`Listing`** model: Extended with a `Photos` collection property
- **`DatabaseContext`**: Added photo-related database operations

### Business Logic Layer
- **`IListingService`**: Extended interface with photo management methods
- **`ListingServiceBL`**: Implementation of photo operations with error handling

### Web Layer
- **`PhotoController`**: Dedicated controller for photo upload/management
- **`CreateListingViewModel`**: Extended to support file uploads
- **Views**: Updated to display photo galleries and upload interfaces

## Database Schema

### ListingPhotos Table
```sql
CREATE TABLE ListingPhotos (
    PhotoId INT IDENTITY(1,1) PRIMARY KEY,
    ListingId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DisplayOrder INT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_ListingPhotos_Listings 
        FOREIGN KEY (ListingId) REFERENCES Listings(Listings_Id) 
        ON DELETE CASCADE
);
```

## Setup Instructions

### 1. Database Setup
Run the `Database_Setup.sql` script to create the `ListingPhotos` table:
```sql
sqlcmd -S localhost -d WatchZone -i Database_Setup.sql
```

### 2. File Storage
Ensure the `~/Images/` directory exists and has write permissions:
- The application stores uploaded photos in `WatchZone.Web/Images/`
- Photos are accessible via `/Images/{filename}` URL path

### 3. Dependencies
The implementation uses existing project dependencies:
- System.Web for file uploads
- System.Data.SqlClient for database operations
- Bootstrap and Font Awesome for UI components

## Features

### Photo Upload
- **File Types**: JPG, JPEG, PNG, GIF
- **File Size**: Maximum 5MB per file
- **Multiple Upload**: Users can select and upload multiple files at once
- **Progress Tracking**: Visual progress bar during upload
- **Validation**: Client and server-side validation

### Photo Management
- **Primary Photo**: First uploaded photo becomes primary automatically
- **Set Primary**: Users can change which photo is primary
- **Delete Photos**: Individual photo deletion with confirmation
- **Display Order**: Photos are ordered by upload sequence

### Gallery Display
- **Main Photo**: Large display of primary or first photo
- **Thumbnails**: Grid of thumbnail images for navigation
- **Click to View**: Click thumbnails to change main photo
- **Photo Count Badge**: Shows number of photos on listing cards

## API Endpoints

### PhotoController Actions
- `POST /Photo/UploadPhoto` - Upload a new photo
- `POST /Photo/DeletePhoto` - Delete a specific photo
- `POST /Photo/SetPrimaryPhoto` - Set a photo as primary

### Request/Response Format
```javascript
// Upload Response
{
    "success": true,
    "photoId": 123,
    "fileName": "guid.jpg",
    "filePath": "/Images/guid.jpg",
    "isPrimary": false
}

// Error Response
{
    "success": false,
    "message": "Error description"
}
```

## Security Features

### Authentication & Authorization
- Only authenticated users can upload photos
- Users can only manage photos for their own listings
- Admin users have full access to all photos

### File Validation
- File type validation (images only)
- File size limits (5MB maximum)
- Unique filename generation to prevent conflicts
- Path traversal protection

### Error Handling
- Comprehensive error logging
- Graceful failure handling
- User-friendly error messages
- Automatic cleanup on failed operations

## User Interface

### Listing Details Page
- Photo gallery with main image and thumbnails
- Upload section for listing owners
- Photo management controls (set primary, delete)
- Progress indicators for uploads

### Listing Index Page
- Primary photo display on listing cards
- Photo count badges
- Fallback to ImageUrl for backward compatibility
- Placeholder for listings without photos

### Create Listing Page
- Information about photo upload capability
- Guidance to upload photos after creation
- Backward compatibility with ImageUrl field

## Backward Compatibility

The implementation maintains full backward compatibility:
- Existing `ImageUrl` field is preserved
- Listings without photos fall back to `ImageUrl`
- Existing functionality remains unchanged
- Gradual migration path for existing data

## Performance Considerations

### Database Optimization
- Indexed foreign key relationships
- Efficient queries with proper WHERE clauses
- Cascade delete for data integrity

### File Storage
- Unique GUID-based filenames prevent conflicts
- Organized storage in dedicated Images directory
- Efficient file serving through IIS

### Frontend Performance
- Asynchronous photo uploads
- Progress feedback for user experience
- Lazy loading of photo collections
- Optimized image display with CSS object-fit

## Error Scenarios & Handling

### Upload Failures
- Network interruptions: Retry mechanism
- Server errors: Cleanup uploaded files
- Validation failures: Clear user feedback
- Storage issues: Graceful degradation

### Data Consistency
- Transaction-based operations where needed
- Foreign key constraints ensure referential integrity
- Cascade deletes maintain data consistency
- Error logging for debugging

## Future Enhancements

### Potential Improvements
- Image resizing and optimization
- Cloud storage integration (Azure Blob, AWS S3)
- Drag-and-drop upload interface
- Bulk photo operations
- Photo metadata extraction
- Advanced gallery features (zoom, slideshow)

### Scalability Considerations
- CDN integration for photo delivery
- Background processing for image operations
- Caching strategies for frequently accessed photos
- Database partitioning for large photo collections

## Testing

### Manual Testing Checklist
- [ ] Upload single photo
- [ ] Upload multiple photos
- [ ] Set primary photo
- [ ] Delete photos
- [ ] View photo gallery
- [ ] Test file validation
- [ ] Test permission checks
- [ ] Test error scenarios

### Test Data
Use the commented sample data in `Database_Setup.sql` to create test photos for existing listings.

## Support

For issues or questions regarding the photo upload functionality:
1. Check error logs in the application
2. Verify database table creation
3. Ensure file system permissions
4. Review network connectivity for uploads

This implementation provides a robust, scalable foundation for photo management in the WatchZone application while maintaining backward compatibility and following established architectural patterns. 