namespace OnlineMusicApi;

/// <summary>歌曲在某个音源内的唯一标识。</summary>
/// <param name="Source">所属音源。</param>
/// <param name="Value">音源内的原始 ID。</param>
public readonly record struct SongId(MusicSource Source, string Value);

/// <summary>专辑在某个音源内的唯一标识。</summary>
/// <param name="Source">所属音源。</param>
/// <param name="Value">音源内的原始 ID。</param>
public readonly record struct AlbumId(MusicSource Source, string Value);

/// <summary>艺术家在某个音源内的唯一标识。</summary>
/// <param name="Source">所属音源。</param>
/// <param name="Value">音源内的原始 ID。</param>
public readonly record struct ArtistId(MusicSource Source, string Value);

/// <summary>歌单在某个音源内的唯一标识。</summary>
/// <param name="Source">所属音源。</param>
/// <param name="Value">音源内的原始 ID。</param>
public readonly record struct PlaylistId(MusicSource Source, string Value);
