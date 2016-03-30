Imports System.IO
Imports System.Text

''' <summary>
''' Operations with ARCH pack file.
''' </summary>
Public Class Arch
    Private Const Padding As Integer = &H10
    Private Const MagicStamp As String = "ARCH"
    Private Shared ReadOnly DefaultEncoding As Encoding = Encoding.ASCII

    Public Function Get_Format(file As sFile, magic As Byte()) As Format
        If Encoding.ASCII.GetString(magic) = MagicStamp Then
            Return Format.Pack
        End If

        Return Format.Unknown
    End Function

    Public Function Unpack(file__1 As sFile) As sFolder
        Dim strIn As Stream = File.OpenRead(file__1.path)
        Dim br As New BinaryReader(strIn)
        Dim unpacked As New sFolder()
        unpacked.files = New List(Of sFile)()
        unpacked.folders = New List(Of sFolder)()

        ' Read header
        Dim magicStamp As New String(br.ReadChars(4))
        Dim numFiles As UInteger = br.ReadUInt32()
        Dim fntOffset As UInteger = br.ReadUInt32()
        Dim fatOffset As UInteger = br.ReadUInt32()
        Dim natOffset As UInteger = br.ReadUInt32()
        Dim filesOffset As UInteger = br.ReadUInt32()

        ' Extract files
        For i As Integer = 0 To CInt(numFiles - 1)
            strIn.Position = natOffset
            SetNameOffset(strIn, i)
            Dim nameOffset As UShort = br.ReadUInt16()

            strIn.Position = fntOffset + nameOffset
            Dim filename As String = ReadString(strIn)

            strIn.Position = fatOffset + (&H10 * i)
            Dim encodedSize As Integer = br.ReadInt32()
            Dim decodedSize As Integer = br.ReadInt32()
            Dim fileOffset As UInteger = br.ReadUInt32() + filesOffset
            Dim nameOffset2 As UShort = br.ReadUInt16()
            Dim isEncoded As Boolean = br.ReadUInt16() = 1

            Dim result As Boolean
            Console.Write("{0} file {1}... ", If(isEncoded, "Decoding", "Saving"), filename)
            Dim newFile As New sFile()
            If isEncoded Then
                Dim decodedPath As String = Path.GetTempFileName 
                newFile.offset = 0
                newFile.size = CUInt(decodedSize)
                newFile.path = decodedPath

                Dim dec As New Decoder(strIn, fileOffset, encodedSize, decodedSize)
                result = dec.Decode(decodedPath)
            Else
                newFile.offset = fileOffset
                newFile.path = file__1.path
                newFile.size = CUInt(encodedSize)

                result = True
            End If

            Console.Write("{0}", If(result, "Ok", "Fail"))
            AddFile(unpacked, newFile, filename)
        Next

        br.Close()
        br = Nothing
        Return unpacked
    End Function

    Public Function Pack(ByRef unpacked As sFolder, file__1 As sFile) As String
        Dim files As sFile() = GetFiles(unpacked)
        Dim numFiles As Integer = files.Length

        ' Write the sections       
        Dim bw As BinaryWriter

        ' A) Fnt
        Dim fntStr As New MemoryStream()
        bw = New BinaryWriter(fntStr)
        Dim namesOffsets As UShort() = New UShort(numFiles - 1) {}

        bw.Write(&H0)
        ' I'll write later the section size
        bw.Write(numFiles)
        For i As Integer = 0 To numFiles - 1
            namesOffsets(i) = CUShort(fntStr.Position)
            WriteString(fntStr, files(i).name)
        Next

        WritePadding(fntStr)

        ' Now write section size
        fntStr.Position = 0
        bw.Write(CUInt(fntStr.Length))
        bw.Flush()
        bw = Nothing

        ' B) Fat
        Dim fatStr As New MemoryStream()
        bw = New BinaryWriter(fatStr)

        Dim offset As UInteger = &H0
        For i As Integer = 0 To numFiles - 1
            bw.Write(files(i).size)
            bw.Write(&H0)
            bw.Write(offset)
            bw.Write(namesOffsets(i))
            bw.Write(CUShort(&H0))
            ' No encoding
            offset = AddPadding(offset + files(i).size)
        Next

        bw.Flush()
        bw = Nothing

        ' C) Nat
        Dim natStr As New MemoryStream()
        bw = New BinaryWriter(natStr)

        For i As Integer = 0 To numFiles - 1
            bw.Write(CUShort(i))
            bw.Write(namesOffsets(i))
        Next

        WritePadding(natStr)
        bw.Flush()
        bw = Nothing

        ' D) Write file
        Dim outFile As String = Path.GetTempFileName
        Dim strOut As Stream = File.OpenWrite(outFile)
        bw = New BinaryWriter(strOut)

        ' Calculate section offsets
        Dim fntOffset As UInteger = &H20
        ' After header
        Dim fatOffset As UInteger = fntOffset + CUInt(fntStr.Length)
        ' After Fnt
        Dim natOffset As UInteger = fatOffset + CUInt(fatStr.Length)
        ' After Fat
        Dim filesOffset As UInteger = natOffset + CUInt(natStr.Length)
        ' After Nat
        ' Write header
        bw.Write(Encoding.ASCII.GetBytes(MagicStamp))
        bw.Write(numFiles)
        bw.Write(fntOffset)
        bw.Write(fatOffset)
        bw.Write(natOffset)
        bw.Write(filesOffset)
        WritePadding(strOut)
        bw.Flush()

        ' Write sections
        fntStr.WriteTo(strOut)
        fatStr.WriteTo(strOut)
        natStr.WriteTo(strOut)
        bw.Flush()

        ' Write files
        Dim br As BinaryReader
        For i As Integer = 0 To numFiles - 1
            br = New BinaryReader(File.OpenRead(files(i).path))
            br.BaseStream.Position = files(i).offset
            bw.Write(br.ReadBytes(CInt(files(i).size)))
            br.Close()
            br = Nothing

            WritePadding(strOut)
            bw.Flush()
        Next

        bw.Flush()
        bw.Close()
        bw = Nothing
        Return outFile
    End Function
    Private Shared Sub AddFile(folder As sFolder, file As sFile, filePath As String)
        If filePath.Contains("\") Then
            Dim folderName As String = filePath.Substring(0, filePath.IndexOf("\"c))
            Dim subfolder As New sFolder()
            For Each f As sFolder In folder.folders
                If f.name = folderName Then
                    subfolder = f
                End If
            Next

            If String.IsNullOrEmpty(subfolder.name) Then
                subfolder.name = folderName
                subfolder.folders = New List(Of sFolder)()
                subfolder.files = New List(Of sFile)()
                folder.folders.Add(subfolder)
            End If

            AddFile(subfolder, file, filePath.Substring(filePath.IndexOf("\"c) + 1))
        Else
            file.name = filePath
            folder.files.Add(file)
        End If
    End Sub

    Private Shared Function GetFiles(folder As sFolder) As sFile()
        Dim files As New List(Of sFile)()
        Dim queue As New Queue(Of sFolder)()
        folder.name = String.Empty
        queue.Enqueue(folder)

        Do
            Dim currentFolder As sFolder = queue.Dequeue()
            For Each f As sFolder In currentFolder.folders
                Dim subfolder As sFolder = f
                If Not String.IsNullOrEmpty(currentFolder.name) Then
                    subfolder.name = currentFolder.name + "\"c + subfolder.name
                End If

                queue.Enqueue(subfolder)
            Next

            For Each f As sFile In currentFolder.files
                Dim file As sFile = f
                If Not String.IsNullOrEmpty(currentFolder.name) Then
                    file.name = currentFolder.name + "\"c + file.name
                End If
                files.Add(file)
            Next
        Loop While queue.Count <> 0

        Return files.ToArray()
    End Function

    Private Shared Sub WritePadding(str As Stream)
        While str.Position Mod Padding <> 0
            str.WriteByte(&H0)
        End While
    End Sub

    Private Shared Function AddPadding(val As UInteger) As UInteger
        If val Mod Padding <> 0 Then
            val = CUInt(val + (Padding - (val Mod Padding)))
        End If

        Return val
    End Function

    Private Shared Function ReadString(str As Stream) As String
        Dim s As String = String.Empty
        Dim data As New List(Of Byte)()

        While str.ReadByte() <> 0
            str.Position -= 1
            data.Add(CByte(str.ReadByte()))
        End While

        s = DefaultEncoding.GetString(data.ToArray())
        data.Clear()
        data = Nothing

        Return s
    End Function

    Private Shared Sub WriteString(str As Stream, s As String)
        Dim data As Byte() = DefaultEncoding.GetBytes(s & ControlChars.NullChar)
        str.Write(data, 0, data.Length)
    End Sub

    Private Shared Sub SetNameOffset(str As Stream, fileId As Integer)
        Dim br As New BinaryReader(str)
        While br.ReadUInt16() <> fileId
            br.ReadUInt16()
        End While

        br = Nothing
    End Sub
End Class

''' <summary>
''' Decode Arch files.
''' </summary>
Public Class Decoder
    Private nextSamples As New Stack(Of Byte)(&H80)
    Private buffer1 As Byte() = New Byte(255) {}
    Private buffer2 As Byte() = New Byte(255) {}

    Private str As Stream
    Private encodedSize As Integer
    Private decodedSize As Integer

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Decoder" /> class.
    ''' </summary>
    ''' <param name="str">Stream with the data encoded.</param>
    ''' <param name="decodedSize">Size of the decoded file.</param>
    Public Sub New(str As Stream, Optional decodedSize As Integer = -1)
        Me.New(str, 0, CInt(str.Length), decodedSize)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Decoder" /> class.
    ''' </summary>
    ''' <param name="str">Stream with the data encoded</param>
    ''' <param name="offset">Offset to the data encoded.</param>
    ''' <param name="encodedSize">Size of the encoded file.</param>
    ''' <param name="decodedSize">Size of the decoded file.</param>
    Public Sub New(str As Stream, offset As UInteger, encodedSize As Integer, decodedSize As Integer)
        str.Position = offset
        Me.str = str
        Me.encodedSize = encodedSize
        Me.decodedSize = decodedSize
    End Sub

    ''' <summary>
    ''' Decode the data.
    ''' </summary>
    ''' <param name="fileOut">Path to the output file.</param>
    ''' <returns>A value indicating whether the operation was successfully.</returns>
    Public Function Decode(fileOut As String) As Boolean
        If File.Exists(fileOut) Then
            File.Delete(fileOut)
        End If

        Dim fs As New FileStream(fileOut, FileMode.Create, FileAccess.Write)

        Dim result As Boolean = Me.Decode(fs)

        fs.Flush()
        fs.Close()
        fs.Dispose()

        Return result
    End Function

    ''' <summary>
    ''' Decode the data.
    ''' </summary>
    ''' <param name="strOut">Stream to the output file.</param>
    ''' <returns>A value indicating whether the operation was successfully.</returns>
    Public Function Decode(strOut As Stream) As Boolean
        Dim startReading As Long = Me.str.Position
        Dim startWriting As Long = strOut.Position

        While Me.str.Position - startReading < Me.encodedSize
            InitBuffer(Me.buffer2)
            Me.FillBuffer()

            Me.Process(strOut)
        End While

        If Me.decodedSize <> -1 Then
            Return (strOut.Position - startWriting) = Me.decodedSize
        Else
            Return True
        End If
    End Function

    Private Shared Sub InitBuffer(buffer As Byte())
        If buffer.Length > &H100 Then
            Throw New ArgumentException("Invalid buffer length", "buffer")
        End If

        For i As Integer = 0 To buffer.Length - 1
            buffer(i) = CByte(i)
        Next
    End Sub

    Private Sub FillBuffer()
        Dim index As Integer = 0

        While index <> &H100
            Dim id As Integer = Me.str.ReadByte()
            Dim numLoops As Integer = id

            If id > &H7F Then
                numLoops = 0
                Dim skipPositions As Integer = id - &H7F
                index += skipPositions
            End If

            If index = &H100 Then
                Exit While
            End If

            ' It's in the ARM code but... really?
            If numLoops < 0 Then
                Continue While
            End If

            For i As Integer = 0 To numLoops
                Dim b As Byte = CByte(Me.str.ReadByte())
                Me.buffer2(index) = b

                ' It'll write
                If b <> index Then
                    Me.buffer1(index) = CByte(Me.str.ReadByte())
                End If

                index += 1
            Next
        End While
    End Sub

    Private Sub Process(strOut As Stream)
        Dim numLoops As Integer = (Me.str.ReadByte() << 8) + Me.str.ReadByte()
        Me.nextSamples.Clear()
        Dim index As Integer

        While True
            If Me.nextSamples.Count = 0 Then
                If numLoops = 0 Then
                    Return
                End If

                numLoops -= 1
                index = Me.str.ReadByte()
            Else
                index = Me.nextSamples.Pop()
            End If

            If Me.buffer2(index) = index Then
                strOut.WriteByte(CByte(index))
            Else
                Me.nextSamples.Push(Me.buffer1(index))
                Me.nextSamples.Push(Me.buffer2(index))
                index = Me.nextSamples.Count
            End If
        End While
    End Sub
End Class