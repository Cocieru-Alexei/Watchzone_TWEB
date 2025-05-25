using System;

namespace WatchZone.Domain.Model
{
    public class ListingPhoto
    {
        public int PhotoId { get; set; }
        public int ListingId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
        public int DisplayOrder { get; set; }
    }
} 