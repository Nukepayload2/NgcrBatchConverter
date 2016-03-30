Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.IO
Imports System.Management
Module 自定义拓展
    Public IgnoreNotSupported As Boolean = False
    Private Declare Function DeleteObject Lib "gdi32" Alias "DeleteObject" (ByVal hObject As IntPtr) As Integer
    ''' <summary>
    ''' 把Bitmap转换为ImageSource
    ''' </summary>
    ''' <param name="图">位图</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()>
    Public Function ToImageSource(图 As System.Drawing.Bitmap) As ImageSource
        Dim Hbitmap As IntPtr = 图.GetHbitmap
        Dim ImgSource As ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions)
        DeleteObject(Hbitmap)
        Return ImgSource
    End Function
    <Extension()>
    Function strbyte8(str As String) As String
        Dim sb As New StringBuilder
        For i As Integer = 1 To 8 - str.Length
            sb.Append("0")
        Next
        sb.Append(str)
        Return sb.ToString
    End Function

End Module
Module ComputerInfo
    Dim cpucount As Integer
    Dim getcpu As Func(Of Integer) = Function() cpucount
    Dim calccpu As Func(Of Integer) = Function()
                                          Dim c As New ManagementClass(New ManagementPath("Win32_Processor"))
                                          Dim moc = c.GetInstances
                                          For Each m In moc
                                              Dim cou As Integer = CInt(m.Properties("NumberOfCores").Value)
                                              If cou <> 0 Then
                                                  cpucount = cou
                                                  Exit For
                                              End If
                                          Next
                                          If cpucount = 0 Then
                                              cpucount = 1
                                          End If
                                          calccpu = getcpu
                                          Debug.WriteLine(cpucount)
                                          Return cpucount
                                      End Function
    ReadOnly Property CpuCoreCount As Integer
        Get
            Return calccpu.Invoke
        End Get
    End Property
