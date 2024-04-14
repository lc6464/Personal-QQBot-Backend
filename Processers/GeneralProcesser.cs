namespace PersonalQQBotBackend.Processers;

/// <summary>
/// 群聊非匿名 At 及好友私聊处理类
/// </summary>
public static class GeneralProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.ProgramLogger!;

	public static async Task ProcessAsync(ReceivedMessage message) {
		var isGroup = message.GetIsGroupNotAnonymous();

		var isLengthFrom5To128 = message.RawMessage!.Length is <= 128 and >= 18; // 判断字符数是否在5~128范围内
		if (isLengthFrom5To128) {
			var match = Regexes.InternetSearch().Match(message.RawMessage);
			if (match.Success) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中联网搜索功能。", LogLevel.Debug);
				var searchPath = "/search?q=" + System.Net.WebUtility.UrlEncode(match.Groups[1].Value);
				await MessageTools.SendTextMessageAsync($"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}] " : "")}关于您想要联网查询的信息，您可以参考：\r\nhttps://www.google.com{searchPath}\r\nhttps://cn.bing.com{searchPath}", isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
				return;
			}
		}

		/*
		 * 这玩意会导致机器人与别的机器人吵起来，所以不要了

		await MessageTools.SendTextMessageAsync(isGroup
				? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
				: $"[CQ:reply,id={message.MessageId}]你好！",
			isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);

		*/
	}
}