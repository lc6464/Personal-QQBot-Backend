namespace PersonalQQBotBackend.Processers;

public static class EchoProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static void Process(ReceivedMessage message) {
		SendMessageAction sendMessageAction = default;
		var removeSuccess = false;
		lock (MainProcesser.sendMessageActionsPool) {
			sendMessageAction = MainProcesser.sendMessageActionsPool.Find(x => x.Message.Echo == message.Echo);
			removeSuccess = MainProcesser.sendMessageActionsPool.Remove(sendMessageAction);
		}
		var success = message.Status?.ToString() is "ok" or "async";
		var sendMessage = sendMessageAction.Message;
		if (removeSuccess) {
			if (success) {
				_logger.LogWithTime($"发送消息成功：{message.Echo}，发送的消息的 Action：{sendMessage.Action}，Message ID: {message.Data?.MessageId}");
				_logger.LogWithTime($"消息 {message.Data?.MessageId} 的内容：{sendMessage.Params?.Message}", LogLevel.Debug);
			} else {
				_logger.LogWithTime($"发送消息失败：{message.Echo}\r\nError Message：{message.ErrorMessage} Wording：{message.Wording} Retcode：{message.Retcode}\r\n发送的消息内容：{sendMessage.Params?.Message}", LogLevel.Error);
			}
			sendMessageAction.Callback?.Invoke(success, message, sendMessageAction);
		} else {
			_logger.LogWithTime($"未找到对应的发送消息动作，可能是由于超时而被移除：{message.Echo}", LogLevel.Error);
			if (success) {
				_logger.LogWithTime($"发送消息成功：{message.Echo}，Message ID: {message.Data?.MessageId}，发送的消息内容已丢失。");
			} else {
				_logger.LogWithTime($"发送消息失败：{message.Echo}\r\nError Message：{message.ErrorMessage} Wording：{message.Wording} Retcode：{message.Retcode}\r\n发送的消息内容已丢失。", LogLevel.Error);
			}
		}
	}
}