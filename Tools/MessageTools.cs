namespace PersonalQQBotBackend.Tools;

public static class MessageTools {
	public static async Task SendSendMessageAsync(SendMessage message, Action<bool, ReceivedMessage?, SendMessageAction?>? callback = null, int timeoutSecond = 10, Action<SendMessageAction?>? timeoutCallback = null) {
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

	public static string GetEcho(ReceivedMessage message) =>
		$"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
}