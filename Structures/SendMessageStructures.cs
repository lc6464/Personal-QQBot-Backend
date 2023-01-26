namespace PersonalQQBotBackend.Structures;

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

	[JsonPropertyName("auto_escape")]
	public bool? AutoEscape { get; set; }

	[JsonPropertyName("model")]
	public string? Model { get; set; }

	[JsonPropertyName("model_show")]
	public string? ModelShow { get; set; }
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