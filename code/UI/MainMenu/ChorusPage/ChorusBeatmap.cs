using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace Rhythm4K.Chorus;

public class ChorusBeatmap
{
    [JsonPropertyName( "name" )]
    public string Name { get; set; }

    [JsonPropertyName( "artist" )]
    public string Artist { get; set; }

    [JsonPropertyName( "charter" )]
    public string Charter { get; set; }

    [JsonPropertyName( "album" )]
    public string Album { get; set; }

    [JsonPropertyName( "genre" )]
    public string Genre { get; set; }

    [JsonPropertyName( "year" )]
    public string Year { get; set; }

    [JsonPropertyName( "chartName" )]
    public object ChartName { get; set; }

    [JsonPropertyName( "chartAlbum" )]
    public object ChartAlbum { get; set; }

    [JsonPropertyName( "chartGenre" )]
    public object ChartGenre { get; set; }

    [JsonPropertyName( "chartYear" )]
    public object ChartYear { get; set; }

    [JsonPropertyName( "chartId" )]
    public object ChartId { get; set; }

    [JsonPropertyName( "songId" )]
    public object SongId { get; set; }

    [JsonPropertyName( "groupId" )]
    public object GroupId { get; set; }

    [JsonPropertyName( "chartDriveChartId" )]
    public object ChartDriveChartId { get; set; }

    [JsonPropertyName( "albumArtMd5" )]
    public object AlbumArtMd5 { get; set; }

    [JsonPropertyName( "md5" )]
    public object Md5 { get; set; }

    [JsonPropertyName( "chartMd5" )]
    public object ChartMd5 { get; set; }

    [JsonPropertyName( "versionGroupId" )]
    public object VersionGroupId { get; set; }

    [JsonPropertyName( "song_length" )]
    public object SongLength { get; set; }

    [JsonPropertyName( "diff_band" )]
    public object DifficultyBand { get; set; }

    [JsonPropertyName( "diff_guitar" )]
    public object DifficultyGuitar { get; set; }

    [JsonPropertyName( "diff_guitar_coop" )]
    public object DifficultyGuitarCoop { get; set; }

    [JsonPropertyName( "diff_rhythm" )]
    public object DifficultyRhythm { get; set; }

    [JsonPropertyName( "diff_bass" )]
    public object DifficultyBass { get; set; }

    [JsonPropertyName( "diff_drums" )]
    public object DifficultyDrums { get; set; }

    [JsonPropertyName( "diff_drums_real" )]
    public object DifficultyDrumsReal { get; set; }

    [JsonPropertyName( "diff_keys" )]
    public object DifficultyKeys { get; set; }

    [JsonPropertyName( "diff_guitarghl" )]
    public object DifficultyGuitarLive { get; set; }

    [JsonPropertyName( "diff_guitar_coop_ghl" )]
    public object DifficultyGuitarCoopLive { get; set; }

    [JsonPropertyName( "diff_rhythm_ghl" )]
    public object DifficultyRhythmLive { get; set; }

    [JsonPropertyName( "diff_bass_ghl" )]
    public object DifficultyBassLive { get; set; }

    [JsonPropertyName( "diff_vocals" )]
    public object DifficultyVocals { get; set; }

    [JsonPropertyName( "preview_start_time" )]
    public object PreviewStartTime { get; set; }

    [JsonPropertyName( "icon" )]
    public string Icon { get; set; }

    [JsonPropertyName( "loading_phrase" )]
    public string LoadingPhrase { get; set; }

    [JsonPropertyName( "album_track" )]
    public object AlbumTrack { get; set; }

    [JsonPropertyName( "playlist_track" )]
    public object PlaylistTrack { get; set; }

    [JsonPropertyName( "modchart" )]
    public bool Modchart { get; set; }

    [JsonPropertyName( "delay" )]
    public object Delay { get; set; }

    [JsonPropertyName( "chart_offset" )]
    public object ChartOffset { get; set; }

    [JsonPropertyName( "hopo_frequency" )]
    public object HopoFrequency { get; set; }

    [JsonPropertyName( "eigthnote_hopo" )]
    public bool EighthNoteHopo { get; set; }

    [JsonPropertyName( "multiplier_note" )]
    public object MultiplierNote { get; set; }

    [JsonPropertyName( "video_start_time" )]
    public object VideoStartTime { get; set; }

    [JsonPropertyName( "pro_drums" )]
    public bool ProDrums { get; set; }

    [JsonPropertyName( "end_events" )]
    public bool EndEvents { get; set; }

    [JsonPropertyName( "notesData" )]
    public ChorusNoteData NotesData { get; set; }

    [JsonPropertyName( "folderIssues" )]
    public List<object> FolderIssues { get; set; }

    [JsonPropertyName( "metadataIssues" )]
    public List<object> MetadataIssues { get; set; }

