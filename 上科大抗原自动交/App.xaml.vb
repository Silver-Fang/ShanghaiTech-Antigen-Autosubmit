Imports Windows.Storage

''' <summary>
''' 提供特定于应用程序的行为，以补充默认的应用程序类。
''' </summary>
NotInheritable Class App
	Inherits Application
	Implements INotifyPropertyChanged
	Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

	Property 学号或工号 As UInteger
	Friend ReadOnly 学号或工号绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("学号或工号"), .Mode = BindingMode.TwoWay}
	Property 密码 As String = ""
	Friend ReadOnly 密码绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("密码"), .Mode = BindingMode.TwoWay}
	Property 自动任务 As Boolean
	Friend ReadOnly 自动任务绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("自动任务"), .Mode = BindingMode.TwoWay}
	Property 每日重试 As Boolean
	Friend ReadOnly 每日重试绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("每日重试"), .Mode = BindingMode.TwoWay}
	Property 成功喵提醒 As Boolean
	Friend ReadOnly 成功喵提醒绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("成功喵提醒"), .Mode = BindingMode.TwoWay}
	Property 失败喵提醒 As Boolean
	Friend ReadOnly 失败喵提醒绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("失败喵提醒"), .Mode = BindingMode.TwoWay}
	Property 喵提醒码 As String = ""
	Friend ReadOnly 喵提醒码绑定 As New Binding With {.Source = Me, .Path = New PropertyPath("喵提醒码"), .Mode = BindingMode.TwoWay}
	Friend ReadOnly 预约数据表 As New ObservableCollection(Of 预约表行)
	Friend Shared ReadOnly 数据目录 As StorageFolder = ApplicationData.Current.LocalFolder
	Private 数据文件 As StorageFile
	Friend 帮助文档 As StorageFile

	Async Function 载入预约数据() As Task
		数据文件 = If(Await 数据目录.TryGetItemAsync("预约数据.bin"), Await 数据目录.CreateFileAsync("预约数据.bin"))
		Dim 读入器 = New BinaryReader(Await 数据文件.OpenStreamForReadAsync)
		Try
			学号或工号 = 读入器.ReadUInt32
			密码 = 读入器.ReadString
			自动任务 = 读入器.ReadBoolean
			每日重试 = 读入器.ReadBoolean
			成功喵提醒 = 读入器.ReadBoolean
			失败喵提醒 = 读入器.ReadBoolean
			喵提醒码 = 读入器.ReadString
			Dim 预约数 = 读入器.ReadInt32
			Dim 新行 As 预约表行
			For a = 1 To 预约数
				新行 = Await 预约表行.读入(读入器)
				If 新行 IsNot Nothing Then
					预约数据表.Add(新行)
				End If
			Next
		Catch ex As EndOfStreamException
		Catch ex As ArgumentOutOfRangeException
		End Try
	End Function

	''' <summary>
	''' 在应用程序由最终用户正常启动时进行调用。
	''' 当启动应用程序以打开特定的文件或显示时使用
	''' 搜索结果等
	''' </summary>
	''' <param name="e">有关启动请求和过程的详细信息。</param>
	Protected Overrides Async Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
		Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

		' 不要在窗口已包含内容时重复应用程序初始化，
		' 只需确保窗口处于活动状态

		If rootFrame Is Nothing Then
			' 创建要充当导航上下文的框架，并导航到第一页
			rootFrame = New Frame()

			AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

			If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
				' TODO: 从之前挂起的应用程序加载状态
			End If
			' 将框架放在当前窗口中
			Window.Current.Content = rootFrame
		End If
		Await 载入预约数据()
		帮助文档 = Await StorageFile.GetFileFromApplicationUriAsync(New Uri("ms-appx:///帮助文档.txt"))

		If e.PrelaunchActivated = False Then
			If rootFrame.Content Is Nothing Then
				' 当导航堆栈尚未还原时，导航到第一页，
				' 并通过将所需信息作为导航参数传入来配置
				' 参数
				rootFrame.Navigate(GetType(MainPage), e.Arguments)
			End If

			' 确保当前窗口处于活动状态
			Window.Current.Activate()
		End If
	End Sub

	''' <summary>
	''' 导航到特定页失败时调用
	''' </summary>
	'''<param name="sender">导航失败的框架</param>
	'''<param name="e">有关导航失败的详细信息</param>
	Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
		Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
	End Sub

	Async Function 保存预约() As Task
		Dim 写出器 = New BinaryWriter(Await 数据文件.OpenStreamForWriteAsync)
		写出器.Write(学号或工号)
		写出器.Write(密码)
		写出器.Write(自动任务)
		写出器.Write(每日重试)
		写出器.Write(成功喵提醒)
		写出器.Write(失败喵提醒)
		写出器.Write(喵提醒码)
		写出器.Write(预约数据表.Count)
		For Each 行 As 预约表行 In 预约数据表
			行.写出(写出器)
		Next
		写出器.Close()
	End Function

	''' <summary>
	''' 在将要挂起应用程序执行时调用。  在不知道应用程序
	''' 无需知道应用程序会被终止还是会恢复，
	''' 并让内存内容保持不变。
	''' </summary>
	''' <param name="sender">挂起的请求的源。</param>
	''' <param name="e">有关挂起请求的详细信息。</param>
	Private Async Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
		Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
		' TODO: 保存应用程序状态并停止任何后台活动
		Await 保存预约()
		deferral.Complete()
	End Sub

	Protected Overrides Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
		交抗原(args.TaskInstance.GetDeferral)
	End Sub
End Class
