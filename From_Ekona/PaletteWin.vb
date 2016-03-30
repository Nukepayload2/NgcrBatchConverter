' ----------------------------------------------------------------------
' <copyright file="PaletteWin.cs" company="none">

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
' <date>28/04/2012 19:01:44</date>
' -----------------------------------------------------------------------
Imports System.IO
Imports System.Drawing


Public Class PaletteWin
    Inherits PaletteBase
    Private m_gimp_error As Boolean
    ' Error of Gimp, it reads the first colors at 0x1C instead of 0x18
    Public Sub New(file As String)
        MyBase.New()
        Read(file)
    End Sub
    Public Sub New(colors As Color())
        MyBase.New()
        Set_Palette(New Color()() {colors}, True)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))

        br.ReadChars(4)
        ' RIFF
        br.ReadUInt32()
        br.ReadChars(4)
        ' PAL
        br.ReadChars(4)
        ' data
        br.ReadUInt32()
        ' unknown, always 0x00
        br.ReadUInt16()
        ' unknown, always 0x0300
        Dim nColors As UShort = br.ReadUInt16()

        Dim colors As Color()() = New Color(0)() {}
        colors(0) = New Color(nColors - 1) {}
        For j As Integer = 0 To nColors - 1
            Dim newColor As Color = Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte())
            br.ReadByte()
            ' always 0x00
            colors(0)(j) = newColor
        Next

        br.Close()
        Set_Palette(colors, True)
    End Sub

    Public Overrides Sub Write(fileOut As String)
        If File.Exists(fileOut) Then
            File.Delete(fileOut)
        End If

        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        bw.Write(New Char() {"R"c, "I"c, "F"c, "F"c})
        ' "RIFF"
        bw.Write(CUInt(&H10 + Palette(0).Length * 4))
        ' file_length - 8
        bw.Write(New Char() {"P"c, "A"c, "L"c, " "c})
        ' "PAL "
        bw.Write(New Char() {"d"c, "a"c, "t"c, "a"c})
        ' "data"
        bw.Write(CUInt(Palette(0).Length) * 4 + 4)
        ' data_size = file_length - 0x14
        bw.Write(CUShort(&H300))
        ' version = 00 03
        bw.Write(CUShort(Palette(0).Length))
        ' num_colors
        If m_gimp_error Then
            bw.Write(CUInt(&H0))
        End If
        ' Error in Gimp 2.8
        For i As Integer = 0 To Palette(0).Length - 1
            bw.Write(Palette(0)(i).R)
            bw.Write(Palette(0)(i).G)
            bw.Write(Palette(0)(i).B)
            bw.Write(CByte(&H0))
            bw.Flush()
        Next

        bw.Close()
    End Sub


    Public Property Gimp_Error() As Boolean
        Get
            Return m_gimp_error
        End Get
        Set(value As Boolean)
            m_gimp_error = Value
        End Set
    End Property
End Class

