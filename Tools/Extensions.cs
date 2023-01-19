namespace PersonalQQBotBackend.Tools;

public static class Extensions {
	public static string NextString(this Random random, int length) {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		return new(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static string NextString(this Random random, int length, string chars) =>
		new(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());


	public static void LogWithTime(this ILogger logger, object message, LogLevel logLevel = LogLevel.Information) =>
		logger.Log(logLevel, "[{}] {}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);

	public static void LogWithTime<T>(this ILogger<T> logger, object message, LogLevel logLevel = LogLevel.Information) =>
		logger.Log(logLevel, "[{}] {}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
}