    [JsonPropertyName( "hasVideoBackground" )]
    public bool HasVideoBackground { get; set; }

    [JsonPropertyName( "modifiedTime" )]
    public object ModifiedTime { get; set; }

    [JsonPropertyName( "applicationDriveId" )]
    public object ApplicationDriveId { get; set; }

    [JsonPropertyName( "applicationUsername" )]
    public object ApplicationUsername { get; set; }

    [JsonPropertyName( "packName" )]
    public object PackName { get; set; }

    [JsonPropertyName( "parentFolderId" )]
    public object ParentFolderId { get; set; }

    [JsonPropertyName( "drivePath" )]
    public object DrivePath { get; set; }

    [JsonPropertyName( "driveFileId" )]
    public object DriveFileId { get; set; }

    [JsonPropertyName( "driveFileName" )]
    public object DriveFileName { get; set; }

    [JsonPropertyName( "driveChartIsPack" )]
    public bool DriveChartIsPack { get; set; }

    [JsonPropertyName( "archivePath" )]
    public object ArchivePath { get; set; }

    [JsonPropertyName( "chartFileName" )]
    public object ChartFileName { get; set; }

    public string GetFullSongId()
    {
        return $"{GroupId}-{SongId}-{ChartId}";
    }

    public bool IsDownloaded()
    {
        return FileSystem.Data.DirectoryExists( $"beatmaps/downloads-enchorus/{GetFullSongId()}" );
    }
}

public class ChorusNoteData
{
    [JsonPropertyName( "instruments" )]
    public List<string> Instruments { get; set; }

    [JsonPropertyName( "drumType" )]
    public string DrumType { get; set; }

    [JsonPropertyName( "hasSoloSections" )]
    public bool HasSoloSections { get; set; }

    [JsonPropertyName( "hasLyrics" )]
    public bool HasLyrics { get; set; }

    [JsonPropertyName( "hasVocals" )]
    public bool HasVocals { get; set; }

    [JsonPropertyName( "hasForcedNotes" )]
    public bool HasForcedNotes { get; set; }

    [JsonPropertyName( "hasTapNotes" )]
    public bool HasTapNotes { get; set; }

    [JsonPropertyName( "hasOpenNotes" )]
    public bool HasOpenNotes { get; set; }

    [JsonPropertyName( "has2xKick" )]
    public bool Has2xKick { get; set; }

    [JsonPropertyName( "hasRollLanes" )]
    public bool HasRollLanes { get; set; }

    [JsonPropertyName( "noteIssues" )]
    public List<object> NoteIssues { get; set; }

    [JsonPropertyName( "trackIssues" )]
    public List<object> TrackIssues { get; set; }

    [JsonPropertyName( "chartIssues" )]
    public List<object> ChartIssues { get; set; }

    [JsonPropertyName( "noteCounts" )]
    public List<ChorusNoteCount> NoteCounts { get; set; }

    [JsonPropertyName( "maxNps" )]
    public List<ChorusNoteInformation> NoteInformation { get; set; }

    [JsonPropertyName( "hashes" )]
    public List<ChorusHash> Hashes { get; set; }

    [JsonPropertyName( "tempoMapHash" )]
    public string TempoMapHash { get; set; }

    [JsonPropertyName( "tempoMarkerCount" )]
    public int TempoMarkerCount { get; set; }

    [JsonPropertyName( "length" )]
    public int Length { get; set; }

    [JsonPropertyName( "effectiveLength" )]
    public int EffectiveLength { get; set; }
}

public class ChorusNoteCount
{
    [JsonPropertyName( "instrument" )]
    public string Instrument { get; set; }

    [JsonPropertyName( "difficulty" )]
    public string Difficulty { get; set; }

    [JsonPropertyName( "count" )]
    public int NoteCount { get; set; }
}

public class ChorusNoteInformation
{
    [JsonPropertyName( "instrument" )]
    public string Instrument { get; set; }

    [JsonPropertyName( "difficulty" )]
    public string Difficulty { get; set; }

    [JsonPropertyName( "time" )]
    public float Time { get; set; }

    [JsonPropertyName( "nps" )]
    public float NotesPerSecond { get; set; }

    [JsonPropertyName( "notes" )]
    public List<ChorusNote> Notes { get; set; }
}

public class ChorusNote
{
    [JsonPropertyName( "type" )]
    public string Type { get; set; }

    [JsonPropertyName( "time" )]
    public float Time { get; set; }

    [JsonPropertyName( "length" )]
    public float Length { get; set; }
}

public class ChorusHash
{
    [JsonPropertyName( "instrument" )]
    public string Instrument { get; set; }

    [JsonPropertyName( "difficulty" )]
    public string Difficulty { get; set; }

    [JsonPropertyName( "hash" )]
    public string Hash { get; set; }
}