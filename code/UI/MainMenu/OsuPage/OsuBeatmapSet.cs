using Sandbox;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Rhythm4K.Osu;

public class OsuBeatmapSet
{
    [JsonPropertyName( "id" )]
    public long Id { get; set; }

    [JsonPropertyName( "unique_id" )]
    public string UniqueId { get; set; }

    [JsonPropertyName( "title" )]
    public object Title { get; set; }

    [JsonPropertyName( "artist" )]
    public object Artist { get; set; }

    [JsonPropertyName( "beatmaps" )]
    public List<OsuBeatmap> Beatmaps { get; set; }

    [JsonPropertyName( "average_length" )]
    public object AverageLength { get; set; }

    [JsonPropertyName( "bpm" )]
    public object BPM { get; set; }

    [JsonPropertyName( "covers" )]
    public Dictionary<string, object> Covers { get; set; }

    [JsonPropertyName( "creator" )]
    public object Creator { get; set; }

    [JsonPropertyName( "discussion_enabled" )]
    public object DiscussionEnabled { get; set; }

    [JsonPropertyName( "dmca_removed" )]
    public object DmcaRemoved { get; set; }

    [JsonPropertyName( "favourite_count" )]
    public object FavouriteCount { get; set; }

    [JsonPropertyName( "has_favourited" )]
    public object HasFavourited { get; set; }

    [JsonPropertyName( "has_scores" )]
    public object HasScores { get; set; }

    [JsonPropertyName( "hype_current" )]
    public object CurrentHype { get; set; }

    [JsonPropertyName( "hype_required" )]
    public object RequiredHype { get; set; }

    [JsonPropertyName( "last_updated" )]
    public object LastUpdated { get; set; }

    [JsonPropertyName( "legacy_thread_url" )]
    public object LegacyThreadUrl { get; set; }

    [JsonPropertyName( "mode_ctb" )]
    public object ModeCtb { get; set; }

    [JsonPropertyName( "mode_mania" )]
    public object ModeMania { get; set; }

    [JsonPropertyName( "mode_std" )]
    public object ModeStd { get; set; }

    [JsonPropertyName( "mode_taiko" )]
    public object ModeTaiko { get; set; }

    [JsonPropertyName( "nominations_current" )]
    public object CurrentNominations { get; set; }

    [JsonPropertyName( "nominations_required" )]
    public object RequiredNominations { get; set; }

    [JsonPropertyName( "preview_url" )]
    public object PreviewUrl { get; set; }

    [JsonPropertyName( "ranked" )]
    public object Ranked { get; set; }

    [JsonPropertyName( "ranked_date" )]
    public object RankedDate { get; set; }

    [JsonPropertyName( "source" )]
    public object Source { get; set; }

    [JsonPropertyName( "status" )]
    public object Status { get; set; }

    [JsonPropertyName( "storyboard" )]
    public bool Storyboard { get; set; }

    [JsonPropertyName( "submitted_date" )]
    public object SubmittedDate { get; set; }

    [JsonPropertyName( "tags" )]
    public object Tags { get; set; }

    [JsonPropertyName( "user_id" )]
    public object UserId { get; set; }

    [JsonPropertyName( "Video" )]
    public bool Video { get; set; }

    [JsonPropertyName( "download_date" )]
    public object DownloadDate { get; set; }

    [JsonPropertyName( "checksum_md5" )]
    public object ChecksumMd5 { get; set; }

    [JsonPropertyName( "checksum_sha1" )]
    public object ChecksumSha1 { get; set; }


    public string GetFullSongId()
    {
        if ( UniqueId is not null )
        {
            return $"{Id.ToString()}-{UniqueId.ToString()}";
        }
        else
        {
            return Id.ToString();
        }
    }

    public bool IsDownloaded()
    {
        return FileSystem.Data.DirectoryExists( $"beatmaps/downloads-beatconnect/{GetFullSongId()}" );
    }

}