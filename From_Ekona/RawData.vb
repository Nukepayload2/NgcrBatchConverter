' ----------------------------------------------------------------------
' <copyright file="RawData.cs" company="none">

' Copyright (C) 2012
'
'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by 
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
'
'   This program is distributed in the hope that it will be useful, 
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details. 
'
'   You should have received a copy of the GNU General Public License
'   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
'
' </copyright>

' <author>pleoNeX</author>
' <email>benito356@gmail.com</email>
' <date>23/06/2012 19:04:27</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing



Public Class RawPalette
    Inherits PaletteBase
    ' Unknown data
    Private prev_data As Byte()
    Private next_data As Byte()

    Public Sub New(file As String, id As Integer, editable As Boolean, depth As ColorFormat, offset As Integer, size As Integer, _
     Optional fileName As String = "")
        MyBase.New()
        If fileName = "" Then
            Me.FileName = System.IO.Path.GetFileName(file)
        Else
            Me.FileName = fileName
        End If
        
        Read(file, editable, depth, offset, size)
    End Sub
    Public Sub New(colors As Color()(), editable As Boolean, depth As ColorFormat, Optional fileName As String = "")
        MyBase.New()
        Me.FileName = fileName
        Set_Palette(colors, depth, editable)
    End Sub
    Public Sub New(colors As Color(), editable As Boolean, depth As ColorFormat, Optional fileName As String = "")
        MyBase.New()
        Me.FileName = fileName
        Set_Palette(New Color()() {colors}, depth, editable)
    End Sub
    Public Sub New(file As String, id As Integer, editable As Boolean, offset As Integer, size As Integer, Optional fileName As String = "")
        MyBase.New()
        If fileName = "" Then
            Me.FileName = System.IO.Path.GetFileName(file)
        Else
            Me.FileName = fileName
        End If


        Read(file, editable, offset, size)
    End Sub


    Public Overrides Sub Read(fileIn As String)
        Read(fileIn, True, 0, -1)
    End Sub
    Public Overloads Sub Read(fileIn As String, editable As Boolean, depth As ColorFormat, offset As Integer, fileSize As Integer)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        prev_data = br.ReadBytes(offset)

        If fileSize <= 0 Then
            fileSize = CInt(br.BaseStream.Length)
        End If
        If fileSize > &H2000 Then
            fileSize = &H2000
        End If

        Dim palette_length As Integer = &H200
        If depth = ColorFormat.colors16 OrElse fileSize < &H200 Then
            palette_length = &H20
        End If

        ' Color data
        Dim palette As Color()() = New Color(fileSize \ palette_length - 1)() {}
        For i As Integer = 0 To palette.Length - 1
            palette(i) = Actions.BGR555ToColor(br.ReadBytes(palette_length))
        Next

        next_data = br.ReadBytes(CInt(br.BaseStream.Length - fileSize))

        br.Close()

        Set_Palette(palette, depth, editable)
    End Sub
    Public Overloads Sub Read(fileIn As String, editable As Boolean, offset As Integer, fileSize As Integer)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        prev_data = br.ReadBytes(offset)

        If fileSize <= 0 Then
            fileSize = CInt(br.BaseStream.Length)
        End If
        Dim fileSize_ As Integer = fileSize
        If fileSize > &H2000 Then
            fileSize = &H2000
        End If

        Dim palette_length As Integer = &H200
        If fileSize < &H200 Then
            palette_length = fileSize
        End If

        ' Color data
        Dim palette As Color()() = New Color(fileSize \ palette_length - 1)() {}
        For i As Integer = 0 To palette.Length - 1
            palette(i) = Actions.BGR555ToColor(br.ReadBytes(palette_length))
        Next

        next_data = br.ReadBytes(CInt(br.BaseStream.Length - fileSize))

        Set_Palette(palette, editable)

        br.BaseStream.Position = offset
        Me.Original = br.ReadBytes(fileSize_)
        br.Close()
    End Sub

    Public Overrides Sub Write(fileOut As String)
        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        bw.Write(prev_data)
        For i As Integer = 0 To Palette.Length - 1
            bw.Write(Actions.ColorToBGR555(Palette(i)))
        Next
        bw.Write(next_data)

        bw.Flush()
        bw.Close()
    End Sub
End Class

