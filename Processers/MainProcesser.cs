using Microsoft.Extensions.Caching.Memory;

namespace PersonalQQBotBackend.Processers;

public static class MainProcesser {
	//private static readonly ClientWebSocket ws = WebSocketProvider.WebSocket;
	private static readonly ILogger _logger = LoggerProvider.logger;

	public static readonly List<SendMessage> sendMessagesPool = new();

	public static async Task ProcessReceivedMessageAsync(ReceivedMessage message) {
		if (message.PostType == "message") {
			var aLiQueryRegexMatch = Regexes.ALiQueryRegex().Match(message.RawMessage!); // 获取阿梨相关查询功能的 Match
			if ((message.MessageType == "group" && message.SubType != "anonymous" || message.MessageType == "private" && message.SubType == "friend") && aLiQueryRegexMatch.Success) {
				// 如果是群消息且不是匿名消息，或者是私聊消息且是好友消息，且匹配到了阿梨相关查询功能的正则表达式
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中阿梨相关查询功能：{message.RawMessage}", LogLevel.Debug);
				await ALiQueryProcesser.Process(message, aLiQueryRegexMatch).ConfigureAwait(false);
			} else if (message.MessageType == "group" && message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]") && message.SubType != "anonymous") {
				_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中群聊 At：{message.RawMessage}", LogLevel.Debug);
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						GroupId = message.GroupId,
						Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
				};
				await MessageTools.SendSendMessage(sendMessage).ConfigureAwait(false);
			} else if (message.MessageType == "private" && message.SubType == "friend") {
				_logger.LogWithTime($"{message.UserId} 命中好友私聊：{message.RawMessage}", LogLevel.Debug);
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						UserId = message.UserId,
						Message = $"[CQ:reply,id={message.MessageId}]你好！"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
				};
				await MessageTools.SendSendMessage(sendMessage).ConfigureAwait(false);
			}
		} else if (message.PostType == "notice" && message.SubType == "poke" && message.TargetId == message.SelfId && message.UserId != message.SelfId) {
			_logger.LogWithTime($"{message.GroupId} {message.UserId} 命中戳一戳。", LogLevel.Debug);
			SendMessage sendMessage = new() {
				Action = "send_msg",
				Params = message.GroupId is not null ? new() {
					GroupId = message.GroupId,
					Message = $"[CQ:poke,qq={message.UserId}]"
				} : new() {
					UserId = message.UserId,
					Message = "你好！"
				},
				Echo = message.GroupId is not null
					? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{Random.Shared.NextString(16)}"
					: $"{DateTime.Now.Ticks}-{message.UserId}-{Random.Shared.NextString(16)}"
			};
			await MessageTools.SendSendMessage(sendMessage).ConfigureAwait(false);
		} else if (!string.IsNullOrWhiteSpace(message.Echo)) {
			EchoProcesser.Process(message);
		}
	}
}