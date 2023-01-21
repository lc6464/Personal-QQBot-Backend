namespace PersonalQQBotBackend.Structures;

public struct BiliApiSpaceInfo {
	[JsonPropertyName("code")]
	public int Code { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("ttl")]
	public int Ttl { get; set; }

	[JsonPropertyName("data")]
	public BiliApiSpaceInfoData? Data { get; set; }
}

public struct BiliApiSpaceInfoData {
	[JsonPropertyName("mid")]
	public long Mid { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("live_room")]
	public BiliApiSpaceInfoDataLiveRoom? LiveRoom { get; set; }
}

public struct BiliApiSpaceInfoDataLiveRoom {
	[JsonPropertyName("roomStatus")]
	public int RoomStatus { get; set; }

	[JsonPropertyName("roomid")]
	public long? RoomId { get; set; }

	[JsonPropertyName("liveStatus")]
	public int? LiveStatus { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("title")]
	public string? Title { get; set; }

	[JsonPropertyName("cover")]
	public string? Cover { get; set; }

	[JsonPropertyName("roundStatus")]
	public int? RoundStatus { get; set; }

	[JsonPropertyName("watched_show")]
	public BiliApiSpaceInfoDataLiveRoomWatchedShow? WatchedShow { get; set; }
}

public struct BiliApiSpaceInfoDataLiveRoomWatchedShow {
	[JsonPropertyName("num")]
	public long Number { get; set; }
}


public struct BiliApiUploaderVideosInfo {
	[JsonPropertyName("code")]
	public int Code { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("ttl")]
	public int Ttl { get; set; }

	[JsonPropertyName("data")]
	public BiliApiUploaderVideosInfoData? Data { get; set; }
}

public struct BiliApiUploaderVideosInfoData {
	[JsonPropertyName("list")]
	public BiliApiUploaderVideosInfoDataList List { get; set; }

	[JsonPropertyName("page")]
	public BiliApiUploaderVideosInfoDataPage Page { get; set; }
}

public struct BiliApiUploaderVideosInfoDataList {
	[JsonPropertyName("vlist")]
	public BiliApiUploaderVideosInfoDataListVListItem[] VList { get; set; }
}

public struct BiliApiUploaderVideosInfoDataListVListItem {
	[JsonPropertyName("aid")]
	public long Aid { get; set; }

	[JsonPropertyName("bvid")]
	public string Bvid { get; set; }

	[JsonPropertyName("author")]
	public string Author { get; set; }

	[JsonPropertyName("mid")]
	public long Mid { get; set; }

	[JsonPropertyName("play")]
	public long Play { get; set; }

	[JsonPropertyName("comment")]
	public long Comment { get; set; }

	[JsonPropertyName("typeid")]
	public long TypeId { get; set; }

	[JsonPropertyName("pic")]
	public string Picture { get; set; }

	[JsonPropertyName("title")]
	public string Title { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("created")]
	public long Created { get; set; }

	[JsonPropertyName("length")]
	public string Length { get; set; }

	[JsonPropertyName("is_union_video")]
	public int IsUnionVideo { get; set; }
}

public struct BiliApiUploaderVideosInfoDataPage {
	[JsonPropertyName("count")]
	public int Count { get; set; }

	[JsonPropertyName("pn")]
	public int PageNumber { get; set; }

	[JsonPropertyName("ps")]
	public int PageSize { get; set; }
}