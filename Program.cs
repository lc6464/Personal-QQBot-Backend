﻿using var host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

Console.WriteLine("欢迎使用 LC 的个人 QQ 机器人后端。");

var uriString = config.GetValue<string>("Connection:Uri");

if (uriString is null) {
	Console.Error.WriteLine("未找到配置文件中的 Connection:Uri 项。");
	Environment.Exit(2); // skipcq: CS-W1005
}

Console.WriteLine("正在连接至 WebSocket 服务器。");

Uri connectionUri = new(uriString);

var ws = WebSocketProvider.WebSocket;

try {
	await ws.ConnectAsync(connectionUri, CancellationToken.None).ConfigureAwait(false);
	Console.WriteLine("已连接至 WebSocket 服务器。");
} catch (Exception e) {
	Console.Error.WriteLine("连接 WebSocket 服务器失败。");
	Console.Error.WriteLine(e);
	Environment.Exit(1); // skipcq: CS-W1005
}

var isLogGeneralEvent = config.GetValue<bool>("Log:GeneralEvent");


#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(async () => {
	while (true) {
		var buffer = new byte[524288]; // 512KB 缓冲区
		var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ConfigureAwait(false);
		if (result.MessageType == WebSocketMessageType.Close) {
			Console.Error.WriteLine("连接已关闭。");

			Environment.Exit(2); // skipcq: CS-W1005
		}


		if (isLogGeneralEvent) {
			var messageString = Encoding.UTF8.GetString(buffer.AsSpan(..result.Count));
			if (!messageString.Contains("\"post_type\":\"meta_event\",\"meta_event_type\":\"heartbeat\"")) {
				Console.WriteLine($"接收到事件：{messageString}");
			}
		}

		var message = JsonSerializer.Deserialize<ReceivedMessage>(buffer.AsSpan(..result.Count));

		await Processers.ProcessReceivedMessageMessageAsync(message).ConfigureAwait(false);
	}
});




System.Timers.Timer timer = new() { AutoReset = true, Interval = 10000 };
timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) => {
	var count = 0;
	lock (Processers.sendMessagesPool) {
		count = Processers.sendMessagesPool.Count;
	}
	if (count != 0) {
		Console.WriteLine($"消息池剩余消息数：{count}");
		Thread.Sleep(200);
		lock (Processers.sendMessagesPool) {
			count = Processers.sendMessagesPool.Count;
		}
		if (count != 0) {
			Console.WriteLine($"消息池剩余消息数：{count}，其中的一些消息很可能没有被处理！");
		}
	}
};
timer.Start();




#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
await host.RunAsync();