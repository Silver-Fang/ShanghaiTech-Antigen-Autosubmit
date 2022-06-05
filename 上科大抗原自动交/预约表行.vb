Imports Windows.Storage

Class 预约表行
	Implements INotifyPropertyChanged
	Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
	Private i日期时间 As Date

	Property 日期时间 As Date
		Get
			Return i日期时间
		End Get
		Set(value As Date)
			Static 日期时间事件参数 As New PropertyChangedEventArgs("日期时间")
			i日期时间 = value
			RaiseEvent PropertyChanged(Me, 日期时间事件参数)
		End Set
	End Property

	Private i编号 As String

	Property 编号 As String
		Get
			Return i编号
		End Get
		Set(value As String)
			Static 编号改变事件参数 As New PropertyChangedEventArgs("编号")
			i编号 = value
			RaiseEvent PropertyChanged(Me, 编号改变事件参数)
		End Set
	End Property

	Property 照片令牌 As String
	Property 照片文件 As StorageFile

	Shared ReadOnly 照片改变事件参数 As New PropertyChangedEventArgs("照片")
	Private i照片 As New BitmapImage

	Property 照片 As BitmapImage
		Get
			Return i照片
		End Get
		Set(value As BitmapImage)
			i照片 = value
			RaiseEvent PropertyChanged(Me, 照片改变事件参数)
		End Set
	End Property

	Shared ReadOnly 照片列表 As AccessCache.StorageItemAccessList = AccessCache.StorageApplicationPermissions.FutureAccessList

	Private Sub New()
	End Sub

	Shared Async Function 读入(读入器 As BinaryReader) As Task(Of 预约表行)
		Dim 返回值 As New 预约表行 With {.i日期时间 = New Date(读入器.ReadInt64), .i编号 = 读入器.ReadString, .照片令牌 = 读入器.ReadString}
		Try
			返回值.照片文件 = Await 照片列表.GetFileAsync(返回值.照片令牌)
		Catch ex As FileNotFoundException
			Return Nothing
		End Try
		Call 返回值.i照片.SetSourceAsync(Await 返回值.照片文件.OpenReadAsync)
		Return 返回值
	End Function

	Shared Async Function 新建(日期时间 As Date, 编号 As String, 照片文件 As StorageFile) As Task(Of 预约表行)
		Dim 返回值 As New 预约表行 With {.i日期时间 = 日期时间, .i编号 = 编号, .照片文件 = 照片文件}
		返回值.照片令牌 = 照片列表.Add(照片文件)
		Call 返回值.i照片.SetSourceAsync(Await 返回值.照片文件.OpenReadAsync)
		Return 返回值
	End Function

	Sub 写出(写出器 As BinaryWriter)
		写出器.Write(日期时间.Ticks)
		写出器.Write(编号)
		写出器.Write(照片令牌)
	End Sub
End Class