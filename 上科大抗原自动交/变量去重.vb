Friend Class 变量去重
	Implements IEqualityComparer(Of KeyValuePair(Of String, String))
	Public Shared ReadOnly 唯一实例 As New 变量去重

	Public Overloads Function Equals(x As KeyValuePair(Of String, String), y As KeyValuePair(Of String, String)) As Boolean Implements IEqualityComparer(Of KeyValuePair(Of String, String)).Equals
		Return x.Key = y.Key
	End Function

	Public Overloads Function GetHashCode(obj As KeyValuePair(Of String, String)) As Integer Implements IEqualityComparer(Of KeyValuePair(Of String, String)).GetHashCode
		Return obj.Key.GetHashCode
	End Function
End Class
