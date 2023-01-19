namespace PersonalQQBotBackend.Tools;

public static class MessageTools {
	public static async Task SendSendMessage(SendMessage message) {
		lock (MainProcesser.sendMessagesPool) {
			MainProcesser.sendMessagesPool.Add(message);
		}
		await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(message)).ConfigureAwait(false);
	}
}