﻿using System.Collections.Generic;

using Microsoft.WindowsAzure.MediaServices.Client;

namespace AzureSkyMedia.PlatformServices
{
    internal partial class MediaClient
    {
        public IStreamingAssetFilter CreateFilter(IAsset asset, string filterName, int markIn, int markOut, int? bitrate)
        {
            ulong? timescale = 1;
            ulong? markInSeconds = (ulong?)markIn;
            ulong? markOutSeconds = markOut > 0 ? (ulong?)markOut : null;
            PresentationTimeRange timeRange = new PresentationTimeRange(timescale, markInSeconds, markOutSeconds);
            List<FilterTrackSelectStatement> trackConditions = new List<FilterTrackSelectStatement>();
            FirstQuality firstQuality = bitrate.HasValue ? new FirstQuality(bitrate.Value) : null;
            return asset.AssetFilters.Create(filterName, timeRange, trackConditions, firstQuality);
        }
    }
}