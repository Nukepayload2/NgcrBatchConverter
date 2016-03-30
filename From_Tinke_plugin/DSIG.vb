' ----------------------------------------------------------------------
' <copyright file="DSIG.cs" company="none">

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
' <date>06/07/2012 2:19:41</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Drawing
Imports System.IO
Public Class DSIG
    Inherits ImageBase
    Private dsig As sDSIG
    Private m_palette As PaletteBase

    Public Sub New(file As sFile)
        MyBase.New(file.path, file.id, file.name)
    End Sub
    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        dsig = New sDSIG()

        dsig.id = br.ReadChars(4)
        dsig.type = br.ReadByte()
        If dsig.type <> &H2 Then

            dsig.unk4 = br.ReadByte()
        End If
        dsig.num_colors = br.ReadByte()
        ' Number of palettes of 16 colors
        dsig.unk1 = br.ReadByte()
        dsig.unk2 = br.ReadUInt16()
        dsig.unk3 = br.ReadUInt16()

        Dim depth As ColorFormat = (If(dsig.unk1 = 0, ColorFormat.colors16, ColorFormat.colors256))
        If dsig.unk1 <> 0 Then
            MessageBox.Show("Found different depth!")
        End If
        Dim form As TileForm = (If(dsig.unk4 = &H10, TileForm.Horizontal, TileForm.Lineal))

        Dim colors As Color() = Actions.BGR555ToColor(br.ReadBytes(dsig.num_colors * 2))
        m_palette = New RawPalette(colors, False, depth, FileName)

        Dim tiles As Byte() = br.ReadBytes(CInt(br.BaseStream.Length - br.BaseStream.Position))
        Set_Tiles(tiles, &H100, &HC0, depth, form, False)

        br.Close()
    End Sub

    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Throw New NotImplementedException()
    End Sub

    Public ReadOnly Property Palette() As PaletteBase
        Get
            Return m_palette
        End Get
    End Property

    Public Structure sDSIG
        Public id As Char()
        Public type As Byte
        Public unk4 As Byte
        Public num_colors As Byte
        Public unk1 As Byte
        Public unk2 As UShort
        Public unk3 As UShort
    End Structure
End Class