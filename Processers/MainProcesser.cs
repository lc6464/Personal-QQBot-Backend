using System.Text.RegularExpressions;

namespace PersonalQQBotBackend.Processers;

public static class MainProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.ProgramLogger!;

	public static readonly List<SendMessageAction> sendMessageActionsPool = new();

	public static async Task ProcessReceivedMessageAsync(ReceivedMessage message) {
		if (message.PostType == "message") {
			if (message.GetIsGroup() && message.RawMessage!.Contains("[CQ:redbag,title=")) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中红包提醒功能：{message.RawMessage}", LogLevel.Debug);
				await MessageTools.SendTextMessageAsync($"快到群 {message.GroupId} 中领取用户 {message.UserId} 发送的红包！", false, 1138_7791_74, message.GetEcho()).ConfigureAwait(false);
			}

			bool isGroupNotAnonymous = message.GetIsGroupNotAnonymous(), // 群聊非匿名
				isPrivateWithFriend = message.GetIsPrivateWithFriend(), // 私聊好友
				isAted = message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]");

			var isLengthFrom8To80 = message.RawMessage.Length is <= 80 and >= 8; // 判断字符数是否在8~80范围内

			Match aLiQueryRegexMatch = Match.Empty, biliUploaderQueryRegexMatch = Match.Empty;
			if ((isGroupNotAnonymous || isPrivateWithFriend) && isLengthFrom8To80) {
				aLiQueryRegexMatch = Regexes.ALiQuery().Match(message.RawMessage); // 获取阿梨相关查询功能的 Match
				if (!aLiQueryRegexMatch.Success) {
					biliUploaderQueryRegexMatch = Regexes.BiliUploaderQuery().Match(message.RawMessage); // 获取B站UP主相关查询功能的 Match
				}
			}


			if (aLiQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了阿梨相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中阿梨相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await ALiQueryProcesser.ProcessAsync(message, aLiQueryRegexMatch).ConfigureAwait(false);
			} else if (biliUploaderQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了B站UP主相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中B站UP主相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await BiliUploaderQueryProcesser.ProcessAsync(message, biliUploaderQueryRegexMatch).ConfigureAwait(false);
			} else if (isPrivateWithFriend && message.UserId == 1138_7791_74) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中 LC 私聊：{message.RawMessage}", LogLevel.Debug);
				await LCPProcesser.ProcessAsync(message).ConfigureAwait(false);
			} else if (isGroupNotAnonymous && isAted && message.GroupId == 522_071_644 && message.UserId == 1138_7791_74) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中 LC 测试群 At：{message.RawMessage}", LogLevel.Debug);
				await LCTGAProcesser.ProcessAsync(message).ConfigureAwait(false);
			} else if (isGroupNotAnonymous && isAted && message.UserId == 1138_7791_74) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中 LC 群聊 At：{message.RawMessage}", LogLevel.Debug);
				await LCGAProcesser.ProcessAsync(message).ConfigureAwait(false);
			} else if (isGroupNotAnonymous && isAted && message.GroupId == 522_071_644) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中测试群 At：{message.RawMessage}", LogLevel.Debug);
				await TGAProcesser.ProcessAsync(message).ConfigureAwait(false);
			} else if ((isGroupNotAnonymous && isAted) || isPrivateWithFriend) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} {message.MessageId} 命中群聊 At 或好友私聊：{message.RawMessage}", LogLevel.Debug);
				await GeneralProcesser.ProcessAsync(message).ConfigureAwait(false);
			}
		} else if (message.GetIsPoked()) {
			_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中戳一戳。", LogLevel.Debug);
			var isGroup = message.GroupId is not null;
			var messageText = isGroup ? $"[CQ:poke,qq={message.UserId}]" : "你好！";
			await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, message.GetEcho()).ConfigureAwait(false);
		} else if (!string.IsNullOrWhiteSpace(message.Echo)) {
			EchoProcesser.Process(message);
		}
	}
}