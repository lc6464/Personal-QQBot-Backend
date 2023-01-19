namespace PersonalQQBotBackend.Processers;

public static class EchoProcesser {
	private static readonly ILogger _logger = LoggerProvider.logger;

	public static void Process(ReceivedMessage message) {
		lock (MainProcesser.sendMessagesPool) {
			MainProcesser.sendMessagesPool.RemoveAll(x => x.Echo == message.Echo);
		}
		if (message.Status?.ToString() is "ok" or "async") {
			_logger.LogWithTime($"发送消息失败：{message.Echo}\r\n消息内容：{message.RawMessage}", LogLevel.Error);
		} else {
			_logger.LogWithTime($"发送消息成功：{message.Echo}，Message ID: {message.Data?.MessageId}");
			_logger.LogWithTime($"消息内容：{message.RawMessage}", LogLevel.Debug);
		}
	}
}