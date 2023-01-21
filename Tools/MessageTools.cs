namespace PersonalQQBotBackend.Tools;

public static class MessageTools {
	public static async Task SendSendMessageAsync(SendMessage message, Action<bool, ReceivedMessage?>? callback = null) {
		lock (MainProcesser.sendMessageActionsPool) {
			MainProcesser.sendMessageActionsPool.Add(new() { Message = message, Callback = callback });
		}
		await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(message)).ConfigureAwait(false);
	}

	public static async Task SendTextMessageAsync(string message, bool isGroup, long? targetId, string echo) {
		SendMessageParams @params = isGroup ? new() { GroupId = targetId } : new() { UserId = targetId };
		@params.Message = message;
		SendMessage sendMessage = new() { Action = "send_msg", Params = @params, Echo = echo };
		await SendSendMessageAsync(sendMessage).ConfigureAwait(false);
	}
}