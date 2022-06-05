Imports System.Net.Http
Imports Windows.Storage
Imports System.Text.RegularExpressions
Imports System.Net.WebUtility
Imports AngleSharp.Html.Dom
Imports Windows.ApplicationModel.Background

Public Module 抗原提交核心
	ReadOnly HTTP客户端 As New HttpClient,
		数据目录 As StorageFolder = ApplicationData.Current.LocalFolder,
		JS时间起点 As Date = Date.SpecifyKind(New Date(1970, 1, 1, 0, 0, 0), DateTimeKind.Utc),
		随机字符集 As Char() = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678",
		变量正则 As New Regex("var (\w+) ?= ?(['""](.+?)['""]|(\d+));"),
		HTML解析器 As New AngleSharp.Html.Parser.HtmlParser,
		调度间隔 As New TimeSpan(0, 15, 0),
		任务生成器 As New BackgroundTaskBuilder With {.Name = "上科大抗原自动交"},
		一天 As New TimeSpan(1, 0, 0, 0)

	Private Function 取62基数子串(基数 As ULong, 字符数组 As Char()) As Text.StringBuilder
		If 基数 < 62 Then
			Return New Text.StringBuilder(字符数组(基数))
		Else
			Return 取62基数子串(Math.Floor(基数 / 62), 字符数组).Append(字符数组(基数 Mod 62))
		End If
	End Function

	Private Async Function 交抗原核心() As Task(Of String)
		Const 基地址 = "http://wj.shanghaitech.edu.cn",
			请求URI = 基地址 & "/user/qlist.aspx?sysid=159110074",
			ktimes = 31,
			b = ktimes Mod 10, '如果b=0，则应强行指定b=1
			字典 = "kgESOLJUbB2fCteoQdYmXvF8j9IZs3K0i6w75VcDnG14WAyaxNqPuRlpTHMrhz"
		For Each 任务 In BackgroundTaskRegistration.AllTasks.Values
			If 任务.Name = "上科大抗原自动交" Then
				任务.Unregister(False)
			End If
		Next
		Dim 预约数据文件 = Await 数据目录.GetFileAsync("预约数据.bin"), 读入器 As New BinaryReader(Await 预约数据文件.OpenStreamForReadAsync),
			学工号 = 读入器.ReadUInt32,
			密码 = 读入器.ReadString,
			自动任务 = 读入器.ReadBoolean,
			每日重试 = 读入器.ReadBoolean,
			成功喵提醒 = 读入器.ReadBoolean,
			失败喵提醒 = 读入器.ReadBoolean,
			喵提醒地址 = $"http://miaotixing.com/trigger?id={读入器.ReadString}&text=",
			HTTP响应 As HttpResponseMessage
		Try
			HTTP响应 = Await HTTP客户端.GetAsync(请求URI)
		Catch ex As HttpRequestException
		End Try
		Dim ulQs As IHtmlDivElement, 网络通畅 = HTTP响应 IsNot Nothing
		If 网络通畅 Then
			Dim HTML文档 = HTML解析器.ParseDocument(Await HTTP响应.Content.ReadAsStreamAsync),
			跳转URI = HTTP响应.RequestMessage.RequestUri
			If 跳转URI.AbsoluteUri <> 请求URI Then
				HTML文档 = HTML解析器.ParseDocument(Await (Await HTTP客户端.PostAsync(跳转URI, New FormUrlEncodedContent(New Dictionary(Of String, String) From {{"__EVENTTARGET", "btnSubmit"}, {"__VIEWSTATE", DirectCast(HTML文档.GetElementById("__VIEWSTATE"), IHtmlInputElement).Value}, {"__VIEWSTATEGENERATOR", DirectCast(HTML文档.GetElementById("__VIEWSTATEGENERATOR"), IHtmlInputElement).Value}, {"__EVENTVALIDATION", DirectCast(HTML文档.GetElementById("__EVENTVALIDATION"), IHtmlInputElement).Value}, {"hfQuery", $"10000|{学工号}〒30000|{密码}"}, {"hfPwd", "2"}, {"txtVerifyCode", "1"}}))).Content.ReadAsStreamAsync)
			End If
			ulQs = HTML文档.GetElementById("ulQs")
			If ulQs Is Nothing Then
				If 失败喵提醒 Then
					Call HTTP客户端.GetAsync(喵提醒地址 & "学工号或密码错误")
				End If
				读入器.Close()
				Return "学工号或密码错误"
			End If
		End If
		Dim 预约表 As New List(Of 简单预约表行),
			行 As 简单预约表行,
			现在 = Date.Now,
			列表起始位置 = 读入器.BaseStream.Position
		For a = 1 To 读入器.ReadInt32
			行 = New 简单预约表行 With {.预约时间 = New Date(读入器.ReadInt64), .编号 = 读入器.ReadString, .文件 = 读入器.ReadString}
			If 行.预约时间 > 现在 - 调度间隔 Then
				预约表.Add(行)
			End If
		Next
		读入器.Close()
		If Not 预约表.Any Then
			If 失败喵提醒 Then
				Call HTTP客户端.GetAsync(喵提醒地址 & "没有可以提交的预约")
			End If
			Return "没有可以提交的预约"
		End If
		预约表.Sort(Function(行1 As 简单预约表行, 行2 As 简单预约表行) (行1.预约时间 - 行2.预约时间).TotalSeconds)
		行 = 预约表.First
		Dim 下次调度 As Date = 现在 + 调度间隔,
			提交结果 = 行提交结果.无可提交表单
		If 行.预约时间 < 下次调度 Then
			If 网络通畅 Then
				For Each 调查项目 In ulQs.Children
					Dim URL = 调查项目.GetElementsByTagName("a").Single.GetAttribute("href"),
				网页源码 = Await HTTP客户端.GetStringAsync(基地址 & URL),
				变量字典 As New Dictionary(Of String, String)((From 匹配 In 变量正则.Matches(网页源码) Let 组 = 匹配.Groups Select New KeyValuePair(Of String, String)(组(1).Value, If(组(3).Value = "", 组(4).Value, 组(3).Value))).Distinct(变量去重.唯一实例)),
				activityId = 变量字典("activityId") Xor 2130030173,
				版本 = URL.Split("/")(1)
					Dim HTML文档 = HTML解析器.ParseDocument(网页源码),
					key As String, token As String, starttime As String, 上传URL As String, 提交URL As String
					Select Case 版本
						Case "vj"
							Dim 上传框架 = HTML文档.GetElementById("uploadFrame4")
							If 上传框架 Is Nothing Then
								'此问卷未到填写时间
								Continue For
							End If
							Dim 上传变量 As New Dictionary(Of String, String)((From 匹配 In 变量正则.Matches(Await HTTP客户端.GetStringAsync(基地址 & 上传框架.GetAttribute("src"))) Let 组 = 匹配.Groups Select New KeyValuePair(Of String, String)(组(1).Value, If(组(3).Value = "", 组(4).Value, 组(3).Value))).Distinct(变量去重.唯一实例))
							key = 上传变量("dir")
							token = 上传变量("token")
							starttime = 变量字典("starttime")
							上传URL = "https://wjx-z0.qiniup.com/"
							提交URL = "http://wj.shanghaitech.edu.cn/joinnew/processjq.ashx?shortid=" & 变量字典("shortAid")
						Case "vm"
							提交URL = DirectCast(HTML文档.GetElementById("form1"), IHtmlFormElement).Action
							If 提交URL = "" Then
								'此问卷未到填写时间
								Continue For
							End If
							Dim TokenKey = (Await HTTP客户端.GetStringAsync($"{基地址}/joinnew/GetQiniuToken.ashx?ms=8192&q=4&activity={activityId}")).Split("〒")
							token = TokenKey(0)
							key = TokenKey(1)
							上传URL = "https://upload.qiniup.com/"
							starttime = DirectCast(HTML文档.GetElementById("starttime"), IHtmlInputElement).Value
					End Select
					Dim 照片文件 = Await AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(行.文件),
				文件流 = Await 照片文件.OpenStreamForReadAsync,
				文件名 = 照片文件.Name
					key &= CUInt((Date.Now - JS时间起点).TotalSeconds) & MathNet.Numerics.Combinatorics.SelectVariation(随机字符集, 6).ToArray & Path.GetExtension(文件名)
					Call HTTP客户端.PostAsync(上传URL, New MultipartFormDataContent From {{New StringContent(文件名), "name"}, {New StringContent(key), "key"}, {New StringContent(token), "token"}, {New StreamContent(文件流), "file", 文件名}})
					Dim 数字匹配 As MatchCollection = Regex.Matches(starttime, "\d+"),
				数值1 As UInteger = (TimeZoneInfo.ConvertTimeToUtc(Date.SpecifyKind(New Date(数字匹配(0).Value, 数字匹配(1).Value, 数字匹配(2).Value, 数字匹配(3).Value, 数字匹配(4).Value, 数字匹配(5).Value), DateTimeKind.Local)) - JS时间起点).TotalSeconds, 字符串 As String = 数值1
					If 数值1 Mod 10 Then
						字符串 = 字符串.Reverse.ToArray
					End If
					字符串 &= "89123"
					Dim 字符数组 As Char() = 字典,
				rndnum = 变量字典("rndnum"),
				数值2 = rndnum.Split(".")(0) Xor &H36E455,
				字典上界 = 字符数组.Length - 1
					For Each 位置字符 In (字符串 & 数值2).ToCharArray
						数值1 = UInteger.Parse(位置字符)
						Dim 字符 = 字符数组(数值1)
						字符数组(数值1) = 字符数组(字典上界 - 数值1)
						字符数组(字典上界 - 数值1) = 字符
					Next
					'activityId = 164395883 '仅测试用
					字符数组 = 取62基数子串(CULng(字符串) + 数值2 + activityId, 字符数组).ToString
					数值1 = (From 字 In 字符数组 Select AscW(字)).Sum Mod 字符数组.Length
					Dim jqnonce = 变量字典("jqnonce"),
				relrealname = 变量字典("relrealname"),
				relusername = 变量字典("relusername"),
				reldept = 变量字典("reldept")
					If (Await (Await HTTP客户端.PostAsync($"{提交URL}&starttime={UrlEncode(starttime).Replace("+", "%20")}&hmt=1&mst={变量字典("maxSurveyTime")}&vpsiu=1&submittype=1&ktimes={ktimes}&hlv=1&rn={rndnum}&jqpram={CStr(字符数组.Skip(数值1).Concat(字符数组.Take(数值1)).ToArray)}&jcn={UrlEncode((From 字 In relrealname.AsEnumerable Select ChrW(AscW(字) Xor b)).ToArray)}&relts={变量字典("relts")}&relusername={relusername}&relsign={变量字典("relsign")}&relrealname={UrlEncode(relrealname)}&reldept={UrlEncode(reldept)}&t={CULng((TimeZoneInfo.ConvertTimeToUtc(Date.Now) - JS时间起点).TotalMilliseconds)}&jqnonce={jqnonce}&jqsign={UrlEncode((From 字 In jqnonce.AsEnumerable Select ChrW(AscW(字) Xor b)).ToArray)}", New FormUrlEncodedContent(New Dictionary(Of String, String) From {{"submitdata", $"1$1!{relrealname}^2!{relusername}^3!{reldept.Replace("/", "-")}^4!}}2$1}}3$1}}4${key}%2C{文件流.Length}%2C{文件名}}}5${行.编号}"}}))).Content.ReadAsStringAsync).StartsWith("10〒/wjx/join/complete.aspx?") Then
						If 成功喵提醒 Then
							Call HTTP客户端.GetAsync(喵提醒地址 & "提交成功")
						End If
						提交结果 = 行提交结果.提交成功
					Else
						If 失败喵提醒 Then
							Call HTTP客户端.GetAsync(喵提醒地址 & "提交失败")
						End If
						提交结果 = 行提交结果.提交失败
					End If
				Next
			Else
				提交结果 = 行提交结果.网络不通
			End If
		Else
			提交结果 = 行提交结果.时间未到
		End If
		If 自动任务 Then
			If 每日重试 AndAlso 提交结果 <> 行提交结果.提交成功 AndAlso 提交结果 <> 行提交结果.时间未到 Then
				行.预约时间 += 一天
				预约表.RemoveAt(0)
				预约表.Add(行)
			End If
			Dim 待调度列表 = 预约表.SkipWhile(Function(预约 As 简单预约表行) 预约.预约时间 < 下次调度)
			自动任务 = 待调度列表.Any
			If 自动任务 Then
				下次调度 = 待调度列表.First.预约时间
				任务生成器.SetTrigger(New TimeTrigger((下次调度 - 现在).TotalMinutes, True))
				任务生成器.Register()
			End If
			Dim 写出器 As New BinaryWriter(Await 预约数据文件.OpenStreamForWriteAsync)
			写出器.BaseStream.Position = 列表起始位置
			写出器.Write(待调度列表.Count)
			For Each 写出行 In 待调度列表
				写出器.Write(写出行.预约时间.Ticks)
				写出器.Write(写出行.编号)
				写出器.Write(写出行.文件)
			Next
			写出器.Close()
		End If
		If 自动任务 Then
			Return $"{提交结果}。已预约下次提交：{下次调度}（±15分钟误差）"
		Else
			Return 提交结果
		End If
	End Function

	Async Sub 交抗原(延迟 As BackgroundTaskDeferral)
		Dim 字符串任务 = 交抗原核心(),
			日志流任务 = DirectCast(If(Await 数据目录.TryGetItemAsync("日志.log"), Await 数据目录.CreateFileAsync("日志.log")), StorageFile).OpenStreamForWriteAsync,
			日志流 As Stream
		Try
			日志流 = Await 日志流任务
		Catch ex As IOException
		End Try
		Dim 写出器 As StreamWriter
		If 日志流 IsNot Nothing Then
			日志流.Seek(0, SeekOrigin.End)
			写出器 = New StreamWriter(日志流)
			写出器.WriteLine(Date.Now & " 任务启动")
		End If
		Dim 结果 As String
		Try
			结果 = Await 字符串任务
		Catch ex As Exception
			结果 = $"{ex.GetType}
{ex.Message}
{ex.StackTrace}"
		End Try
		If 日志流 IsNot Nothing Then
			写出器.WriteLine($"{Date.Now} {结果}")
			写出器.Close()
		End If
		延迟.Complete()
	End Sub

	Async Function 交抗原() As Task(Of String)
		Try
			Return Await 交抗原核心()
		Catch ex As Exception
			Return $"{ex.GetType}
{ex.Message}
{ex.StackTrace}"
		End Try
	End Function
End Module