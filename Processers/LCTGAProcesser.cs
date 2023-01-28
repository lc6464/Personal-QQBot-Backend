namespace PersonalQQBotBackend.Processers;

/// <summary>
/// LC 测试群 At 处理类
/// </summary>
public static class LCTGAProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.ProgramLogger!;

	public static async Task ProcessAsync(ReceivedMessage message) {
		var isGroup = message.GetIsGroupNotAnonymous();
		var isLengthFrom5To80 = message.RawMessage!.Length is <= 64 and >= 5; // 判断字符数是否在5~64范围内
		if (isLengthFrom5To80) {
			var match = Regexes.SetQQOnlineModelRegex().Match(message.RawMessage);
			if (match.Success) {
				_logger.LogWithTime($"{message.MessageId} 命中设置机型功能。", LogLevel.Debug);
				var model = match.Groups[1].Value;
				SendMessage sendMessage = new() {
					Action = "_set_model_show",
					Params = new() {
						Model = model,
						ModelShow = model
					},
					Echo = message.GetEcho()
				};
				var lastMessage = message;
				await sendMessage.SendSendMessageAsync(async (success, message, action) => {
					if (success) {
						_logger.LogWithTime($"{lastMessage.GroupId} {lastMessage.UserId} {lastMessage.MessageId} 尝试设置机型：{model}");
						await MessageTools.SendTextMessageAsync($"[CQ:reply,id={lastMessage.MessageId}]{(isGroup ? $" [CQ:at,qq={lastMessage.UserId}]" : "")}已设置机型，请自行查看是否成功，机型为：{model}", isGroup, isGroup ? lastMessage.GroupId : lastMessage.UserId, lastMessage.GetEcho()).ConfigureAwait(false);
					} else {
						_logger.LogWithTime($"{lastMessage.GroupId} {lastMessage.UserId} {lastMessage.MessageId} 设置机型失败：{model}", LogLevel.Error);
						await MessageTools.SendTextMessageAsync($"[CQ:reply,id={lastMessage.MessageId}]{(isGroup ? $" [CQ:at,qq={lastMessage.UserId}]" : "")}设置机型失败，请查看日志。", isGroup, isGroup ? lastMessage.GroupId : lastMessage.UserId, lastMessage.GetEcho()).ConfigureAwait(false);
					}
				}, timeoutCallback: async action =>
					await MessageTools.SendTextMessageAsync($"[CQ:reply,id={lastMessage.MessageId}]{(isGroup ? $" [CQ:at,qq={lastMessage.UserId}]" : "")}设置机型超时，请查看日志。", isGroup, isGroup ? lastMessage.GroupId : lastMessage.UserId, lastMessage.GetEcho()).ConfigureAwait(false)
				).ConfigureAwait(false);
				return;
			}
		}



		_logger.LogWithTime($"{message.MessageId} 未命中任何功能，转到 LC 群聊 At 处理类。", LogLevel.Debug);
		await LCGAProcesser.ProcessAsync(message).ConfigureAwait(false);
	}
}