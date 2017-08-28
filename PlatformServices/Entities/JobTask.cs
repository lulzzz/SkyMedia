﻿using System.Collections.Generic;

using Microsoft.WindowsAzure.MediaServices.Client;

namespace AzureSkyMedia.PlatformServices
{
    public class MediaJobTask
    {
        public MediaJobTask()
        {
            this.ProcessorConfigBoolean = new Dictionary<string, bool>();
            this.ProcessorConfigInteger = new Dictionary<string, int>();
            this.ProcessorConfigString = new Dictionary<string, string>();
        }

        public MediaJobTask CreateCopy()
        {
            MediaJobTask newTask = (MediaJobTask)this.MemberwiseClone();
            foreach (KeyValuePair<string, bool> keyValue in this.ProcessorConfigBoolean)
            {
                newTask.ProcessorConfigBoolean.Add(keyValue.Key, keyValue.Value);
            }
            foreach (KeyValuePair<string, int> keyValue in this.ProcessorConfigInteger)
            {
                newTask.ProcessorConfigInteger.Add(keyValue.Key, keyValue.Value);
            }
            foreach (KeyValuePair<string, string> keyValue in this.ProcessorConfigString)
            {
                newTask.ProcessorConfigString.Add(keyValue.Key, keyValue.Value);
            }
            return newTask;
        }

        public int? ParentIndex { get; set; }

        public string Name { get; set; }

        public string[] InputAssetIds { get; set; }

        public MediaProcessor MediaProcessor { get; set; }

        public string ProcessorConfig { get; set; }

        public string ProcessorDocumentId { get; set; }

        public string OutputAssetName { get; set; }

        public AssetCreationOptions OutputAssetEncryption { get; set; }

        public AssetFormatOption OutputAssetFormat { get; set; }

        public TaskOptions Options { get; set; }

        public ContentProtection ContentProtection { get; set; }

        public Dictionary<string, bool> ProcessorConfigBoolean { get; set; }

        public Dictionary<string, int> ProcessorConfigInteger { get; set; }

        public Dictionary<string, string> ProcessorConfigString { get; set; }
    }
}