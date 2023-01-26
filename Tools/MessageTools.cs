namespace PersonalQQBotBackend.Tools;

public static class MessageTools {
	public static async Task SendSendMessageAsync(this SendMessage message, Action<bool, ReceivedMessage?, SendMessageAction?>? callback = null, int timeoutSecond = 10, Action<SendMessageAction?>? timeoutCallback = null) {
		lock (MainProcesser.sendMessageActionsPool) {
			MainProcesser.sendMessageActionsPool.Add(new(message, callback) { TimeoutSecond = timeoutSecond, TimeoutCallback = timeoutCallback });
		}
		await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(message)).ConfigureAwait(false);
	}

	public static async Task SendTextMessageAsync(string message, bool isGroup, long? targetId, string echo, Action<bool, ReceivedMessage?, SendMessageAction?>? callback = null, int timeoutSecond = 10, Action<SendMessageAction?>? timeoutCallback = null) {
		SendMessageParams @params = isGroup ? new() { GroupId = targetId } : new() { UserId = targetId };
		@params.Message = message;
		await SendSendMessageAsync(new() { Action = "send_msg", Params = @params, Echo = echo }, callback, timeoutSecond, timeoutCallback).ConfigureAwait(false);
	}

	public static string GetEcho(this ReceivedMessage message) =>
		$"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";

	public static bool GetIsGroup(this ReceivedMessage message) => message.MessageType == "group";

	public static bool GetIsGroupNotAnonymous(this ReceivedMessage message) => GetIsGroup(message) && message.SubType != "anonymous";

	public static bool GetIsPrivate(this ReceivedMessage message) => message.MessageType == "private";

	public static bool GetIsPrivateWithFriend(this ReceivedMessage message) => GetIsPrivate(message) && message.SubType == "friend";

	/// <summary>
	/// 判断是否为戳一戳
	/// </summary>
	/// <param name="message">接收到的消息</param>
	/// <returns>若为戳一戳，则为 <see cref="true"/>；否则为 <see cref="false"/>。</returns>
	public static bool GetIsPoke(this ReceivedMessage message) => message.PostType == "notice" && message.SubType == "poke";

	/// <summary>
	/// 判断是否被戳一戳
	/// </summary>
	/// <param name="message">接收到的消息</param>
	/// <returns>若被戳一戳，则为 <see cref="true"/>；否则为 <see cref="false"/>。</returns>
	public static bool GetIsPoked(this ReceivedMessage message) => GetIsPoke(message) && message.TargetId == message.SelfId && message.UserId != message.SelfId;
}