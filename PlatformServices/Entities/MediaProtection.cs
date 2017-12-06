﻿namespace AzureSkyMedia.PlatformServices
{
    public class ContentProtection : StorageEntity
    {
        public string DirectoryId { get; set; }

        public string Audience { get; set; }

        public bool Aes { get; set; }

        public bool DrmPlayReady { get; set; }

        public bool DrmWidevine { get; set; }

        public bool DrmFairPlay { get; set; }

        public bool ContentAuthTypeToken { get; set; }

        public bool ContentAuthTypeAddress { get; set; }

        public string ContentAuthAddressRange { get; set; }
    }
}