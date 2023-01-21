Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args).Build();
var config = Host.Services.GetRequiredService<IConfiguration>();
var programLogger = Host.Services.GetRequiredService<ILogger<Program>>();

programLogger.LogWithTime("欢迎使用 LC 的个人 QQ 机器人后端。");

var uriString = config.GetValue<string>("Connection:Uri");

if (uriString is null) {
	programLogger.LogWithTime("未找到配置文件中的 Connection:Uri 项。", LogLevel.Critical);
	Environment.Exit(2); // skipcq: CS-W1005
}

programLogger.LogWithTime("正在连接至 WebSocket 服务器。");

Uri connectionUri = new(uriString);

var ws = WebSocketProvider.WebSocket;

try {
	await ws.ConnectAsync(connectionUri, CancellationToken.None).ConfigureAwait(false);
	programLogger.LogWithTime("已连接至 WebSocket 服务器。");
} catch (Exception e) {
	programLogger.LogWithTime($"连接 WebSocket 服务器失败。\r\n{e}", LogLevel.Critical);
	Environment.Exit(1); // skipcq: CS-W1005
}

var isLogReceivedEvent = config.GetValue<bool>("Log:ReceivedEvent");
var tooLongMessage = false;


#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(async () => {
	while (true) {
		var buffer = new byte[524288]; // 512KB 缓冲区
		var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ConfigureAwait(false);

		if (!result.EndOfMessage) { // 直接丢弃过长的消息
			tooLongMessage = true;
			continue;
		} else if (tooLongMessage) {
			tooLongMessage = false;
			programLogger.LogWithTime("接收到超过 512KB 的数据，已丢弃。", LogLevel.Warning);
			continue;
		}

		if (result.MessageType == WebSocketMessageType.Close) { // 对方关闭连接
			programLogger.LogWithTime("连接已被关闭。", LogLevel.Critical);
			Environment.Exit(2); // skipcq: CS-W1005
		} else if (result.MessageType == WebSocketMessageType.Binary) { // 直接丢弃二进制数据
			programLogger.LogWithTime("接收到二进制数据，已丢弃。", LogLevel.Warning);
			continue;
		}

		try { // 尝试解析数据并处理消息
			var message = JsonSerializer.Deserialize<ReceivedMessage>(buffer.AsSpan(..result.Count));

			if (isLogReceivedEvent && message.MetaEventType != "heartbeat") {
				var messageString = Encoding.UTF8.GetString(buffer.AsSpan(..result.Count));
				programLogger.LogWithTime($"接收到事件：{messageString}", LogLevel.Debug);
			}

			_ = MainProcesser.ProcessReceivedMessageAsync(message); // 异步处理消息（不等待）
		} catch (Exception e) {
			var messageString = Encoding.UTF8.GetString(buffer.AsSpan(..result.Count));
			programLogger.LogWithTime($"无法解析接收到的数据：{messageString}\r\n{e}", LogLevel.Error);
		}
	}
});




System.Timers.Timer timer = new() { AutoReset = true, Interval = 1000 };
timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) => {
	lock (MainProcesser.sendMessageActionsPool) {
		foreach (var sendMessageAction in MainProcesser.sendMessageActionsPool) {
			if (sendMessageAction.CreatedTime.AddSeconds(sendMessageAction.TimeoutSecond) < DateTime.Now) {
				programLogger.LogWithTime($"发送消息 {sendMessageAction.Message.Echo} 超时，创建于 {sendMessageAction.CreatedTime:yyyy-MM-dd HH:mm:ss}，超时时长{sendMessageAction.TimeoutSecond}秒。", LogLevel.Warning);
				if (sendMessageAction.TimeoutCallback is not null) {
					programLogger.LogWithTime($"正在异步执行 {sendMessageAction.Message.Echo} 的超时回调。", LogLevel.Debug);
					_ = Task.Run(sendMessageAction.TimeoutCallback);
				}
				_ = MainProcesser.sendMessageActionsPool.Remove(sendMessageAction);
			}
		}
	}
};
timer.Start();




#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
await Host.RunAsync();



// skipcq: CS-W1061
public partial class Program {
	public static IHost? Host { get; private set; }
}