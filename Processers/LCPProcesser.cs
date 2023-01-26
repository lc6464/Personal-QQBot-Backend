namespace PersonalQQBotBackend.Processers;

/// <summary>
/// LC 私聊处理类
/// </summary>
public static class LCPProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static async Task ProcessAsync(ReceivedMessage message) {




		_logger.LogWithTime($"{message.MessageId} 未命中任何功能，转到 LC 测试群 At 处理类。", LogLevel.Debug);
		await LCTGAProcesser.ProcessAsync(message).ConfigureAwait(false);
	}
}