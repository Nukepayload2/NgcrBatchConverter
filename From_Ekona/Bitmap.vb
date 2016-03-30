' ----------------------------------------------------------------------
' <copyright file="Bitmap.cs" company="none">

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
' <date>28/04/2012 14:28:51</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing


Public Class BMP
    Inherits ImageBase
    Private m_palette As PaletteBase
    Dim divisor As Integer '注意！新加进去的
    Public Sub New(file As String)
        MyBase.New(file, -1)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        If New [String](br.ReadChars(2)) <> "BM" Then
            Throw New NotSupportedException()
        End If

        br.BaseStream.Position = &HA
        Dim offsetImagen As UInteger = br.ReadUInt32()

        br.BaseStream.Position += &H4
        Dim width As UInteger = br.ReadUInt32()
        Dim height As UInteger = br.ReadUInt32()

        br.BaseStream.Position += &H2
        Dim bpp As UInteger = br.ReadUInt16()
        Dim format As ColorFormat
        If bpp = &H4 Then
            format = ColorFormat.colors16
        ElseIf bpp = &H8 Then
            format = ColorFormat.colors256
        Else
            Throw New NotSupportedException()
        End If

        Dim compression As UInteger = br.ReadUInt32()
        Dim data_size As UInteger = br.ReadUInt32()

        br.BaseStream.Position += &H8
        Dim num_colors As UInteger = br.ReadUInt32()

        If num_colors = &H0 Then
            num_colors = CUInt(If(bpp = &H4, &H10, &H100))
        End If

        br.BaseStream.Position += &H4
        Dim colors As Color()() = New Color(0)() {}
        colors(0) = New Color(CInt(num_colors - 1)) {}
        For i As Integer = 0 To CInt(num_colors - 1)
            Dim color__1 As Byte() = br.ReadBytes(4)
            colors(0)(i) = Color.FromArgb(color__1(2), color__1(1), color__1(0))
        Next
        ' Get the colors with BGR555 encoding (not all colours from bitmap are allowed)
        Dim temp As Byte() = Actions.ColorToBGR555(colors(0))
        colors(0) = Actions.BGR555ToColor(temp)
        m_palette = New RawPalette(colors, False, format)

        Dim tiles As Byte() = New Byte(CInt(width * height - 1)) {}
        br.BaseStream.Position = offsetImagen

        Select Case bpp
            Case 4
                Dim divisor As Integer = CInt(width) \ 2
                If width Mod 4 <> 0 Then
                    Dim res As Integer
                    Math.DivRem(CInt(width) \ 2, 4, res)
                    divisor = CInt(width) \ 2 + (4 - res)
                End If

                tiles = New Byte(tiles.Length * 2 - 1) {}
                For h As Integer = CInt(height) - 1 To 0 Step -1
                    For w As Integer = 0 To CInt(width - 1) Step 2
                        Dim b As Byte = br.ReadByte()
                        tiles(CInt(w + h * width)) = CByte(b >> 4)

                        If w + 1 <> width Then
                            tiles(CInt(w + 1 + h * width)) = CByte(b And &HF)
                        End If
                    Next
                    br.ReadBytes(CInt(Math.Truncate(divisor - (CSng(width) / 2))))
                Next
                tiles = BitsConverter.Bits4ToByte(tiles)
                Exit Select
            Case 8
                divisor = CInt(width)
                If width Mod 4 <> 0 Then
                    Dim res As Integer
                    Math.DivRem(CInt(width), 4, res)
                    divisor = CInt(width) + (4 - res)
                End If

                For h As Integer = CInt(height) - 1 To 0 Step -1
                    For w As Integer = 0 To CInt(width - 1)
                        tiles(CInt(w + h * width)) = br.ReadByte()
                    Next
                    br.ReadBytes(divisor - CInt(width))
                Next
                Exit Select
        End Select

        br.Close()
        Set_Tiles(tiles, CInt(width), CInt(height), format, TileForm.Lineal, False)
    End Sub

    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Throw New NotImplementedException()
    End Sub

    Public ReadOnly Property Palette() As PaletteBase
        Get
            Return m_palette
        End Get
    End Property
End Class
