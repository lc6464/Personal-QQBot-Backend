namespace PersonalQQBotBackend.Processers;

/// <summary>
/// 测试群 At 处理类
/// </summary>
public static class TGAProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.ProgramLogger!;

	public static async Task ProcessAsync(ReceivedMessage message) {
		var isGroup = message.GetIsGroupNotAnonymous();
		if (message.RawMessage!.Contains("回调测试") || message.RawMessage.Contains("撤回测试")) {
			_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中回调或撤回测试：{message.RawMessage}", LogLevel.Debug);
			var messageText = $"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}]" : "")}请等待约五秒（撤回测试可能会低于五秒）。";
			await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho(),
				async (success, callbackMessage, sendMessageAction) => {
					if (success) {
						_logger.LogWithTime($"回调或撤回测试：发送消息成功：{sendMessageAction?.Message.Echo}", LogLevel.Debug);
						if (message.RawMessage.Contains("回调测试")) {
							messageText = $"[CQ:reply,id={callbackMessage?.Data?.MessageId}]回调测试成功。";
							await Task.Delay(5000).ConfigureAwait(false);
							await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
						} else {
							SendMessage sendMessage = new() { Action = "delete_msg", Params = new() { MessageId = callbackMessage?.Data?.MessageId }, Echo = message.GetEcho() };
							await Task.Delay(5000).ConfigureAwait(false);
							await sendMessage.SendSendMessageAsync().ConfigureAwait(false);
						}
					} else {
						_logger.LogWithTime($"回调或撤回测试：发送消息失败：{sendMessageAction?.Message.Echo}", LogLevel.Debug);
					}
				}, 5, async sendMessageAction => {
					_logger.LogWithTime($"回调或撤回测试：回调超时：{sendMessageAction?.Message.Echo}", LogLevel.Error);
					messageText = "回调超时，未在五秒内接收到响应信息！";
					await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
				}).ConfigureAwait(false);
			return;
		}
		var isLengthFrom18To80 = message.RawMessage.Length is <= 64 and >= 18; // 判断字符数是否在18~64范围内
		if (isLengthFrom18To80) {
			var match = Regexes.ParseQQFaceRegex().Match(message.RawMessage);
			if (match.Success) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中QQ表情ID获取功能：{message.RawMessage}", LogLevel.Debug);
				await MessageTools.SendTextMessageAsync($"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}]" : "")}您提供的QQ表情ID为：{match.Groups[1].Value}", isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
				return;
			}
		}



		_logger.LogWithTime($"{message.MessageId} 未命中任何功能，转到 群聊非匿名 At 及好友私聊处理类。", LogLevel.Debug);
		await GNAAAndPWFProcesserProcesser.ProcessAsync(message).ConfigureAwait(false);
	}
}