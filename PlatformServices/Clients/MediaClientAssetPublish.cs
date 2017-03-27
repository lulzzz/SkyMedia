﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.MediaServices.Client;

using Newtonsoft.Json.Linq;

namespace AzureSkyMedia.PlatformServices
{
    public partial class MediaClient
    {
        private static IAsset[] GetTaskOutputs(IJob job, string[] processorIds)
        {
            List<IAsset> taskOutputs = new List<IAsset>();
            foreach (ITask jobTask in job.Tasks)
            {
                if (processorIds.Contains(jobTask.MediaProcessorId))
                {
                    taskOutputs.AddRange(jobTask.OutputAssets);
                }
            }
            return taskOutputs.ToArray();
        }

        private static ITask[] GetJobTasks(IJob job, string[] processorIds)
        {
            List<ITask> jobTasks = new List<ITask>();
            foreach (ITask jobTask in job.Tasks)
            {
                if (processorIds.Contains(jobTask.MediaProcessorId))
                {
                    jobTasks.Add(jobTask);
                }
            }
            return jobTasks.ToArray();
        }

        private static string GetLanguageCode(ITask indexerTask)
        {
            JObject processorConfig = JObject.Parse(indexerTask.Configuration);
            JToken processorOptions = processorConfig["Features"][0]["Options"];
            string spokenLanguage = processorOptions["Language"].ToString();
            return spokenLanguage.Substring(0, 2).ToLower();
        }

        private static BlobClient GetBlobClient(JobPublish jobPublish)
        {
            string[] accountCredentials = new string[] { jobPublish.StorageAccountName, jobPublish.StorageAccountKey };
            return new BlobClient(accountCredentials);
        }

        private static void PublishIndex(IJob job, IAsset asset, JobPublish jobPublish)
        {
            string processorId = Constant.Media.ProcessorId.Indexer;
            string[] processorIds = new string[] { processorId };
            ITask[] jobTasks = GetJobTasks(job, processorIds);
            if (jobTasks.Length > 0)
            {
                BlobClient blobClient = GetBlobClient(jobPublish);
                foreach (ITask jobTask in jobTasks)
                {
                    IAsset outputAsset = jobTask.OutputAssets[0];
                    string fileExtension = Constant.Media.FileExtension.WebVtt;
                    string[] fileNames = GetFileNames(outputAsset, fileExtension);
                    foreach (string fileName in fileNames)
                    {
                        string languageCode = GetLanguageCode(jobTask);
                        string languageExtension = string.Concat(Constant.TextDelimiter.Identifier, languageCode, fileExtension);
                        string languageFileName = fileName.Replace(fileExtension, languageExtension);
                        blobClient.CopyFile(outputAsset, asset, fileName, languageFileName, false);
                    }
                }
            }
        }

        private static void PublishAnalytics(IJob job, IAsset asset, JobPublish jobPublish)
        {
            string processorId1 = Constant.Media.ProcessorId.FaceDetection;
            string processorId2 = Constant.Media.ProcessorId.FaceRedaction;
            string processorId3 = Constant.Media.ProcessorId.VideoAnnotation;
            string processorId4 = Constant.Media.ProcessorId.CharacterRecognition;
            string processorId5 = Constant.Media.ProcessorId.ContentModeration;
            string processorId6 = Constant.Media.ProcessorId.MotionDetection;
            string[] processorIds = new string[] { processorId1, processorId2, processorId3, processorId4, processorId5, processorId6 };
            ITask[] jobTasks = GetJobTasks(job, processorIds);
            if (jobTasks.Length > 0)
            {
                BlobClient blobClient = GetBlobClient(jobPublish);
                using (DatabaseClient databaseClient = new DatabaseClient(true))
                {
                    string collectionId = Constant.Database.DocumentCollection.Metadata;
                    foreach (ITask jobTask in jobTasks)
                    {
                        IAsset outputAsset = jobTask.OutputAssets[0];
                        string fileExtension = Constant.Media.FileExtension.Json;
                        string[] fileNames = GetFileNames(outputAsset, fileExtension);
                        if (fileNames.Length > 0 && asset != null)
                        {
                            string sourceContainerName = outputAsset.Uri.Segments[1];
                            string sourceFileName = fileNames[0];
                            CloudBlockBlob sourceBlob = blobClient.GetBlob(sourceContainerName, string.Empty, sourceFileName, false);
                            string jsonData = string.Empty;
                            using (System.IO.Stream sourceStream = sourceBlob.OpenRead())
                            {
                                StreamReader streamReader = new StreamReader(sourceStream);
                                jsonData = streamReader.ReadToEnd();
                            }
                            if (!string.IsNullOrEmpty(jsonData))
                            {
                                string assetAccount = jobPublish.PartitionKey;
                                string documentId = databaseClient.CreateDocument(assetAccount, asset.Id, collectionId, jsonData);
                                MediaProcessor mediaProcessor = Processor.GetProcessorType(jobTask.MediaProcessorId);
                                string destinationFileName = string.Concat(documentId, Constant.TextDelimiter.Identifier, mediaProcessor.ToString(), fileExtension);
                                blobClient.CopyFile(outputAsset, asset, sourceFileName, destinationFileName, false);
                            }
                        }
                        if (jobTask.Configuration.Contains("mode") && jobTask.Configuration.Contains("analyze"))
                        {
                            IAsset inputAsset = jobTask.InputAssets[0];
                            string primaryFileName = GetPrimaryFile(inputAsset);
                            blobClient.CopyFile(inputAsset, outputAsset, primaryFileName, primaryFileName, true);
                        }
                    }
                }
            }
        }