End Module
Public Module StreamReaderExtensions

    <Extension()>
    Public Function Read1Byte(stream As Stream) As Byte
        Return CByte(stream.ReadByte())
    End Function

    <Extension()>
    Public Function ReadByte(stream As Stream, offset As Long) As Byte
        stream.Position = offset

        Return CByte(stream.ReadByte())
    End Function

    ' Read bytes 

    <Extension()>
    Public Function ReadBytes(stream As Stream, offset As Long, length As Integer) As Byte()
        Dim array As Byte() = New Byte(length - 1) {}
        stream.Position = offset
        stream.Read(array, 0, length)

        Return array
    End Function
    <Extension()>
    Public Function ReadBytes(stream As Stream, offset As Long, length As UInteger) As Byte()
        Return stream.ReadBytes(offset, CInt(length))
    End Function

    ' Read a short 

    <Extension()>
    Public Function ReadShort(stream As Stream, offset As Long) As Short
        Dim array As Byte() = New Byte(1) {}
        stream.Position = offset
        stream.Read(array, 0, 2)

        Return BitConverter.ToInt16(array, 0)
    End Function
    <Extension()>
    Public Function ReadUShort(stream As Stream, offset As Long) As UShort
        Dim array As Byte() = New Byte(1) {}
        stream.Position = offset
        stream.Read(array, 0, 2)

        Return BitConverter.ToUInt16(array, 0)
    End Function
    <Extension()>
    Public Function ReadUShort(stream As Stream) As UShort
        Dim array As Byte() = New Byte(1) {}
        stream.Read(array, 0, 2)
        Return BitConverter.ToUInt16(array, 0)
    End Function

    ' Read an integer 

    <Extension()>
    Public Function ReadInt(stream As Stream, offset As Long) As Integer
        Dim array As Byte() = New Byte(3) {}
        stream.Position = offset
        stream.Read(array, 0, 4)

        Return BitConverter.ToInt32(array, 0)
    End Function
    <Extension()>
    Public Function ReadUInt(stream As Stream, offset As Long) As UInteger
        Dim array As Byte() = New Byte(3) {}
        stream.Position = offset
        stream.Read(array, 0, 4)

        Return BitConverter.ToUInt32(array, 0)
    End Function
    <Extension()>
    Public Function ReadUInt(stream As Stream) As UInteger
        Dim array As Byte() = New Byte(3) {}
        stream.Read(array, 0, 4)
        Return BitConverter.ToUInt32(array, 0)
    End Function
    ' Read a string 

    <Extension()>
    Public Function ReadString(stream As Stream, maxLength As Integer) As String
        Dim str As String = String.Empty
        For i As Integer = 0 To maxLength - 1
            Dim chr As Char = ChrW(stream.ReadByte())
            str += chr
        Next
        Return str
    End Function
    <Extension()>
    Public Function ReadString(stream As Stream, offset As Long, maxLength As Integer, nullTerminator As Boolean) As String
        Dim str As String = String.Empty
        stream.Position = offset

        For i As Integer = 0 To maxLength - 1
            Dim chr As Char = ChrW(stream.ReadByte())
            If chr = ControlChars.NullChar AndAlso nullTerminator Then
                Exit For
            Else
                str += chr
            End If
        Next

        Return str
    End Function
    <Extension()>
    Public Function ReadString(stream As Stream, offset As Long, maxLength As Integer) As String
        Return stream.ReadString(offset, maxLength, True)
    End Function

    ' Read a string with a specific encoding
    <Extension()>
    Public Function ReadString(stream As Stream, offset As Long, maxLength As Integer, encoding As Encoding, nullTerminator As Boolean) As String
        stream.Position = offset

        Dim array As Byte() = New Byte(maxLength - 1) {}
        stream.Read(array, &H0, maxLength)

        Dim str As String = encoding.GetString(array)
        If nullTerminator Then
            str = str.TrimEnd(ControlChars.NullChar)
        End If

        Return str
    End Function

    <Extension()>
    Public Function ReadString(stream As Stream, offset As Long, maxLength As Integer, encoding As Encoding) As String
        Return stream.ReadString(offset, maxLength, encoding, True)
    End Function

    ' Copy to a byte array
    <Extension()>
    Public Function ToByteArray(stream As Stream) As Byte()
        Dim array As Byte() = New Byte(CInt(stream.Length - 1)) {}
        stream.Position = 0
        stream.Read(array, 0, array.Length)

        Return array
    End Function

    <Extension()>
    Public Sub Write(stream As Stream, value As Byte)
        stream.WriteByte(value)
    End Sub

    ' Write a short 

    <Extension()>
    Public Sub Write(stream As Stream, value As Short)
        stream.Write(BitConverter.GetBytes(value), 0, 2)
    End Sub
    <Extension()>
    Public Sub Write(stream As Stream, value As UShort)
        stream.Write(BitConverter.GetBytes(value), 0, 2)
    End Sub

    ' Write an integer 

    <Extension()>
    Public Sub Write(stream As Stream, value As Integer)
        stream.Write(BitConverter.GetBytes(value), 0, 4)
    End Sub
    <Extension()>
    Public Sub Write(stream As Stream, value As UInteger)
        stream.Write(BitConverter.GetBytes(value), 0, 4)
    End Sub

    ' Write a byte array 

    <Extension()>
    Public Sub Write(stream As Stream, values As Byte())
        stream.Write(values, 0, values.Length)
    End Sub

    ' Write a string 

    <Extension()>
    Public Sub Write(stream As Stream, value As String)
        For i As Integer = 0 To value.Length - 1
            stream.WriteByte(CByte(AscW(value(i))))
        Next
    End Sub
    <Extension()>
    Public Sub Write(stream As Stream, value As String, length As Integer)
        For i As Integer = 0 To length - 1
            If i < value.Length Then
                stream.WriteByte(CByte(AscW(value(i))))
            Else
                stream.WriteByte(&H0)
            End If
        Next
    End Sub
    <Extension()>
    Public Sub Write(stream As Stream, value As String, strLength As Integer, length As Integer)
        For i As Integer = 0 To length - 1
            If i < value.Length AndAlso i < strLength Then
                stream.WriteByte(CByte(AscW(value(i))))
            Else
                stream.WriteByte(&H0)
            End If
        Next
    End Sub

    ' Write a string with a specific encoding
    <Extension()>
    Public Sub Write(stream As Stream, value As String, strLength As Integer, length As Integer, encoding As Encoding)
        Dim strArray As Byte() = encoding.GetBytes(value)

        Dim trimAmount As Integer = 0
        While strArray.Length > strLength
            trimAmount += 1
            strArray = encoding.GetBytes(value.Substring(0, value.Length - trimAmount))
        End While

        For i As Integer = 0 To length - 1
            If i < strArray.Length AndAlso i < strLength Then
                stream.WriteByte(strArray(i))
            Else
                stream.WriteByte(&H0)
            End If
        Next
    End Sub

    ' Write a stream 

    <Extension()>
    Public Sub Write(output As Stream, input As Stream)
        Dim buffer As Byte() = New Byte(4095) {}
        input.Position = 0

        Dim bytes As Integer
        While (InlineAssignHelper(bytes, input.Read(buffer, 0, buffer.Length))) > 0
            output.Write(buffer, 0, bytes)
        End While
    End Sub
    <Extension()>
    Public Sub Write(output As Stream, input As MemoryStream)
        input.Position = 0
        input.WriteTo(output)
    End Sub
    <Extension()>
    Public Sub Write(output As Stream, input As Stream, offset As Long, length As Long)
        Dim buffer As Byte() = New Byte(4095) {}
        input.Position = offset

        Dim bytes As Integer
        While (InlineAssignHelper(bytes, input.Read(buffer, 0, CInt(If(input.Position + buffer.Length > offset + length, offset + length - input.Position, buffer.Length))))) > 0
            output.Write(buffer, 0, bytes)
        End While
    End Sub

    Private Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Module

Public Module MyFunc
    Public Function IIF(Of t)(exp As Boolean, tr As t, fa As t) As t
        If exp Then
            Return tr
        Else
            Return fa
        End If
    End Function
End Module