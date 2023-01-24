namespace PersonalQQBotBackend.Processers;

public static class GNAAAndPWFProcesserProcesser {
	//private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static async Task ProcessAsync(ReceivedMessage message) {
		var isGroup = message.MessageType == "group" && message.SubType != "anonymous";

		await MessageTools.SendTextMessageAsync(isGroup
				? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
				: $"[CQ:reply,id={message.MessageId}]你好！",
			isGroup, isGroup ? message.GroupId : message.UserId, MessageTools.GetEcho(message)).ConfigureAwait(false);
	}
}