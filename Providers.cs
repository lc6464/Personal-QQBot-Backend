using Microsoft.Extensions.Caching.Memory;

namespace PersonalQQBotBackend;

public static class HttpClientProvider {
	public static readonly HttpClient client = new() {
		Timeout = TimeSpan.FromSeconds(5),
		DefaultRequestHeaders = {
			{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.52" },
			{ "Accept", "application/json" },
			{ "Accept-Language", "zh-CN" }
		}
	};
}

public static class CacheProvider {
	public static readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

	public static T Set<T>(object key, T value, TimeSpan? slidingExpiration = null) {
		if (slidingExpiration is null) {
			return cache.Set(key, value);
		}

		using var entry = cache.CreateEntry(key);
		entry.SlidingExpiration = slidingExpiration;
		entry.Value = value;

		return value;
	}
}


public static class WebSocketProvider {
	public static ClientWebSocket WebSocket { get; private set; } = new();

	public static void InitializeWebSocket() {
		var accessToken = GetAccessToken();
		if (accessToken is not null) {
			WebSocket.Options.SetRequestHeader("Authorization", accessToken);
		}
	}

	public static void RenewWebSocket() {
		WebSocket = new();
		InitializeWebSocket();
	}

	public static string? GetAccessToken() => Program.Host?.Services.GetRequiredService<IConfiguration>().GetValue<string?>("Connection:AccessToken");

	public static async Task SendTextAsync(ArraySegment<byte> buffer, bool endOfMessage = true) =>
		await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None).ConfigureAwait(false);

	public static async ValueTask SendTextAsync(ReadOnlyMemory<byte> buffer, bool endOfMessage = true) =>
		await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage, CancellationToken.None).ConfigureAwait(false);
}


public static class LoggerProvider {
	public static ILogger<Program>? ProgramLogger { get; set; }
	
	public static ILogger<T> GetLogger<T>() =>
		Program.Host!.Services.GetRequiredService<ILogger<T>>();
}