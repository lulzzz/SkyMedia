﻿using System.Collections.Generic;

using Microsoft.WindowsAzure.MediaServices.Client;

using SkyMedia.WebApp.Models;

namespace SkyMedia.ServiceBroker
{
    internal partial class MediaClient
    {
        private static string[] GetInputAssetIds(MediaAssetInput[] inputAssets)
        {
            List<string> assetIds = new List<string>();
            foreach (MediaAssetInput inputAsset in inputAssets)
            {
                assetIds.Add(inputAsset.AssetId);
            }
            return assetIds.ToArray();
        }

        private static bool HasProtectionEnabled(ContentProtection contentProtection)
        {
            return contentProtection != null && (contentProtection.AES || contentProtection.DRMPlayReady || contentProtection.DRMWidevine);
        }

        private static MediaJobTask SetJobTask(MediaClient mediaClient, MediaJobTask jobTask, string processorConfig, MediaAssetInput[] inputAssets)
        {
            jobTask.ProcessorConfig = (processorConfig == null) ? string.Empty : processorConfig;
            jobTask.InputAssetIds = GetInputAssetIds(inputAssets);
            if (string.IsNullOrEmpty(jobTask.OutputAssetName))
            {
                string assetId = jobTask.InputAssetIds[0];
                IAsset asset = mediaClient.GetEntityById(EntityType.Asset, assetId) as IAsset;
                jobTask.OutputAssetName = string.Concat(asset.Name, " (", jobTask.Name, ")");
            }
            jobTask.OutputAssetEncryption = HasProtectionEnabled(jobTask.ContentProtection) ? AssetCreationOptions.StorageEncrypted : AssetCreationOptions.None;
            return jobTask;
        }
    }
}
