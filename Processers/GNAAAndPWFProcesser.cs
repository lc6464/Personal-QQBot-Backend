namespace PersonalQQBotBackend.Processers;

/// <summary>
/// 群聊非匿名 At 及好友私聊处理类
/// </summary>
public static class GNAAAndPWFProcesserProcesser {
	//private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static async Task ProcessAsync(ReceivedMessage message) {
		var isGroup = message.GetIsGroupNotAnonymous();

		await MessageTools.SendTextMessageAsync(isGroup
				? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
				: $"[CQ:reply,id={message.MessageId}]你好！",
			isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
	}
}