using System.Text.Json;
using Struct;

using var host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

Console.WriteLine("欢迎使用 LC 的个人 QQ 机器人后端。");

var uriString = config.GetValue<string>("Connection:Uri");

if (uriString is null) {
	Console.Error.WriteLine("未找到配置文件中的 Connection:Uri 项。");
	return;
}

Console.WriteLine("正在连接至 WebSocket 服务器。");

Uri connectionUri = new(uriString);

ClientWebSocket ws = new();

//if (!await Tools.RetryAsync(async () => await ws.ConnectAsync(connectionUri, CancellationToken.None), TimeSpan.FromMilliseconds(config.GetValue<int>("Connection:Retry:Delay")), config.GetValue<int>("Connection:Retry:MaxRetries"))) {
try {
	await ws.ConnectAsync(connectionUri, CancellationToken.None);
} catch {
	Console.Error.WriteLine("连接 WebSocket 服务器失败。");
	return;
}


await Task.Run(async () => {
	while (true) {
		var buffer = new byte[1048576];
		var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
		if (result.MessageType == WebSocketMessageType.Close) {
			Console.WriteLine("连接已关闭。");
			/*if (config.GetValue<bool>("Connection:AutoReconnect")) {
				Console.WriteLine("正在尝试重新连接……");
				if (await Tools.RetryAsync(async () => await ws.ConnectAsync(connectionUri, CancellationToken.None), TimeSpan.FromMilliseconds(config.GetValue<int>("Connection:Retry:Delay")), config.GetValue<int>("Connection:Retry:MaxRetries"))) {
					Console.WriteLine("连接成功。");
					continue;
				} else {
					Console.Error.WriteLine("连接 WebSocket 服务器失败。");
				}
			}*/
			return;
		}


		var message = JsonSerializer.Deserialize<ReceivedMessage>(buffer.AsSpan(..result.Count));
		Console.WriteLine($"接收到事件：{Encoding.UTF8.GetString(buffer.AsSpan(..result.Count))}");

		if (message.PostType == "message") {
			if (message.MessageType == "group" && message.RawMessage!.Contains($"[CQ:at,qq={message.SelfId}]")) {
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SendMessage() { Action = "send_msg", Params = new() { GroupId = message.GroupId, Message = $"[CQ:reply,id={message.MessageId}] [CQ:at,qq={message.UserId}]你好！" } }), WebSocketMessageType.Text, true, CancellationToken.None);
			} else if (message.MessageType == "private") {
				await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new SendMessage() { Action = "send_msg", Params = new() { UserId = message.UserId, Message = $"[CQ:reply,id={message.MessageId}]你好！" } }), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

	}
});



/* 由于 WebSocket 连接失败后会被释放，暂时取消重试
namespace Tool {
	public static class Tools {
		public static bool Retry(Action action, TimeSpan delay, int totalTimes, int times = 0) {
			try {
				action();
				return true;
			} catch {
				if (times < totalTimes) {
					Thread.Sleep(delay);
					Console.WriteLine($"正在重试，第{times + 1}次，共{totalTimes}次。");
					return Retry(action, delay, totalTimes, times + 1);
				} else {
					return false;
				}
			}
		}

		public static async Task<bool> RetryAsync(Func<Task> action, TimeSpan delay, int totalTimes, int times = 0) {
			try {
				await action();
				return true;
			} catch {
				if (times < totalTimes) {
					Thread.Sleep(delay);
					Console.WriteLine($"正在重试，第{times + 1}次，共{totalTimes}次。");
					return await RetryAsync(action, delay, totalTimes, times + 1);
				} else {
					return false;
				}
			}
		}
	}
}
*/

namespace Struct {
	public struct ReceivedMessage {
		[JsonPropertyName("post_type")]
		public string PostType { get; set; }

		[JsonPropertyName("message_type")]
		public string? MessageType { get; set; }

		[JsonPropertyName("raw_message")]
		public string? RawMessage { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }

		[JsonPropertyName("message_id")]
		public long? MessageId { get; set; }

		[JsonPropertyName("group_id")]
		public long? GroupId { get; set; }

		[JsonPropertyName("user_id")]
		public long? UserId { get; set; }

		[JsonPropertyName("self_id")]
		public long? SelfId { get; set; }
	}


	public struct SendMessage {
		[JsonPropertyName("action")]
		public string Action { get; set; }

		[JsonPropertyName("params")]
		public SendMessageParams? Params { get; set; }

		[JsonPropertyName("echo")]
		public string? Echo { get; set; }
	}


	public struct SendMessageParams {
		[JsonPropertyName("group_id")]
		public long? GroupId { get; set; }

		[JsonPropertyName("user_id")]
		public long? UserId { get; set; }

		[JsonPropertyName("message_id")]
		public long? MessageId { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }
	}
}