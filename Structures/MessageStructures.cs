namespace PersonalQQBotBackend.Structures;

public struct ReceivedMessage {
	[JsonPropertyName("post_type")]
	public string? PostType { get; set; }

	[JsonPropertyName("message_type")]
	public string? MessageType { get; set; }

	[JsonPropertyName("meta_event_type")]
	public string? MetaEventType { get; set; }

	[JsonPropertyName("notice_type")]
	public string? NoticeType { get; set; }

	[JsonPropertyName("sub_type")]
	public string? SubType { get; set; }

	[JsonPropertyName("raw_message")]
	public string? RawMessage { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("message_id")]
	public long? MessageId { get; set; }

	[JsonPropertyName("message_seq")]
	public long? MessageSeq { get; set; }

	[JsonPropertyName("group_id")]
	public long? GroupId { get; set; }

	[JsonPropertyName("user_id")]
	public long? UserId { get; set; }

	[JsonPropertyName("self_id")]
	public long? SelfId { get; set; }

	[JsonPropertyName("target_id")]
	public long? TargetId { get; set; }

	[JsonPropertyName("data")]
	public ReceivedMessageData? Data { get; set; }

	[JsonPropertyName("echo")]
	public string? Echo { get; set; }

	[JsonPropertyName("retcode")]
	public int? Retcode { get; set; }

	[JsonPropertyName("status")]
	public object? Status { get; set; }

	[JsonPropertyName("msg")]
	public string? ErrorMessage { get; set; }

	[JsonPropertyName("wording")]
	public string? Wording { get; set; }

	[JsonPropertyName("time")]
	public long? Time { get; set; }
}


public struct ReceivedMessageData {
	[JsonPropertyName("message_id")]
	public long? MessageId { get; set; }
}


public struct SendMessage {
	[JsonPropertyName("action")]
	public string Action { get; set; }

	[JsonPropertyName("params")]
	public SendMessageParams? Params { get; set; }

	[JsonPropertyName("echo")]
	public string? Echo { get; set; }
}


public struct SendMessageParams {
	[JsonPropertyName("group_id")]
	public long? GroupId { get; set; }

	[JsonPropertyName("user_id")]
	public long? UserId { get; set; }

	[JsonPropertyName("message_id")]
	public long? MessageId { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }
}


public struct SendMessageAction {
	public SendMessageAction() { }
	public SendMessageAction(SendMessage message, Action<bool, ReceivedMessage?, SendMessageAction?>? callback = null) {
		Message = message;
		Callback = callback;
	}
	public SendMessage Message { get; set; }
	public Action<bool, ReceivedMessage?, SendMessageAction?>? Callback { get; set; } = null;
	public int TimeoutSecond { get; set; } = 10;
	public Action<SendMessageAction?>? TimeoutCallback { get; set; } = null;
	public DateTime CreatedTime { get; private init; } = DateTime.Now;
}