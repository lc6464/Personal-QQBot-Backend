using System.Text.RegularExpressions;

namespace PersonalQQBotBackend.GeneratedRegexes;

public static partial class Regexes {
	[GeneratedRegex(@"查询阿梨的?(直播状态|最新稿件)")]
	public static partial Regex ALiQueryRegex();

	[GeneratedRegex(@"查询(?:(?:B站)?UP主?|B站(?:用户|主播|Uploader))的?(?<type>直播状态|最新稿件) *[:：]? *(?<uid>\d{1,18})(?:$|\D)", RegexOptions.IgnoreCase)]
	public static partial Regex BiliUploaderQueryRegex();

	[GeneratedRegex(@"(?:解析|查询|识别)(?:QQ)?表情 *[:：]? *\[CQ:face,id=(\d+)\]")]
	public static partial Regex ParseQQFaceRegex();

	[GeneratedRegex(@"设置(?:QQ)?(?:在线)?机型 *[:：]? *(.*)")]
	public static partial Regex SetQQOnlineModelRegex();
}