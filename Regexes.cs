using System.Text.RegularExpressions;

namespace PersonalQQBotBackend.GeneratedRegexes;

public static partial class Regexes {
	[GeneratedRegex(@"查询阿梨(直播状态|最新稿件)", RegexOptions.Compiled)]
	public static partial Regex ALiQueryRegex();

	[GeneratedRegex(@"查询(?:(?:B站)?UP主?|B站(?:用户|主播|Uploader))的?(?<type>直播状态|最新稿件)(?:：|:) *(?<uid>\d{1,18})(?:$|\D)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	public static partial Regex BiliUploaderQueryRegex();
}