        private static void PublishContent(MediaClient mediaClient, string accountName, IJob job, JobPublish jobPublish, ContentProtection contentProtection)
        {
            string processorId1 = Constant.Media.ProcessorId.EncoderStandard;
            string processorId2 = Constant.Media.ProcessorId.EncoderPremium;
            string processorId3 = Constant.Media.ProcessorId.EncoderUltra;
            string[] processorIds = new string[] { processorId1, processorId2, processorId3 };
            ITask[] jobTasks = GetJobTasks(job, processorIds);
            foreach (ITask jobTask in jobTasks)
            {
                foreach (IAsset outputAsset in jobTask.OutputAssets)
                {
                    PublishStream(mediaClient, outputAsset, contentProtection);
                    PublishIndex(job, outputAsset, jobPublish);
                    PublishAnalytics(job, outputAsset, jobPublish);
                }
            }
        }

        internal static void PublishStream(MediaClient mediaClient, IAsset asset, ContentProtection contentProtection)
        {
            if (asset.IsStreamable || asset.AssetType == AssetType.MP4)
            {
                if (asset.Options == AssetCreationOptions.StorageEncrypted && asset.DeliveryPolicies.Count == 0)
                {
                    mediaClient.AddDeliveryPolicies(asset, contentProtection);
                }
                if (asset.Locators.Count == 0)
                {
                    LocatorType locatorType = LocatorType.OnDemandOrigin;
                    mediaClient.CreateLocator(null, locatorType, asset, null);
                }
            }
        }

        internal static string[] GetFileNames(IAsset asset, string fileExtension)
        {
            List<string> fileNames = new List<string>();
            foreach (IAssetFile assetFile in asset.AssetFiles)
            {
                if (assetFile.Name.EndsWith(fileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    fileNames.Add(assetFile.Name);
                }
            }
            return fileNames.ToArray();
        }

        public static string PublishJob(MediaJobNotification jobNotification, bool webHook)
        {
            string jobPublication = string.Empty;
            if (jobNotification != null && jobNotification.EventType == MediaJobNotificationEvent.JobStateChange &&
                (jobNotification.Properties.NewState == JobState.Error ||
                 jobNotification.Properties.NewState == JobState.Canceled ||
                 jobNotification.Properties.NewState == JobState.Finished))
            {
                if (webHook)
                {
                    string settingKey = Constant.AppSettingKey.MediaJobNotificationStorageQueueName;
                    string queueName = AppSetting.GetValue(settingKey);
                    MessageClient messageClient = new MessageClient();
                    messageClient.AddMessage(queueName, jobNotification);
                }
                else
                {
                    EntityClient entityClient = new EntityClient();
                    string tableName = Constant.Storage.TableName.JobPublish;
                    string partitionKey = jobNotification.Properties.AccountName;
                    string rowKey = jobNotification.Properties.JobId;
                    JobPublish jobPublish = entityClient.GetEntity<JobPublish>(tableName, partitionKey, rowKey);
                    if (jobPublish != null)
                    {
                        tableName = Constant.Storage.TableName.JobPublishProtection;
                        ContentProtection contentProtection = entityClient.GetEntity<ContentProtection>(tableName, partitionKey, rowKey);
                        string accountName = jobPublish.PartitionKey;
                        string accountKey = jobPublish.MediaAccountKey;
                        MediaClient mediaClient = new MediaClient(accountName, accountKey);
                        IJob job = mediaClient.GetEntityById(MediaEntity.Job, rowKey) as IJob;
                        if (job != null)
                        {
                            mediaClient.SetProcessorUnits(job, ReservedUnitType.Basic);
                            if (jobNotification.Properties.NewState == JobState.Finished)
                            {
                                PublishContent(mediaClient, accountName, job, jobPublish, contentProtection);
                            }
                            if (!string.IsNullOrEmpty(jobPublish.MobileNumber))
                            {
                                string messageText = GetNotificationMessage(accountName, job);
                                MessageClient.SendText(messageText, jobPublish.MobileNumber);
                            }
                        }
                        if (contentProtection != null)
                        {
                            tableName = Constant.Storage.TableName.JobPublishProtection;
                            entityClient.DeleteEntity(tableName, contentProtection);
                        }
                        tableName = Constant.Storage.TableName.JobPublish;
                        entityClient.DeleteEntity(tableName, jobPublish);
                    }
                }
            }
            return jobPublication;
        }
    }
}
