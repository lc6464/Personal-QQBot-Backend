namespace PersonalQQBotBackend;

public class MyHttpMessageInvoker : HttpMessageInvoker {
	public MyHttpMessageInvoker() : base(new SocketsHttpHandler()) { }

	public MyHttpMessageInvoker(HttpMessageHandler handler) : base(handler) { }

	public MyHttpMessageInvoker(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler) { }

	public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) {
		var accessToken = WebSocketProvider.GetAccessToken();
		if (accessToken is not null) {
			request.Headers.Authorization = new("Bearer", accessToken);
		}

		return base.Send(request, cancellationToken);
	}

	public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
		var accessToken = WebSocketProvider.GetAccessToken();
		if (accessToken is not null) {
			request.Headers.Authorization = new("Bearer", accessToken);
		}

		return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
	}
}