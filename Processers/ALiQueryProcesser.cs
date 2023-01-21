using Microsoft.Extensions.Caching.Memory;

namespace PersonalQQBotBackend.Processers;

public static class ALiQueryProcesser {
	private static readonly ILogger<Program> _logger = LoggerProvider.GetLogger<Program>();

	public static async Task ProcessAsync(ReceivedMessage message, System.Text.RegularExpressions.Match aLiQueryRegexMatch) {
		var echo = $"{DateTime.Now.Ticks}-{message.GroupId}-{message.UserId}-{message.MessageId}-{Random.Shared.NextString(16)}";
		if (message.UserId == 1138_7791_74 && message.RawMessage!.Contains("清除缓存")) { // 清除缓存
			CacheProvider.cache.Remove($"阿梨{aLiQueryRegexMatch.Groups[1].Value}");
			CacheProvider.cache.Remove($"更新阿梨{aLiQueryRegexMatch.Groups[1].Value}信息时间");
			var isGroup = message.MessageType == "group";
			var sendMessageText = $"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}]" : "")}已清除缓存。";
			await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
		} else if (aLiQueryRegexMatch.Groups[1].Value == "最新稿件") { // 最新稿件
			try {
				DateTime updateInfoTime;
				if (!CacheProvider.cache.TryGetValue<BiliApiUploaderVideosInfo>("阿梨最新稿件", out var info)) { // 读取缓存
					info = await BiliTools.GetBiliApiUploaderVideosInfoAsync(327263639).ConfigureAwait(false);
					updateInfoTime = DateTime.Now;
					_ = CacheProvider.cache.Set("阿梨最新稿件", info, TimeSpan.FromMinutes(10));
					_ = CacheProvider.cache.Set("更新阿梨最新稿件信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
				} else {
					updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨最新稿件信息时间");
				}
				if (info.Code == 0) {
					var videoInfo = info.Data?.List.VList[0];
					var isGroup = message.MessageType == "group";
					var sendMessageText = $"[CQ:reply,id={message.MessageId}]{(isGroup ? $" [CQ:at,qq={message.UserId}]" : "")}阿梨最新稿件：\n" +
						$"标题：{videoInfo?.Title}\n" +
						$"地址：https://www.bilibili.com/video/av{videoInfo?.Aid}\n" +
						$"发布时间：{DateTimeOffset.FromUnixTimeSeconds(videoInfo?.Created ?? 0).LocalDateTime:yyyy-M-d H:mm:ss}\n" +
						$"封面：[CQ:image,file={videoInfo?.Picture}]\n\n" +
						$"数据更新时间：{updateInfoTime:yyyy-M-d H:mm:ss}";
					await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
				} else {
					_logger.LogWithTime($"获取到的阿梨最新稿件信息包含错误：{info.Code} {info.Message}", LogLevel.Error);
					var isGroup = message.MessageType == "group";
					var sendMessageText = isGroup
						? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
						: $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。";
					await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
				}
			} catch (Exception e) {
				_logger.LogWithTime($"获取阿梨最新稿件信息时发生异常：\r\n{e}", LogLevel.Error);
				var isGroup = message.MessageType == "group";
				var sendMessageText = isGroup
					? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
					: $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。";
				await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
			}
		} else { // 直播状态
			try {
				DateTime updateInfoTime;
				if (!CacheProvider.cache.TryGetValue<BiliApiSpaceInfo>("阿梨直播状态", out var info)) { // 读取缓存
					info = await BiliTools.GetBiliApiUserSpaceInfoAsync(327263639).ConfigureAwait(false);
					updateInfoTime = DateTime.Now;
					_ = CacheProvider.cache.Set("阿梨直播状态", info, TimeSpan.FromMinutes(10));
					_ = CacheProvider.cache.Set("更新阿梨直播状态信息时间", updateInfoTime, TimeSpan.FromMinutes(10));
				} else {
					updateInfoTime = CacheProvider.cache.Get<DateTime>("更新阿梨直播状态信息时间");
				}
				if (info.Code == 0) {
					var isGroup = message.MessageType == "group";
					var sendMessageText = $"[CQ:reply,id={message.MessageId}]{(message.MessageType == "group" ? $" [CQ:at,qq={message.UserId}]\n" : "")}" +
						$"阿梨直播状态：{(info.Data?.LiveRoom.LiveStatus == 1 ? "正在直播" : "未在直播")}\n" +
						$"直播间标题：{info.Data?.LiveRoom.Title}\n" +
						$"直播间地址：https://live.bilibili.com/{info.Data?.LiveRoom.RoomId}\n\n" +
						$"数据更新时间：{updateInfoTime:yyyy-M-d H:mm:ss}";
					await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
				} else {
					_logger.LogWithTime($"获取到的阿梨直播状态信息包含错误：{info.Code} {info.Message}", LogLevel.Error);
					var isGroup = message.MessageType == "group";
					var sendMessageText = isGroup
						? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（数据异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
						: $"[CQ:reply,id={message.MessageId}]获取失败（数据异常），请联系 LC (1138779174) 处理。";
					await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
				}
			} catch (Exception e) {
				_logger.LogWithTime($"获取阿梨直播状态信息时发生异常：\r\n{e}", LogLevel.Error);
				var isGroup = message.MessageType == "group";
				var sendMessageText = isGroup
					? $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]获取失败（获取异常），请联系 [CQ:at,qq=1138779174](1138779174) 处理。"
					: $"[CQ:reply,id={message.MessageId}]获取失败（获取异常），请联系 LC (1138779174) 处理。";
				await MessageTools.SendTextMessageAsync(sendMessageText, isGroup, isGroup ? message.GroupId : message.UserId, echo).ConfigureAwait(false);
			}
		}
	}
}