'
' * Copyright (C) 2011  pleoNeX
' *
' *   This program is free software: you can redistribute it and/or modify
' *   it under the terms of the GNU General Public License as published by
' *   the Free Software Foundation, either version 3 of the License, or
' *   (at your option) any later version.
' *
' *   This program is distributed in the hope that it will be useful,
' *   but WITHOUT ANY WARRANTY; without even the implied warranty of
' *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' *   GNU General Public License for more details.
' *
' *   You should have received a copy of the GNU General Public License
' *   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
' *
' * By: pleoNeX
' * 
' 

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing


Public MustInherit Class PaletteBase
#Region "Variables"
    Protected pluginHost As IPluginHost
    Protected m_fileName As [String]
    Protected m_id As Integer
    Private m_loaded As Boolean

    Private m_original As Byte()
    Private m_startByte As Integer

    Protected m_palette As Color()()
    Private m_depth As ColorFormat
    Private m_canEdit As Boolean

    Protected obj As Object
#End Region

    Public Sub New()
        m_loaded = False
    End Sub
    Public Sub New(pal As Color()(), editable As Boolean, Optional fileName As String = "")
        Me.m_fileName = fileName
        Set_Palette(pal, editable)
    End Sub
    Public Sub New(fileIn As String, id As Integer, pluginHost As IPluginHost, Optional fileName As String = "")
        Me.pluginHost = pluginHost
        If fileName = "" Then
            Me.m_fileName = System.IO.Path.GetFileName(fileIn)
        Else
            Me.m_fileName = fileName
        End If
        Me.m_id = id

        Read(fileIn)
    End Sub
    Public Sub New(fileIn As String, id As Integer, Optional fileName As String = "")
        If fileName = "" Then
            Me.m_fileName = System.IO.Path.GetFileName(fileIn)
        Else
            Me.m_fileName = fileName
        End If
        Me.m_id = id

        Read(fileIn)
    End Sub


    Public MustOverride Sub Read(fileIn As String)
    Public MustOverride Sub Write(fileOut As String)

    Public Function Get_Image(index As Integer) As Image
        If index >= m_palette.Length Then
            Return Nothing
        End If

        Return Actions.Get_Image(m_palette(index))
    End Function

    Public Sub FillColors(maxColors As Integer, pal_index As Integer)
        FillColors(maxColors, pal_index, Color.Black)
    End Sub
    Public Sub FillColors(maxColors As Integer, pal_index As Integer, color As Color)
        Dim old_length As Integer = m_palette(pal_index).Length
        If old_length >= maxColors Then
            Return
        End If

        Dim newpal As Color() = New Color(maxColors - 1) {}
        Array.Copy(m_palette(pal_index), newpal, old_length)

        For i As Integer = old_length To maxColors - 1
            newpal(i) = color
        Next

        m_palette(pal_index) = newpal
    End Sub

    Private Sub Change_PaletteDepth(newDepth As ColorFormat)
        If newDepth = m_depth Then
            Return
        End If

        m_depth = newDepth
        If m_depth = ColorFormat.colors256 OrElse m_depth = ColorFormat.A3I5 Then
            m_palette = Actions.Palette_16To256(m_palette)
        Else
            m_palette = Actions.Palette_256To16(m_palette)
        End If
    End Sub
    Private Sub Change_StartByte(start As Integer)
        If start < 0 OrElse start >= m_original.Length Then
            Return
        End If

        m_startByte = start

        ' Get the new palette data
        Dim size As Integer = m_original.Length - start
        If size > &H2000 Then
            size = &H2000
        End If

        Dim data As Byte() = New Byte(size - 1) {}
        Array.Copy(m_original, start, data, 0, data.Length)
        ' Convert it to colors
        Dim colors As New List(Of Color)()
        colors.AddRange(Actions.BGR555ToColor(data))

        Dim num_colors As Integer = (If(m_depth = ColorFormat.colors16, &H10, &H100))
        Dim isExact As Boolean = (If(colors.Count Mod num_colors = 0, True, False))
        m_palette = New Color((colors.Count \ num_colors) + ((If(isExact, 0, 1)) - 1))() {}
        For i As Integer = 0 To m_palette.Length - 1
            Dim palette_length As Integer = If(i * num_colors + num_colors <= colors.Count, num_colors, colors.Count - i * num_colors)
            m_palette(i) = New Color(palette_length - 1) {}
            Array.Copy(colors.ToArray(), i * num_colors, m_palette(i), 0, palette_length)
        Next
    End Sub

    Public Sub Set_Palette(palette As Color()(), editable As Boolean)
        Me.m_palette = palette
        m_canEdit = editable
        If palette(0).Length > 16 Then
            m_depth = ColorFormat.colors256
        Else
            m_depth = ColorFormat.colors16
        End If

        m_loaded = True

        If m_depth = ColorFormat.colors16 AndAlso (palette.Length = 1 AndAlso palette(0).Length > &H10) Then
            Dim newColors As Color()() = New Color(palette(0).Length \ &H10 - 1)() {}
            For i As Integer = 0 To newColors.Length - 1
                Dim pal_colors As Integer = &H10
                If i * &H10 >= palette(0).Length Then
                    pal_colors = palette(0).Length - (i - 1) * &H10
                End If
                newColors(i) = New Color(pal_colors - 1) {}
                Array.Copy(palette(0), i * &H10, newColors(i), 0, pal_colors)
            Next
            Me.m_palette = newColors
        End If

        ' Convert the palette to bytes, to store the original palette
        Dim colors As New List(Of Color)()
        For i As Integer = 0 To palette.Length - 1
            colors.AddRange(palette(i))
        Next
        m_original = Actions.ColorToBGR555(colors.ToArray())
        m_startByte = 0
    End Sub
    Public Sub Set_Palette(palette As Color()(), depth As ColorFormat, editable As Boolean)
        Me.m_palette = palette
        m_canEdit = editable
        Me.m_depth = depth

        m_loaded = True

        If depth = ColorFormat.colors16 AndAlso (palette.Length = 1 AndAlso palette(0).Length > &H10) Then
            Dim newColors As Color()() = New Color(palette(0).Length \ &H10 - 1)() {}
            For i As Integer = 0 To newColors.Length - 1
                Dim pal_colors As Integer = &H10
                If i * &H10 >= palette(0).Length Then
                    pal_colors = palette(0).Length - (i - 1) * &H10
                End If
                newColors(i) = New Color(pal_colors - 1) {}
                Array.Copy(palette(0), i * &H10, newColors(i), 0, pal_colors)
            Next
            Me.m_palette = newColors
        End If

        ' Convert the palette to bytes, to store the original palette
        Dim colors As New List(Of Color)()
        For i As Integer = 0 To palette.Length - 1
            colors.AddRange(palette(i))
        Next
        m_original = Actions.ColorToBGR555(colors.ToArray())
        m_startByte = 0
    End Sub
    Public Sub Set_Palette(palette As Color(), depth As ColorFormat, editable As Boolean)
        Set_Palette(New Color()() {palette}, depth, editable)
    End Sub
    Public Sub Set_Palette(palette As Color(), index As Integer)
        Me.m_palette(index) = palette
    End Sub
    Public Sub Set_Palette(new_pal As PaletteBase)
        Me.m_palette = new_pal.Palette
        Me.m_depth = new_pal.Depth

        m_loaded = True

        ' Convert the palette to bytes, to store the original palette
        Dim colors As New List(Of Color)()
        For i As Integer = 0 To m_palette.Length - 1
            colors.AddRange(m_palette(i))
        Next
        m_original = Actions.ColorToBGR555(colors.ToArray())
        m_startByte = 0
    End Sub
    Public Sub Set_Palette(palette As Color()())
        Me.m_palette = palette
        If palette(0).Length > 16 Then
            m_depth = ColorFormat.colors256
        Else
            m_depth = ColorFormat.colors16
        End If

        m_loaded = True


        If m_depth = ColorFormat.colors16 AndAlso (palette.Length = 1 AndAlso palette(0).Length > &H10) Then
            Dim newColors As Color()() = New Color(palette(0).Length \ &H10 - 1)() {}
            For i As Integer = 0 To newColors.Length - 1
                Dim pal_colors As Integer = &H10
                If i * &H10 >= palette(0).Length Then
                    pal_colors = palette(0).Length - (i - 1) * &H10
                End If
                newColors(i) = New Color(pal_colors - 1) {}
                Array.Copy(palette(0), i * &H10, newColors(i), 0, pal_colors)
            Next
            Me.m_palette = newColors
        End If

        ' Convert the palette to bytes, to store the original palette
        Dim colors As New List(Of Color)()
        For i As Integer = 0 To palette.Length - 1
            colors.AddRange(palette(i))
        Next
        m_original = Actions.ColorToBGR555(colors.ToArray())
        m_startByte = 0
    End Sub

    Public Function Has_DuplicatedColors(index As Integer) As Boolean
        For i As Integer = 0 To m_palette(index).Length - 1
            For j As Integer = 0 To m_palette(index).Length - 1
                If j <> i AndAlso m_palette(index)(i) = m_palette(index)(j) Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function

