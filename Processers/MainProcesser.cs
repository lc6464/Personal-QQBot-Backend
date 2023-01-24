using System.Text.RegularExpressions;

namespace PersonalQQBotBackend.Processers;

public static class MainProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static readonly List<SendMessageAction> sendMessageActionsPool = new();

	public static async Task ProcessReceivedMessageAsync(ReceivedMessage message) {
		var isPoked = message.PostType == "notice" && message.SubType == "poke" && message.TargetId == message.SelfId && message.UserId != message.SelfId;
		if (message.PostType == "message") {
			if (message.MessageType == "group" && message.RawMessage!.Contains("[CQ:redbag,title=")) {
				await MessageTools.SendTextMessageAsync($"快到群 {message.GroupId} 中领取用户 {message.UserId} 发送的红包！", false, 1138_7791_74, MessageTools.GetEcho(message)).ConfigureAwait(false);
			}

			bool isGroupNotAnonymous = message.MessageType == "group" && message.SubType != "anonymous", // 群聊非匿名
				isPrivateWithFriend = message.MessageType == "private" && message.SubType == "friend", // 私聊好友
				isAted = message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]");

			var isLengthFrom8To80 = message.RawMessage.Length is <= 80 and >= 8; // 判断字符数是否在8~80范围内

			Match aLiQueryRegexMatch = Match.Empty, biliUploaderQueryRegexMatch = Match.Empty;
			if ((isGroupNotAnonymous || isPrivateWithFriend) && isLengthFrom8To80) {
				aLiQueryRegexMatch = Regexes.ALiQueryRegex().Match(message.RawMessage); // 获取阿梨相关查询功能的 Match
				if (!aLiQueryRegexMatch.Success) {
					biliUploaderQueryRegexMatch = Regexes.BiliUploaderQueryRegex().Match(message.RawMessage); // 获取B站UP主相关查询功能的 Match
				}
			}


			if (aLiQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了阿梨相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中阿梨相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await ALiQueryProcesser.ProcessAsync(message, aLiQueryRegexMatch).ConfigureAwait(false);
			} else if (biliUploaderQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了B站UP主相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中B站UP主相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await BiliUploaderQueryProcesser.ProcessAsync(message, biliUploaderQueryRegexMatch).ConfigureAwait(false);
			} else if (((isGroupNotAnonymous && message.GroupId == 5220_71644 && isAted) || (isPrivateWithFriend && message.UserId == 1138_7791_74)) && (message.RawMessage.Contains("回调测试") || message.RawMessage.Contains("撤回测试"))) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中回调或撤回测试：{message.RawMessage}", LogLevel.Debug);
				var isGroup = message.GroupId is not null;
				var messageText = $"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}]" : "")} 请等待约五秒（撤回测试可能会低于五秒）。";
				await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, MessageTools.GetEcho(message),
					async (success, callbackMessage, sendMessageAction) => {
						if (success) {
							_logger.LogWithTime($"回调或撤回测试：发送消息成功：{sendMessageAction?.Message.Echo}", LogLevel.Debug);
							if (message.RawMessage.Contains("回调测试")) {
								messageText = $"[CQ:reply,id={callbackMessage?.Data?.MessageId}]回调测试成功。";
								await Task.Delay(5000).ConfigureAwait(false);
								await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, MessageTools.GetEcho(message)).ConfigureAwait(false);
							} else {
								SendMessage sendMessage = new() { Action = "delete_msg", Params = new() { MessageId = callbackMessage?.Data?.MessageId }, Echo = MessageTools.GetEcho(message) };
								SendMessageAction action = new(sendMessage);
								await Task.Delay(5000).ConfigureAwait(false);
								lock (sendMessageActionsPool) {
									sendMessageActionsPool.Add(action);
								}
								await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage)).ConfigureAwait(false);
							}
						} else {
							_logger.LogWithTime($"回调或撤回测试：发送消息失败：{sendMessageAction?.Message.Echo}", LogLevel.Debug);
						}
					}, 5, async (sendMessageAction) => {
						_logger.LogWithTime($"回调或撤回测试：回调超时：{sendMessageAction?.Message.Echo}", LogLevel.Error);
						messageText = "回调超时，未在五秒内接收到响应信息！";
						await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, MessageTools.GetEcho(message)).ConfigureAwait(false);
					}).ConfigureAwait(false);
			} else if ((isGroupNotAnonymous && isAted) || isPrivateWithFriend) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中群聊 At 或好友私聊：{message.RawMessage}", LogLevel.Debug);
				await GNAAAndPWFProcesserProcesser.ProcessAsync(message).ConfigureAwait(false);
			}
		} else if (isPoked) {
			_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中戳一戳。", LogLevel.Debug);
			var isGroup = message.GroupId is not null;
			var messageText = isGroup ? $"[CQ:poke,qq={message.UserId}]" : "你好！";
			await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, MessageTools.GetEcho(message)).ConfigureAwait(false);
		} else if (!string.IsNullOrWhiteSpace(message.Echo)) {
			EchoProcesser.Process(message);
		}
	}
}