﻿using System.Xml;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SkyMedia.WebApp.Models;

namespace SkyMedia.ServiceBroker
{
    internal partial class MediaClient
    {
        private static MediaJobTask[] GetIndexerV1Tasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.IndexerV1;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorIndexerV1Id;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorIndexerV1DocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            XmlDocument processorConfigXml = new XmlDocument();
            processorConfigXml.LoadXml(processorConfig["Xml"].ToString());
            XmlNodeList configSettings = processorConfigXml.SelectNodes(Constants.Media.ProcessorConfig.IndexerV1XPath);
            List<string> captionFormats = new List<string>();
            if (jobTask.IndexerCaptionWebVtt)
            {
                captionFormats.Add("WebVTT");
            }
            if (jobTask.IndexerCaptionTtml)
            {
                captionFormats.Add("TTML");
            }
            if (captionFormats.Count > 0)
            {
                configSettings[1].Attributes[1].Value = string.Join(";", captionFormats);
            }
            if (jobTask.IndexerSpokenLanguages == null)
            {
                jobTask = SetJobTask(mediaClient, jobTask, processorConfigXml.OuterXml, inputAssets);
                jobTasks.Add(jobTask);
            }
            else
            {
                foreach (string spokenLanguage in jobTask.IndexerSpokenLanguages)
                {
                    configSettings[0].Attributes[1].Value = spokenLanguage;
                    jobTask = SetJobTask(mediaClient, jobTask, processorConfigXml.OuterXml, inputAssets);
                    jobTasks.Add(jobTask);
                }
            }
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetIndexerV2Tasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.IndexerV2;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorIndexerV2Id;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorIndexerV2DocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            JToken processorOptions = processorConfig["Features"][0]["Options"];
            JArray captionFormats = new JArray();
            if (jobTask.IndexerCaptionWebVtt)
            {
                captionFormats.Add("WebVTT");
            }
            if (jobTask.IndexerCaptionTtml)
            {
                captionFormats.Add("TTML");
            }
            if (captionFormats.Count > 0)
            {
                processorOptions["Formats"] = captionFormats;
            }
            if (jobTask.IndexerSpokenLanguages == null)
            {
                jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
                jobTasks.Add(jobTask);
            }
            else
            {
                foreach (string spokenLanguage in jobTask.IndexerSpokenLanguages)
                {
                    processorOptions["Language"] = spokenLanguage;
                    jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
                    jobTasks.Add(jobTask);
                }
            }
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetFaceDetectionTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = jobTask.FaceEmotionDetect ? MediaProcessor.FaceDetectionEmotion : MediaProcessor.FaceDetection;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorFaceDetectionId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorFaceDetectionDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            JToken processorOptions = processorConfig["options"];
            processorOptions["mode"] = jobTask.FaceEmotionDetect ? "aggregateEmotion" : "faces";
            processorOptions["aggregateEmotionWindowMs"] = jobTask.FaceEmotionWindowMilliseconds;
            processorOptions["aggregateEmotionIntervalMs"] = jobTask.FaceEmotionIntervalMilliseconds;
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetFaceRedactionTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.FaceRedaction;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorFaceRedactionId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorFaceRedactionDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetMotionDetectionTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.MotionDetection;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorMotionDetectionId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorMotionDetectionDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetMotionHyperlapseTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.MotionHyperlapse;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorMotionHyperlapseId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorMotionHyperlapseDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            JToken processorSources = processorConfig["Sources"][0];
            processorSources["StartFrame"] = jobTask.HyperlapseStartFrame;
            processorSources["NumFrames"] = jobTask.HyperlapseFrameCount;
            JToken processorOptions = processorConfig["Options"];
            processorOptions["Speed"] = jobTask.HyperlapseSpeed;
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetVideoSummarizationTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.VideoSummarization;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorVideoSummarizationId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorVideoSummarizationDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            JToken processorOptions = processorConfig["options"];
            processorOptions["maxMotionThumbnailDurationInSecs"] = jobTask.SummaryDurationSeconds;
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }

        private static MediaJobTask[] GetCharacterRecognitionTasks(MediaClient mediaClient, MediaJobTask jobTask, MediaAssetInput[] inputAssets)
        {
            List<MediaJobTask> jobTasks = new List<MediaJobTask>();
            jobTask.MediaProcessor = MediaProcessor.CharacterRecognition;
            jobTask.Name = Selections.GetProcessorName(jobTask.MediaProcessor);
            string settingKey = Constants.AppSettings.MediaProcessorCharacterRecognitionId;
            string processorId = AppSetting.GetValue(settingKey);
            settingKey = Constants.AppSettings.MediaProcessorCharacterRecognitionDocumentId;
            string documentId = AppSetting.GetValue(settingKey);
            DatabaseClient databaseClient = new DatabaseClient();
            JObject processorConfig = databaseClient.GetDocument(documentId);
            jobTask = SetJobTask(mediaClient, jobTask, processorConfig.ToString(), inputAssets);
            jobTasks.Add(jobTask);
            return jobTasks.ToArray();
        }
    }
}