#Region "Properties"
    Public Property StartByte() As Integer
        Get
            Return m_startByte
        End Get
        Set(value As Integer)
            Change_StartByte(Value)
        End Set
    End Property
    Public Property Depth() As ColorFormat
        Get
            Return m_depth
        End Get
        Set(value As ColorFormat)
            Change_PaletteDepth(Value)
        End Set
    End Property
    Public ReadOnly Property NumberOfPalettes() As Integer
        Get
            Return m_palette.Length
        End Get
    End Property
    Public ReadOnly Property NumberOfColors() As Integer
        Get
            If m_depth = ColorFormat.colors256 Then
                Return m_palette(0).Length
            Else
                Dim colors As Integer = 0
                For i As Integer = 0 To m_palette.Length - 1
                    colors += m_palette(i).Length
                Next
                Return colors
            End If
        End Get
    End Property
    Public ReadOnly Property Palette() As Color()()
        Get
            Return m_palette
        End Get
    End Property
    Public ReadOnly Property CanEdit() As Boolean
        Get
            Return m_canEdit
        End Get
    End Property
    Public ReadOnly Property Loaded() As Boolean
        Get
            Return m_loaded
        End Get
    End Property
    Public Property FileName() As [String]
        Get
            Return m_fileName
        End Get
        Set(value As [String])
            m_fileName = Value
        End Set
    End Property
    Public ReadOnly Property ID() As Integer
        Get
            Return m_id
        End Get
    End Property
    Public Property Original() As Byte()
        Get
            Return m_original
        End Get
        Set(value As Byte())
            m_original = Value
        End Set
    End Property
#End Region
End Class
