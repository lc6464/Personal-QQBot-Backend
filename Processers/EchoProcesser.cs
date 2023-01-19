namespace PersonalQQBotBackend.Processers;

public static class EchoProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static void Process(ReceivedMessage message) {
		SendMessage sendMessage;
		lock (MainProcesser.sendMessagesPool) {
			sendMessage = MainProcesser.sendMessagesPool.Find(x => x.Echo == message.Echo);
			MainProcesser.sendMessagesPool.Remove(sendMessage);
		}
		if (message.Status?.ToString() is "ok" or "async") {
			_logger.LogWithTime($"发送消息成功：{message.Echo}，Message ID: {message.Data?.MessageId}");
			_logger.LogWithTime($"消息 {message.Data?.MessageId} 的内容：{sendMessage.Params?.Message}", LogLevel.Debug);
		} else {
			_logger.LogWithTime($"发送消息失败：{message.Echo}\r\nRetcode：{message.Retcode}\r\n发送的消息内容：{sendMessage.Params?.Message}", LogLevel.Error);
		}
	}
}