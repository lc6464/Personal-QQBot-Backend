using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PersonalQQBotBackend.Processers;

public static class MainProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static readonly List<SendMessageAction> sendMessageActionsPool = new();

	public static async Task ProcessReceivedMessageAsync(ReceivedMessage message) {
		var echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
		if (message.PostType == "message") {
			if (message.MessageType == "group" && (message.RawMessage?.Contains("[CQ:redbag,title=") ?? false)) {
				await MessageTools.SendTextMessageAsync($"快到群 {message.GroupId} 中领取用户 {message.UserId} 发送的红包！", false, 1138_7791_74, echo).ConfigureAwait(false);
			}
			var aLiQueryRegexMatch = Regexes.ALiQueryRegex().Match(message.RawMessage!); // 获取阿梨相关查询功能的 Match
			Match? biliUploaderQueryRegexMatch = null; // 获取B站UP主相关查询功能的 Match
			if (!aLiQueryRegexMatch.Success) {
				biliUploaderQueryRegexMatch = Regexes.BiliUploaderQueryRegex().Match(message.RawMessage!);
			}
			bool isGroupNotAnonymous = message.MessageType == "group" && message.SubType != "anonymous", // 群聊非匿名
				isPrivateWithFriend = message.MessageType == "private" && message.SubType == "friend"; // 私聊好友
			if ((isGroupNotAnonymous || isPrivateWithFriend) && aLiQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了阿梨相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中阿梨相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await ALiQueryProcesser.ProcessAsync(message, aLiQueryRegexMatch).ConfigureAwait(false);
			} else if ((isGroupNotAnonymous || isPrivateWithFriend) && biliUploaderQueryRegexMatch is not null && biliUploaderQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了B站UP主相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中B站UP主相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await BiliUploaderQueryProcesser.ProcessAsync(message, biliUploaderQueryRegexMatch).ConfigureAwait(false);
			} else if (isGroupNotAnonymous && message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]")) {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中群聊 At：{message.RawMessage}", LogLevel.Debug);
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						GroupId = message.GroupId,
						Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
					},
					Echo = echo
				};
				await MessageTools.SendSendMessageAsync(sendMessage).ConfigureAwait(false);
			} else if (isPrivateWithFriend) {
				_logger.LogWithTime($"{message.UserId} 命中好友私聊：{message.RawMessage}", LogLevel.Debug);
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						UserId = message.UserId,
						Message = $"[CQ:reply,id={message.MessageId}]你好！"
					},
					Echo = echo
				};
				await MessageTools.SendSendMessageAsync(sendMessage).ConfigureAwait(false);
			}
		} else if (message.PostType == "notice" && message.SubType == "poke" && message.TargetId == message.SelfId && message.UserId != message.SelfId) {
			_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中戳一戳。", LogLevel.Debug);
			var isGroup = message.GroupId is not null;
			var messageText = isGroup ? $"[CQ:poke,qq={message.UserId}]" : "你好！";
			await MessageTools.SendTextMessageAsync(messageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
		} else if (!string.IsNullOrWhiteSpace(message.Echo)) {
			EchoProcesser.Process(message);
		}
	}
}