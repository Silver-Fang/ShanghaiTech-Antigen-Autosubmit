' https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
Imports Windows.Storage
Imports Windows.ApplicationModel.Background
Imports Windows.System.Launcher

''' <summary>
''' 可用于自身或导航至 Frame 内部的空白页。
''' </summary>
Public NotInheritable Class MainPage
	Inherits Page
	ReadOnly 照片选取器 As New Pickers.FileOpenPicker
	Shared ReadOnly 当前应用 As App = Application.Current, 预约数据表 As ObservableCollection(Of 预约表行) = 当前应用.预约数据表

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		照片选取器.FileTypeFilter.Add(".jpg")
		照片选取器.FileTypeFilter.Add(".png")
		学号或工号.SetBinding(Microsoft.UI.Xaml.Controls.NumberBox.ValueProperty, 当前应用.学号或工号绑定)
		密码.SetBinding(PasswordBox.PasswordProperty, 当前应用.密码绑定)
		自动执行.SetBinding(ToggleSwitch.IsOnProperty, 当前应用.自动任务绑定)
		每日重试.SetBinding(ToggleButton.IsCheckedProperty, 当前应用.每日重试绑定)
		成功喵提醒.SetBinding(ToggleButton.IsCheckedProperty, 当前应用.成功喵提醒绑定)
		失败喵提醒.SetBinding(ToggleButton.IsCheckedProperty, 当前应用.失败喵提醒绑定)
		喵提醒码.SetBinding(TextBox.TextProperty, 当前应用.喵提醒码绑定)
		预约表.ItemsSource = 预约数据表
		AddHandler 自动执行.Toggled, AddressOf 自动执行_Toggled
	End Sub

	Private 正在编辑 As Integer

	Private Sub 添加预约_Click(sender As Object, e As RoutedEventArgs) Handles 添加预约.Click
		Static 上午时间 As New TimeSpan(13, 11, 0),
			下午时间 As New TimeSpan(20, 0, 0)，
			一天 As New TimeSpan(1, 0, 0, 0)
		正在编辑 = -1
		Dim 现在 = Date.Now
		If 现在.Hour < 9 Then
			选择日期.Date = 现在
			选择时间.Time = 上午时间
		ElseIf 现在.Hour < 20 Then
			选择日期.Date = 现在
			选择时间.Time = 下午时间
		Else
			选择日期.Date = 现在 + 一天
			选择时间.Time = 上午时间
		End If
	End Sub

	Private Sub 编辑预约_Click(sender As Object, e As RoutedEventArgs) Handles 编辑预约.Click
		正在编辑 = 预约表.SelectedIndex
		If 正在编辑 = -1 Then
			错误信息.Text = "必须先在预约表中选择一条预约才能编辑"
			错误提示.ShowAt(sender)
		Else
			Dim 编辑行 = 预约数据表(正在编辑)
			选择日期.Date = 编辑行.日期时间
			选择时间.Time = 编辑行.日期时间.TimeOfDay
			编辑照片.Source = 编辑行.照片
			编辑编号.Text = 编辑行.编号
			预约面板.ShowAt(sender)
		End If
	End Sub

	Shared ReadOnly 照片列表 As AccessCache.StorageItemAccessList = AccessCache.StorageApplicationPermissions.FutureAccessList

	Private Sub 删除预约_Click(sender As Object, e As RoutedEventArgs) Handles 删除预约.Click
		正在编辑 = 预约表.SelectedIndex
		If 正在编辑 = -1 Then
			错误信息.Text = "必须先在预约表中选择一条预约才能删除"
			错误提示.ShowAt(sender)
		Else
			Dim 令牌 = 预约数据表(正在编辑).照片令牌
			预约数据表.RemoveAt(正在编辑)
			If Not (From 行 In 预约数据表 Where 行.照片令牌 = 令牌 Select 行.照片令牌).Any Then
				照片列表.Remove(令牌)
			End If
		End If
	End Sub

	Private Sub 编辑照片_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles 编辑照片.Tapped
		文件或拍照.ShowAt(sender)
	End Sub

	Private Sub 恢复预约面板()
		If 正在编辑 = -1 Then
			预约面板.ShowAt(添加预约)
		Else
			预约面板.ShowAt(编辑预约)
		End If
	End Sub

	Private Async Sub 设置照片文件(照片文件 As StorageFile)
		编辑照片.Tag = 照片文件
		Dim 位图 As New BitmapImage
		Call 位图.SetSourceAsync(Await 照片文件.OpenReadAsync)
		编辑照片.Source = 位图
		恢复预约面板()
	End Sub

	Shared ReadOnly 捕获UI As New Windows.Media.Capture.CameraCaptureUI

	Private Async Sub 拍照_Click(sender As Object, e As RoutedEventArgs) Handles 拍照.Click
		Dim 照片文件 = Await 捕获UI.CaptureFileAsync(Windows.Media.Capture.CameraCaptureUIMode.Photo)
		If 照片文件 Is Nothing Then
			恢复预约面板()
		Else
			Await 照片文件.MoveAsync(ApplicationData.Current.LocalFolder, 照片文件.Name, NameCollisionOption.GenerateUniqueName)
			设置照片文件(照片文件)
		End If
	End Sub

	Private Async Sub 选择图像文件_Click(sender As Object, e As RoutedEventArgs) Handles 选择图像文件.Click
		Dim 照片文件 = Await 照片选取器.PickSingleFileAsync
		If 照片文件 Is Nothing Then
			恢复预约面板()
		Else
			设置照片文件(照片文件)
		End If
	End Sub

	Private Async Sub 确认编辑_Click(sender As Object, e As RoutedEventArgs) Handles 确认编辑.Click
		If 正在编辑 = -1 Then
			If 编辑照片.Tag Is Nothing Then
				错误信息.Text = "没有设置照片"
				错误提示.ShowAt(sender)
			Else
				预约数据表.Add(Await 预约表行.新建(选择日期.Date.Date + 选择时间.Time, 编辑编号.Text, 编辑照片.Tag))
				自动执行_Toggled()
			End If
		Else
			With 预约数据表(正在编辑)
				.日期时间 = 选择日期.Date.Date + 选择时间.Time
				.编号 = 编辑编号.Text
				If 编辑照片.Source IsNot .照片 Then
					.照片 = 编辑照片.Source
					.照片文件 = 编辑照片.Tag
					照片列表.AddOrReplace(.照片令牌, .照片文件)
				End If
			End With
			自动执行_Toggled()
		End If
	End Sub

	Shared ReadOnly 调度间隔 As New TimeSpan(0, 15, 0)

	Private Async Sub 自动执行_Toggled()
		自动执行.IsEnabled = False
		BackgroundExecutionManager.RemoveAccess()
		If 自动执行.IsOn Then
			Dim 回答 = Await BackgroundExecutionManager.RequestAccessAsync()
			If 回答 = BackgroundAccessStatus.AllowedSubjectToSystemPolicy OrElse 回答 = BackgroundAccessStatus.AlwaysAllowed Then
				立即提交_Click()
			Else
				自动执行.IsOn = False
				任务提示.Text = "后台任务请求被拒绝，请检查系统设置"
			End If
		End If
		自动执行.IsEnabled = True
	End Sub

	Private Async Sub 立即提交_Click() Handles 立即提交.Click
		立即提交.IsEnabled = False
		进度环.IsActive = True
		Await 当前应用.保存预约()
		任务提示.Text = Await 交抗原()
		当前应用.预约数据表.Clear()
		Call 当前应用.载入预约数据()
		进度环.IsActive = False
		立即提交.IsEnabled = True
	End Sub

	Private Async Sub 打开日志_Click(sender As Object, e As RoutedEventArgs) Handles 打开日志.Click
		Dim 日志文件 = If(Await App.数据目录.TryGetItemAsync("日志.log"), Await App.数据目录.CreateFileAsync("日志.log"))
		Call LaunchFileAsync(日志文件)
	End Sub

	Private Sub 帮助文档_Click(sender As Object, e As RoutedEventArgs) Handles 帮助文档.Click
		Call LaunchFileAsync(当前应用.帮助文档)
	End Sub
End Class
