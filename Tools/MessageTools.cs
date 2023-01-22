namespace PersonalQQBotBackend.Tools;

public static class MessageTools {
	public static async Task SendSendMessageAsync(SendMessage message, Action<bool, ReceivedMessage?>? callback = null, int timeoutSecond = 10, Action? timeoutCallback = null) {
		lock (MainProcesser.sendMessageActionsPool) {
			MainProcesser.sendMessageActionsPool.Add(new(message, callback) { TimeoutSecond = timeoutSecond, TimeoutCallback = timeoutCallback });
		}
		await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(message)).ConfigureAwait(false);
	}

	public static async Task SendTextMessageAsync(string message, bool isGroup, long? targetId, string echo, Action<bool, ReceivedMessage?>? callback = null, int timeoutSecond = 10, Action? timeoutCallback = null) {
		SendMessageParams @params = isGroup ? new() { GroupId = targetId } : new() { UserId = targetId };
		@params.Message = message;
		await SendSendMessageAsync(new() { Action = "send_msg", Params = @params, Echo = echo }, callback, timeoutSecond, timeoutCallback).ConfigureAwait(false);
	}
}