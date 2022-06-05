Imports System.Text.RegularExpressions
Imports System.IO.Path
Imports System.Globalization.NumberStyles
Imports cktool解码.字典类型

Enum 字典类型
	字符串
	零元调用
	运算符
	一元调用
	二元调用
End Enum

Structure 替换为
	Property 类型 As 字典类型
	Property 字符串 As String
End Structure

Module Program
	Const 变量正则 = "_0x\w+"
	ReadOnly 变量声明正则 As New Regex($"var ({变量正则})")

	Private Function 翻译字典(字典行 As String()) As Dictionary(Of String, 替换为)
		Static 字符串定义 As New Regex("'(\w{5})': ('.+')")
		Static 零元函数定义 As New Regex($"'(\w{{5}})': function \({变量正则}\)")
		Static 一元函数定义 As New Regex($"'(\w{{5}})': function \({变量正则}, {变量正则}\)")
		Static 二元函数定义 As New Regex($"'(\w{{5}})': function \({变量正则}, {变量正则}, {变量正则}\)")
		Dim 行号 = 0
		Dim 返回 As New Dictionary(Of String, 替换为)
		While 行号 < 字典行.Length
			Dim 行 = 字典行(行号)
			Dim 匹配 = 字符串定义.Match(行)
			If 匹配.Success Then
				返回.Add(匹配.Groups(1).Value, New 替换为 With {.类型 = 字符串, .字符串 = 匹配.Groups(2).Value})
				行号 += 1
				Continue While
			End If
			匹配 = 零元函数定义.Match(行)
			If 匹配.Success Then
				返回.Add(匹配.Groups(1).Value, New 替换为 With {.类型 = 零元调用})
				行号 += 3
				Continue While
			End If
			匹配 = 一元函数定义.Match(行)
			If 匹配.Success Then
				Static 运算符定义 As New Regex($"return {变量正则} (\+|\-|\*|\/|\&|\^|\>|\<|\%) {变量正则};")
				Static 一元调用定义 As New Regex($"return {变量正则}\({变量正则}\);")
				Dim 键 = 匹配.Groups(1).Value
				行 = 字典行(行号 + 1)
				匹配 = 运算符定义.Match(行)
				If 匹配.Success Then
					返回.Add(键, New 替换为 With {.类型 = 运算符, .字符串 = 匹配.Groups(1).Value})
					行号 += 3
					Continue While
				End If
				匹配 = 一元调用定义.Match(行)
				If 匹配.Success Then
					返回.Add(键, New 替换为 With {.类型 = 一元调用})
					行号 += 3
					Continue While
				End If
			End If
			匹配 = 二元函数定义.Match(行)
			If 匹配.Success Then
				返回.Add(匹配.Groups(1).Value, New 替换为 With {.类型 = 二元调用})
				行号 += 3
				Continue While
			End If
		End While
		Return 返回
	End Function

	Private Sub 翻译乱序表(函数行 As String(), 花括号 As Byte(), 写出器 As List(Of String))
		Static 乱序表正则 As New Regex($"var {变量正则} = '(\d+(\|\d+)*)'\['split'\]\('\|'\)")
		Dim 行号 = 0
		While 行号 < 函数行.Length
			Dim 行 = 函数行(行号)
			Dim 匹配 = 乱序表正则.Match(行)
			If 匹配.Success Then
				行号 += 4
				Dim 表头花括号 = 花括号(行号)
				Dim 表行数 = 花括号.Skip(行号).TakeWhile(Function(级数 As Byte) 级数 >= 表头花括号).Count
				Dim 乱序表 = 匹配.Groups(1).Value.Split("|"c)
				Dim 表头行 = 行号
				Dim 表尾行 = 表行数 + 表头行
				For 序数 = 0 To UBound(乱序表)
					Dim 条目头 = $"case '{乱序表(序数)}':"
					行号 = 表头行
					While 行号 < 表尾行
						If 函数行(行号) = 条目头 AndAlso 花括号(行号) = 表头花括号 Then
							行号 += 1
							Dim case头行 = 行号
							行 = 函数行(行号)
							While 花括号(行号) > 表头花括号 OrElse 花括号(行号) = 表头花括号 AndAlso 行 <> "continue;"
								行号 += 1
								If 行.StartsWith("return") Then
									Exit While
								Else
									行 = 函数行(行号)
								End If
							End While
							翻译乱序表(函数行.Skip(case头行).Take(行号 - case头行).ToArray, 花括号.Skip(case头行).Take(行号 - case头行).ToArray, 写出器)
							Exit While
						Else
							行号 += 1
						End If
					End While
				Next
				行号 = 表头行 + 表行数 + 3
			Else
				If 行 <> "" Then
					写出器.Add(行)
				End If
				行号 += 1
			End If
		End While
	End Sub

	Private Function 字典替换(ByRef 替换字符串 As String, 匹配 As Match, 字典 As Dictionary(Of String, 替换为)) As Boolean
		Dim 上界 = 替换字符串.Length - 1
		Dim 括号表(上界) As Byte
		Dim 位置 As UShort
		Dim 括号 = 0
		For 位置 = 0 To 上界
			Select Case 替换字符串(位置)
				Case "("c
					括号 += 1
				Case ")"c
					括号 -= 1
			End Select
			括号表(位置) = 括号
		Next
		If 括号 Then
			'括号没有闭合，需要获取下一行
			Return True
		Else
			Dim 返回 As New Text.StringBuilder(替换字符串.Take(匹配.Index).ToArray)
			位置 = 匹配.Index + 匹配.Length
			Dim 替换条目 = 字典(匹配.Groups(1).Value)
			Dim 参数(2) As String
			Dim 参数个数 = 0
			If 替换字符串(位置) = "("c Then
				'有参数
				Dim 初始括号 = 括号表(位置)
				位置 += 1
				Dim 字符 As Char
				Do
					Dim 初始位置 = 位置
					字符 = 替换字符串(位置)
					While 括号表(位置) > 初始括号 OrElse 括号表(位置) = 初始括号 AndAlso 字符 <> ","c
						位置 += 1
						字符 = 替换字符串(位置)
					End While
					参数(参数个数) = 替换字符串.Skip(初始位置).Take(位置 - 初始位置).ToArray
					参数个数 += 1
					位置 += 2
				Loop Until 字符 = ")"
				位置 -= 1
				Select Case 替换条目.类型
					Case 零元调用
						返回.Append(参数(0) & "()")
					Case 运算符
						返回.Append($"({参数(0)} {替换条目.字符串} {参数(1)})")
					Case 一元调用
						返回.Append($"{参数(0)}({参数(1)})")
					Case 二元调用
						返回.Append($"{参数(0)}({参数(1)}, {参数(2)})")
				End Select
			Else
				'没有参数
				返回.Append(替换条目.字符串)
			End If
			替换字符串 = 返回.Append(替换字符串.Skip(位置).ToArray).ToString
			Return False
		End If
	End Function

	Private Sub 翻译函数(函数行 As String(), 花括号 As Byte(), 写出器 As List(Of String))
		Dim 字典行数 = 花括号.TakeWhile(Function(级数 As Byte) 级数 > 1).Count
		Dim 字典名 = 变量声明正则.Match(函数行(0)).Groups(1).Value
		Dim 字典 As Dictionary(Of String, 替换为) = 翻译字典(函数行.Skip(1).Take(字典行数 - 1).ToArray)
		字典行数 += 1
		Dim 行号 = 字典行数
		Dim 行 As String
		Dim 字典正则 As New Regex(字典名 & "\['(\w{5})'\]")
		Dim 匹配 As Match
		While 行号 < 函数行.Length
			行 = 函数行(行号)
			匹配 = 字典正则.Match(行)
			While 匹配.Success
				While 字典替换(行, 匹配, 字典)
					函数行(行号) = ""
					行号 += 1
					行 &= 函数行(行号)
				End While
				匹配 = 字典正则.Match(行)
			End While
			函数行(行号) = 行
			行号 += 1
		End While
		翻译乱序表(函数行.Skip(字典行数).ToArray, 花括号.Skip(字典行数).ToArray, 写出器)
	End Sub

	Sub Main(args As String())
		Dim 密码路径 = args(0)
		Dim 写出器 As New List(Of String)
		Dim 源码分行 = IO.File.ReadAllLines(密码路径)
		Dim 密码表 = (From 匹配 In Regex.Matches(源码分行(0), "'([^, ]+)'") Select $"'{Text.Encoding.UTF8.GetString(Convert.FromBase64String(匹配.Groups(1).Value))}'").ToArray
		Dim 切牌量 = UShort.Parse(Regex.Match(源码分行(8), ", 0x(\w+)").Groups(1).Value, HexNumber) Mod 密码表.Length
		密码表 = 密码表.Skip(切牌量).Concat(密码表.Take(切牌量)).ToArray
		Dim 密码正则 As New Regex(变量声明正则.Match(源码分行(9)).Groups(1).Value & "\('0x(\w+)'\)")
		Static 空白正则 As New Regex("^( |\t)+")
		Dim 行号 As UShort
		For 行号 = 0 To 源码分行.Length - 1
			源码分行(行号) = 空白正则.Replace(密码正则.Replace(源码分行(行号), Function(匹配 As Match) 密码表(UShort.Parse(匹配.Groups(1).Value, HexNumber))), "")
		Next
		行号 = 0
		Dim 花括号 As SByte
		Dim 行 As String
		While 行号 < 源码分行.Length
			行 = 源码分行(行号)
			If 行.StartsWith("function abcd") OrElse 行.StartsWith("$(function") Then
				写出器.Add(行)
				Dim 函数起始行 = 行号 + 1
				花括号 = 1
				Dim 花括号等级表 As New List(Of Byte)
				While 花括号
					行号 += 1
					行 = 源码分行(行号)
					花括号 += 行.Count(Function(字符 As Char) 字符 = "{") - 行.Count(Function(字符 As Char) 字符 = "}")
					花括号等级表.Add(花括号)
				End While
				'只传递函数体，首行签名和末行花括号都不传入
				翻译函数(源码分行.Skip(函数起始行).Take(行号 - 函数起始行).ToArray, 花括号等级表.SkipLast(1).ToArray, 写出器)
				写出器.Add(行)
			End If
			行号 += 1
		End While
		花括号 = 0
		For 行号 = 0 To 写出器.Count - 1
			行 = 写出器(行号)
			Dim 缩进 = Enumerable.Repeat(CChar(vbTab), If(行.StartsWith("}"), 花括号 - 1, 花括号))
			写出器(行号) = 缩进.ToArray & 行
			花括号 += 行.Count(Function(字符 As Char) 字符 = "{") - 行.Count(Function(字符 As Char) 字符 = "}")
		Next
		IO.File.WriteAllLinesAsync(Combine(GetDirectoryName(密码路径), GetFileNameWithoutExtension(密码路径) & ".解码.js"), 写出器)
	End Sub
End Module
