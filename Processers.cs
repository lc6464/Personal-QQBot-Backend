using Microsoft.Extensions.Caching.Memory;

namespace PersonalQQBotBackend.Processer;

public static class Processers {
	//private static readonly ClientWebSocket ws = WebSocketProvider.WebSocket;

	public static readonly List<SendMessage> sendMessagesPool = new();

	public static async Task ProcessReceivedMessageMessageAsync(ReceivedMessage message) {
		if (message.PostType == "message") {
			var aLiQueryRegexMatch = Regexes.ALiQueryRegex().Match(message.RawMessage!); // 获取阿梨相关查询功能的 Match
			if (((message.MessageType == "group" && message.SubType != "anonymous") || (message.MessageType == "private" && message.SubType == "friend")) && aLiQueryRegexMatch.Success) {
				SendMessageParams sendMessageParams;
				string echo;
				
				if (message.UserId == 1138_7791_74 && message.RawMessage!.Contains("清除缓存")) { // 清除缓存
					CacheProvider.cache.Remove($"阿梨{aLiQueryRegexMatch.Groups[1].Value}");
					CacheProvider.cache.Remove($"更新阿梨{aLiQueryRegexMatch.Groups[1].Value}信息时间");
					sendMessageParams = message.MessageType == "group" ? new() {
						GroupId = message.GroupId,
						Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]已清除缓存。"
					} : new() {
						UserId = message.UserId,
						Message = $"[CQ:reply,id={message.MessageId}]已清除缓存。"
					};
					echo = message.MessageType == "group"
						? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
						: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
				} else if (aLiQueryRegexMatch.Groups[1].Value == "最新稿件") { // 最新稿件
					try {
						DateTime updateInfoTime;
						if (!CacheProvider.cache.TryGetValue<BiliApiUploaderVideosInfo>("阿梨最新稿件", out var info)) { // 读取缓存
							info = await Tools.GetBiliApiUploaderVideosInfoAsync(327263639).ConfigureAwait(false);
							updateInfoTime = DateTime.Now;
							CacheProvider.cache.Set("阿梨最新稿件", info, TimeSpan.FromMinutes(10));
							CacheProvider.cache.Set("更新阿梨最新稿件信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
						} else {
							updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨最新稿件信息时间");
						}
						if (info.Code == 0) {
							var videoInfo = info.Data?.List.VList[0];
							sendMessageParams = message.MessageType == "group" ? new() { GroupId = message.GroupId } : new() { UserId = message.UserId };
							sendMessageParams.Message = $"[CQ:reply,id={message.MessageId}]{(message.MessageType == "group" ? $" [CQ:at,qq={message.UserId}]" : "")}阿梨最新稿件：\n" +
								$"标题：{videoInfo?.Title}\n" +
								$"地址：https://www.bilibili.com/video/av{videoInfo?.Aid}\n" +
								$"发布时间：{DateTimeOffset.FromUnixTimeSeconds(videoInfo?.Created ?? 0).LocalDateTime:yyyy-M-d H:mm:ss}\n" +
								$"封面：[CQ:image,file={videoInfo?.Picture}]\n\n" +
								$"数据更新时间：{updateInfoTime:yyyy-M-d H:mm:ss}";
							echo = message.MessageType == "group"
								? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
						} else {
							Console.Error.WriteLine($"获取到的阿梨最新稿件信息包含错误：{info.Code} {info.Message}");
							sendMessageParams = message.MessageType == "group" ? new() {
								GroupId = message.GroupId,
								Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
							} : new() {
								UserId = message.UserId,
								Message = $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。"
							};
							echo = message.MessageType == "group"
								? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
						}
					} catch (Exception e) {
						Console.Error.WriteLine("获取阿梨最新稿件信息错误：");
						Console.Error.WriteLine(e);
						sendMessageParams = message.MessageType == "group" ? new() {
							GroupId = message.GroupId,
							Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
						} : new() {
							UserId = message.UserId,
							Message = $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。"
						};
						echo = message.MessageType == "group"
							? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
					}
				} else { // 直播状态
					try {
						DateTime updateInfoTime;
						if (!CacheProvider.cache.TryGetValue<BiliApiSpaceInfo>("阿梨直播状态", out var info)) { // 读取缓存
							info = await Tools.GetBiliApiUserSpaceInfoAsync(327263639).ConfigureAwait(false);
							updateInfoTime = DateTime.Now;
							CacheProvider.cache.Set("阿梨直播状态", info, TimeSpan.FromMinutes(10));
							CacheProvider.cache.Set("更新阿梨直播状态信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
						} else {
							updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨直播状态信息时间");
						}
						if (info.Code == 0) {
							sendMessageParams = message.MessageType == "group" ? new() { GroupId = message.GroupId } : new() { UserId = message.UserId };
							sendMessageParams.Message = $"[CQ:reply,id={message.MessageId}]{(message.MessageType == "group" ? $" [CQ:at,qq={message.UserId}]\n" : "")}" +
								$"阿梨直播状态：{(info.Data?.LiveRoom.LiveStatus == 1 ? "正在直播" : "未在直播")}\n" +
								$"直播间标题：{info.Data?.LiveRoom.Title}\n" +
								$"直播间地址：https://live.bilibili.com/{info.Data?.LiveRoom.RoomId}\n\n" +
								$"数据更新时间：{updateInfoTime:yyyy-M-d H:mm:ss}";
							echo = message.MessageType == "group"
								? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
						} else {
							Console.Error.WriteLine($"获取到的阿梨直播状态信息包含错误：{info.Code} {info.Message}");
							sendMessageParams = message.MessageType == "group" ? new() {
								GroupId = message.GroupId,
								Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
							} : new() {
								UserId = message.UserId,
								Message = $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。"
							};
							echo = message.MessageType == "group"
								? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
						}
					} catch (Exception e) {
						Console.Error.WriteLine("获取阿梨直播状态信息错误：");
						Console.Error.WriteLine(e);
						sendMessageParams = message.MessageType == "group" ? new() {
							GroupId = message.GroupId,
							Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
						} : new() {
							UserId = message.UserId,
							Message = $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。"
						};
						echo = message.MessageType == "group"
							? $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							: $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
					}
					
				}

				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = sendMessageParams,
					Echo = echo
				};
				lock (sendMessagesPool) {
					sendMessagesPool.Add(sendMessage);
				}
				await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage)).ConfigureAwait(false);
			} else if (message.MessageType == "group" && message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]") && message.SubType != "anonymous") {
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						GroupId = message.GroupId,
						Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
				};
				lock (sendMessagesPool) {
					sendMessagesPool.Add(sendMessage);
				}
				await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage)).ConfigureAwait(false);
			} else if (message.MessageType == "private" && message.SubType == "friend") {
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						UserId = message.UserId,
						Message = $"[CQ:reply,id={message.MessageId}]你好！"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
				};
				lock (sendMessagesPool) {
					sendMessagesPool.Add(sendMessage);
				}
				await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage)).ConfigureAwait(false);
			}
		} else if (message.PostType == "notice" && message.SubType == "poke" && message.TargetId == message.SelfId && message.UserId != message.SelfId) {
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
			lock (sendMessagesPool) {
				sendMessagesPool.Add(sendMessage);
			}
			await WebSocketProvider.SendTextAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage)).ConfigureAwait(false);
		} else if (!string.IsNullOrWhiteSpace(message.Echo)) {
			lock (sendMessagesPool) {
				sendMessagesPool.RemoveAll(x => x.Echo == message.Echo);
			}
			if (message.Status?.ToString() != "ok") {
				Console.Error.WriteLine($"发送消息失败：{message.Echo}");
			} else {
				Console.WriteLine($"发送消息成功：{message.Echo}，Message ID: {message.Data?.MessageId}");
			}
		}
	}
}