Public Class RawImage
    Inherits ImageBase
    ' Unknown data - Needed to write the file
    Private prev_data As Byte(), post_data As Byte()
    Private ori_data As Byte()

    Public Sub New(file As [String], id As Integer, form As TileForm, format As ColorFormat, editable As Boolean, offset As Integer, _
     size As Integer, Optional fileName As String = "")
        MyBase.New()

        If fileName = "" Then
            Me.FileName = Path.GetFileName(file)
        Else
            Me.FileName = fileName
        End If

        Read(file, form, format, editable, offset, size)
    End Sub
    Public Sub New(file As [String], id As Integer, form As TileForm, format As ColorFormat, width As Integer, height As Integer, _
     editable As Boolean, offset As Integer, size As Integer, Optional fileName As String = "")
        MyBase.New()

        If fileName = "" Then
            Me.FileName = Path.GetFileName(file)
        Else
            Me.FileName = fileName
        End If

        Read(file, form, format, editable, offset, size)
        Me.Width = width
        Me.Height = height
    End Sub
    Public Sub New(tiles As Byte(), form As TileForm, format As ColorFormat, width As Integer, height As Integer, editable As Boolean, _
     Optional fileName As String = "")
        MyBase.New()
        Me.FileName = fileName
        Set_Tiles(tiles, width, height, format, form, editable)
    End Sub


    Public Overrides Sub Read(fileIn As String)
        Read(fileIn, TileForm.Horizontal, ColorFormat.colors16, True, 0, -1)
    End Sub
    Public Overloads Sub Read(fileIn As String, form As TileForm, format As ColorFormat, editable As Boolean, offset As Integer, fileSize As Integer)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        prev_data = br.ReadBytes(offset)

        If fileSize <= offset Then
            fileSize = CInt(br.BaseStream.Length)
        End If
        If fileSize + offset >= br.BaseStream.Length Then
            offset = CInt(br.BaseStream.Length) - fileSize
        End If
        If fileSize <= offset Then
            fileSize = CInt(br.BaseStream.Length)
        End If

        ori_data = br.ReadBytes(fileSize)
        post_data = br.ReadBytes(CInt(br.BaseStream.Length - br.BaseStream.Position))

        br.BaseStream.Position = offset

        ' Read the tiles
        Dim tiles As Byte() = br.ReadBytes(fileSize)
        br.Close()

        Set_Tiles(tiles, &H100, &HC0, format, form, editable)

        Dim size As Size = Actions.Get_Size(fileSize, BPP)
        Width = size.Width
        Height = size.Height
    End Sub

    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Dim image_size As Integer = CInt(Width * Height * BPP / 8)

        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))
        bw.Write(prev_data)
        For i As Integer = 0 To StartByte - 1
            bw.Write(ori_data(i))
        Next
        bw.Write(Tiles)
        For i As Integer = image_size + StartByte To ori_data.Length - 1
            bw.Write(ori_data(i))
        Next
        bw.Write(post_data)
        bw.Flush()
        bw.Close()
    End Sub
End Class

Public Class RawMap
    Inherits MapBase
    ' Unknown data
    Private prev_data As Byte()
    Private next_data As Byte()

    Public Sub New(file As String, id As Integer, offset As Integer, size As Integer, editable As Boolean, Optional fileName As String = "")
        MyBase.New()

        If fileName = "" Then
            Me.FileName = System.IO.Path.GetFileName(file)
        Else
            Me.FileName = fileName
        End If

        Read(file, offset, size, editable)
    End Sub
    Public Sub New(map As NTFS(), width As Integer, height As Integer, editable As Boolean, Optional fileName As String = "")
        MyBase.New(map, editable, width, height, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Read(fileIn, 0, -1, True)
    End Sub
    Public Overloads Sub Read(fileIn As String, offset As Integer, size As Integer, editable As Boolean)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        prev_data = br.ReadBytes(offset)

        Dim file_size As Integer
        If size <= 0 Then
            file_size = CInt(br.BaseStream.Length)
        Else
            file_size = size
        End If

        Dim map As NTFS() = New NTFS(file_size \ 2 - 1) {}
        For i As Integer = 0 To map.Length - 1
            map(i) = Actions.MapInfo(br.ReadUInt16())
        Next

        next_data = br.ReadBytes(CInt(br.BaseStream.Length - file_size))

        Dim width As Integer = (If(map.Length * 8 >= &H100, &H100, map.Length * 8))
        Dim height As Integer = (map.Length \ (width \ 8)) * 8

        br.Close()
        Set_Map(map, editable, width, height)
    End Sub

    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        bw.Write(prev_data)
        For i As Integer = 0 To Map.Length - 1
            bw.Write(Actions.MapInfo(Map(i)))
        Next
        bw.Write(next_data)

        bw.Flush()
        bw.Close()
    End Sub
End Class

Public Class RawSprite
    Inherits SpriteBase

    Public Sub New(banks As Bank(), blocksize As UInteger, Optional editable As Boolean = False)
        Set__Banks(banks, blocksize, editable)
    End Sub
    Public Sub New(oams As OAM(), blocksize As UInteger, Optional editable As Boolean = False)
        Dim bank As New Bank()
        bank.name = "Bank 1"
        bank.oams = oams
        Set__Banks(New Bank() {bank}, blocksize, editable)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
        Throw New NotImplementedException()
    End Sub
End Class

