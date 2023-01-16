using Microsoft.Extensions.Caching.Memory;

namespace PersonalQQBotBackend;

public static class Processer {
	public static readonly List<SendMessage> sendMessagesPool = new();

	public static async Task ProcessReceivedMessageMessageAsync(ClientWebSocket ws, ReceivedMessage message) {
		if (message.PostType == "message") {
			if (((message.MessageType == "group" && message.SubType != "anonymous") || (message.MessageType == "private" && message.SubType == "friend")) && message.RawMessage!.Contains("查询阿梨")) {
				if (message.MessageType == "group" && message.SubType != "anonymous") {
					SendMessage? sendMessage = null;
					if (message.RawMessage.Contains("查询阿梨最新稿件")) {
						if (message.UserId == 1138_7791_74 && message.RawMessage.Contains("清除缓存")) {
							CacheProvider.cache.Remove("阿梨最新稿件");
							CacheProvider.cache.Remove("更新阿梨最新稿件信息时间");
							sendMessage = new() {
								Action = "send_msg",
								Params = new SendMessageParams {
									GroupId = message.GroupId,
									Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]已清除缓存。"
								},
								Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							};
						} else {
							try {
								DateTime updateInfoTime;
								if (!CacheProvider.cache.TryGetValue<BiliApiUploaderVideosInfo>("阿梨最新稿件", out var info)) {
									info = await Tools.GetBiliApiUploaderVideosInfoAsync(327263639).ConfigureAwait(false);
									updateInfoTime = DateTime.Now;
									CacheProvider.cache.Set("阿梨最新稿件", info, TimeSpan.FromMinutes(10));
									CacheProvider.cache.Set("更新阿梨最新稿件信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
								} else {
									updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨最新稿件信息时间");
								}
								if (info.Code == 0) {
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											GroupId = message.GroupId,
											Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]阿梨最新稿件：\n" +
											$"标题：{info.Data?.List.VList[0].Title}\n" +
											$"链接：https://www.bilibili.com/video/{info.Data?.List.VList[0].Aid}\n" +
											$"发布时间：{DateTimeOffset.FromUnixTimeSeconds(info.Data?.List.VList[0].Created ?? 0):yyyy-M-d H:mm:ss}\n\n" +
											$"数据更新时间：{updateInfoTime}"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								} else {
									Console.Error.WriteLine($"获取到的阿梨最新稿件信息包含错误：{info.Code} {info.Message}");
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											GroupId = message.GroupId,
											Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								}
							} catch (Exception e) {
								Console.Error.WriteLine("获取阿梨最新稿件信息错误：");
								Console.Error.WriteLine(e);
								sendMessage = new() {
									Action = "send_msg",
									Params = new SendMessageParams {
										GroupId = message.GroupId,
										Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
									},
									Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								};
							}
						}
					} else if (message.RawMessage.Contains("查询阿梨直播状态")) {
						if (message.UserId == 1138_7791_74 && message.RawMessage.Contains("清除缓存")) {
							CacheProvider.cache.Remove("阿梨直播状态");
							CacheProvider.cache.Remove("更新阿梨直播状态信息时间");
							sendMessage = new() {
								Action = "send_msg",
								Params = new SendMessageParams {
									GroupId = message.GroupId,
									Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]已清除缓存。"
								},
								Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							};
						} else {
							try {
								DateTime updateInfoTime;
								if (!CacheProvider.cache.TryGetValue<BiliApiSpaceInfo>("阿梨直播状态", out var info)) {
									info = await Tools.GetBiliApiUserSpaceInfoAsync(327263639).ConfigureAwait(false);
									updateInfoTime = DateTime.Now;
									CacheProvider.cache.Set("阿梨直播状态", info, TimeSpan.FromMinutes(10));
									CacheProvider.cache.Set("更新阿梨直播状态信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
								} else {
									updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨直播状态信息时间");
								}
								if (info.Code == 0) {
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											GroupId = message.GroupId,
											Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]\n" +
											$"阿梨直播状态：{(info.Data?.LiveRoom.LiveStatus == 1 ? "正在直播" : "未在直播")}\n" +
											$"直播间标题：{info.Data?.LiveRoom.Title}\n" +
											$"直播间链接：https://live.bilibili.com/{info.Data?.LiveRoom.RoomId}\n\n" +
											$"数据更新时间：{updateInfoTime}"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								} else {
									Console.Error.WriteLine($"获取到的阿梨直播状态信息包含错误：{info.Code} {info.Message}");
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											GroupId = message.GroupId,
											Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								}
							} catch (Exception e) {
								Console.Error.WriteLine("获取阿梨直播状态信息错误：");
								Console.Error.WriteLine(e);
								sendMessage = new() {
									Action = "send_msg",
									Params = new SendMessageParams {
										GroupId = message.GroupId,
										Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
									},
									Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								};
							}
						}
					}

					if (sendMessage is not null) {
						lock (sendMessagesPool) {
							sendMessagesPool.Add(sendMessage!.Value);
						}
						await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
					}
				} else if (message.MessageType == "private" && message.SubType == "friend") {
					SendMessage? sendMessage = null;
					if (message.RawMessage.Contains("查询阿梨最新稿件")) {
						if (message.UserId == 1138_7791_74 && message.RawMessage.Contains("清除缓存")) {
							CacheProvider.cache.Remove("阿梨最新稿件");
							CacheProvider.cache.Remove("更新阿梨最新稿件信息时间");
							sendMessage = new() {
								Action = "send_msg",
								Params = new SendMessageParams {
									UserId = message.UserId,
									Message = $"[CQ:reply,id={message.MessageId}]已清除缓存。"
								},
								Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							};
						} else {
							try {
								DateTime updateInfoTime;
								if (!CacheProvider.cache.TryGetValue<BiliApiUploaderVideosInfo>("阿梨最新稿件", out var info)) {
									info = await Tools.GetBiliApiUploaderVideosInfoAsync(327263639).ConfigureAwait(false);
									updateInfoTime = DateTime.Now;
									CacheProvider.cache.Set("阿梨最新稿件", info, TimeSpan.FromMinutes(10));
									CacheProvider.cache.Set("更新阿梨最新稿件信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
								} else {
									updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨最新稿件信息时间");
								}
								if (info.Code == 0) {
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											UserId = message.UserId,
											Message = $"[CQ:reply,id={message.MessageId}]阿梨最新稿件：\n" +
											$"标题：{info.Data?.List.VList[0].Title}\n" +
											$"链接：https://www.bilibili.com/video/av{info.Data?.List.VList[0].Aid}\n" +
											$"发布时间：{DateTimeOffset.FromUnixTimeSeconds(info.Data?.List.VList[0].Created ?? 0):yyyy-M-d H:mm:ss}\n\n" +
											$"数据更新时间：{updateInfoTime}"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								} else {
									Console.Error.WriteLine($"获取到的阿梨最新稿件信息包含错误：{info.Code} {info.Message}");
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											UserId = message.UserId,
											Message = $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								}
							} catch (Exception e) {
								Console.Error.WriteLine("获取阿梨最新稿件信息错误：");
								Console.Error.WriteLine(e);
								sendMessage = new() {
									Action = "send_msg",
									Params = new SendMessageParams {
										UserId = message.UserId,
										Message = $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。"
									},
									Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								};
							}
						}
					} else if (message.RawMessage.Contains("查询阿梨直播状态")) {
						if (message.UserId == 1138_7791_74 && message.RawMessage.Contains("清除缓存")) {
							CacheProvider.cache.Remove("阿梨直播状态");
							CacheProvider.cache.Remove("更新阿梨直播状态信息时间");
							sendMessage = new() {
								Action = "send_msg",
								Params = new SendMessageParams {
									UserId = message.UserId,
									Message = $"[CQ:reply,id={message.MessageId}]已清除缓存。"
								},
								Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
							};
						} else {
							try {
								DateTime updateInfoTime;
								if (!CacheProvider.cache.TryGetValue<BiliApiSpaceInfo>("阿梨直播状态", out var info)) {
									info = await Tools.GetBiliApiUserSpaceInfoAsync(327263639).ConfigureAwait(false);
									updateInfoTime = DateTime.Now;
									CacheProvider.cache.Set("阿梨直播状态", info, TimeSpan.FromMinutes(10));
									CacheProvider.cache.Set("更新阿梨直播状态信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
								} else {
									updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨直播状态信息时间");
								}
								if (info.Code == 0) {
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											UserId = message.UserId,
											Message = $"[CQ:reply,id={message.MessageId}]\n" +
											$"阿梨直播状态：{(info.Data?.LiveRoom.LiveStatus == 1 ? "正在直播" : "未在直播")}\n" +
											$"直播间标题：{info.Data?.LiveRoom.Title}\n" +
											$"直播间链接：https://live.bilibili.com/{info.Data?.LiveRoom.RoomId}\n\n" +
											$"数据更新时间：{updateInfoTime}"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								} else {
									Console.Error.WriteLine($"获取到的阿梨直播状态信息包含错误：{info.Code} {info.Message}");
									sendMessage = new() {
										Action = "send_msg",
										Params = new SendMessageParams {
											UserId = message.UserId,
											Message = $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。"
										},
										Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
									};
								}
							} catch (Exception e) {
								Console.Error.WriteLine("获取阿梨直播状态信息错误：");
								Console.Error.WriteLine(e);
								sendMessage = new() {
									Action = "send_msg",
									Params = new SendMessageParams {
										UserId = message.UserId,
										Message = $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。"
									},
									Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}"
								};
							}
						}
					}

					if (sendMessage is not null) {
						lock (sendMessagesPool) {
							sendMessagesPool.Add(sendMessage!.Value);
						}
						await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
					}
				}
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
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
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
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
			}
		} else if (message.PostType == "notice" && message.SubType == "poke" && message.TargetId == message.SelfId && message.UserId != message.SelfId) {
			if (message.GroupId is not null) {
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						GroupId = message.GroupId,
						Message = $"[CQ:poke,qq={message.UserId}]"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{Random.Shared.NextString(16)}"
				};
				lock (sendMessagesPool) {
					sendMessagesPool.Add(sendMessage);
				}
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
			} else {
				SendMessage sendMessage = new() {
					Action = "send_msg",
					Params = new() {
						UserId = message.UserId,
						Message = "你好！"
					},
					Echo = $"{DateTime.Now.Ticks}-{message.UserId}-{Random.Shared.NextString(16)}"
				};
				lock (sendMessagesPool) {
					sendMessagesPool.Add(sendMessage);
				}
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
			}
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