namespace PersonalQQBotBackend.Processers;

/// <summary>
/// LC 群聊 At 处理类
/// </summary>
public static class LCGAProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.ProgramLogger!;

	public static async Task ProcessAsync(ReceivedMessage message) {





		_logger.LogWithTime($"{message.MessageId} 未命中任何功能，转到 测试群 At 处理类。", LogLevel.Debug);
		await TGAProcesser.ProcessAsync(message).ConfigureAwait(false);
	}
}