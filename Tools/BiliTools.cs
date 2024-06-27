namespace PersonalQQBotBackend.Tools;

public static class BiliTools {
	/* 由于 WebSocket 连接失败后会被释放，暂时取消重试
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
			await action().ConfigureAwait(false);
			return true;
		} catch {
			if (times < totalTimes) {
				Thread.Sleep(delay);
				Console.WriteLine($"正在重试，第{times + 1}次，共{totalTimes}次。");
				return await RetryAsync(action, delay, totalTimes, times + 1).ConfigureAwait(false);
			} else {
				return false;
			}
		}
	}
	*/




	public static async Task<BiliApiSpaceInfo> GetBiliApiUserSpaceInfoAsync(long uid) =>
		await HttpClientProvider.client.GetFromJsonAsync<BiliApiSpaceInfo>($"https://api.bilibili.com/x/space/wbi/acc/info?mid={uid}").ConfigureAwait(false);

	public static async Task<BiliApiUploaderVideosInfo> GetBiliApiUploaderVideosInfoAsync(long uid, int pageNumber = 1, int pageSize = 1) =>
		await HttpClientProvider.client.GetFromJsonAsync<BiliApiUploaderVideosInfo>($"https://api.bilibili.com/x/space/wbi/arc/search?mid={uid}&ps={pageSize}&pn={pageNumber}").ConfigureAwait(false);
}