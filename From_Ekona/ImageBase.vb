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
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO



Public MustInherit Class ImageBase

#Region "Variable definition"
    Protected pluginHost As IPluginHost
    ' Optional
    Protected m_fileName As String
    Protected m_id As Integer
    Private m_loaded As Boolean

    Private m_original As Byte()
    Private m_startByte As Integer
    Private m_zoom As Integer = 1

    Private m_tiles As Byte()
    Private tilePal As Byte()
    Private m_width As Integer, m_height As Integer
    Private format As ColorFormat
    Private tileForm As TileForm
    Private tile_size As Integer
    ' Pixels heigth
    Private m_bpp As Integer
    Private m_canEdit As Boolean

    Private obj As Object
#End Region

    Public Sub New()
    End Sub
    Public Sub New(tiles As Byte(), width As Integer, height As Integer, format As ColorFormat, tileForm As TileForm, editable As Boolean, _
     Optional fileName As String = "")
        Me.m_fileName = fileName
        Set_Tiles(tiles, width, height, format, tileForm, editable)
    End Sub
    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        Me.m_id = id
        If fileName = "" Then
            Me.m_fileName = Path.GetFileName(file)
        Else
            Me.m_fileName = fileName
        End If

        Read(file)
    End Sub
    Public Sub New(file As String, id As Integer, pluginHost As IPluginHost, Optional fileName As String = "")
        Me.m_id = id
        Me.pluginHost = pluginHost
        If fileName = "" Then
            Me.m_fileName = Path.GetFileName(file)
        Else
            Me.m_fileName = fileName
        End If

        Read(file)
    End Sub


    Public Function Get_Image(palette As PaletteBase) As Image
        palette.Depth = format
        Dim pal_colors As Color()() = palette.Palette

        Dim img_tiles As Byte()
        If tileForm = TileForm.Horizontal Then
            If m_height < tile_size Then
                m_height = tile_size
            End If
            img_tiles = Actions.LinealToHorizontal(m_tiles, m_width, m_height, m_bpp, tile_size)
            tilePal = Actions.LinealToHorizontal(tilePal, m_width, m_height, 8, tile_size)
        Else
            img_tiles = m_tiles
        End If

        Return Actions.Get_Image(img_tiles, tilePal, pal_colors, format, m_width, m_height)
    End Function

    Public MustOverride Sub Read(fileIn As String)
    Public MustOverride Sub Write(fileOut As String, palette As PaletteBase)

    Public Sub Change_StartByte(start As Integer)
        If start < 0 OrElse start >= m_original.Length Then
            Return
        End If

        m_startByte = start

        m_tiles = New Byte(m_original.Length - start - 1) {}
        Array.Copy(m_original, start, m_tiles, 0, m_tiles.Length)
        tilePal = New Byte(m_tiles.Length * (tile_size \ m_bpp) - 1) {}
    End Sub

    Public Sub Set_Tiles(tiles As Byte(), width__1 As Integer, height__2 As Integer, format As ColorFormat, form As TileForm, editable As Boolean, _
     Optional tile_size As Integer = 8)
        Me.m_tiles = tiles
        Me.format = format
        Me.tileForm = form
        Me.m_canEdit = editable
        Me.tile_size = tile_size

        Width = width__1
        Height = height__2

        m_zoom = 1
        'startByte = 0;
        m_loaded = True

        m_bpp = 8
        If format = ColorFormat.colors16 Then
            m_bpp = 4
        ElseIf format = ColorFormat.colors2 Then
            m_bpp = 1
        ElseIf format = ColorFormat.colors4 Then
            m_bpp = 2
        ElseIf format = ColorFormat.direct Then
            m_bpp = 16
        ElseIf format = ColorFormat.BGRA32 OrElse format = ColorFormat.ABGR32 Then
            m_bpp = 32
        End If

        tilePal = New Byte(tiles.Length * (tile_size \ m_bpp) - 1) {}

        ' Get the original data for changes in startByte
        m_original = DirectCast(tiles.Clone(), Byte())
    End Sub
    Public Sub Set_Tiles(new_img As ImageBase)
        Me.m_tiles = new_img.Tiles
        Me.format = new_img.FormatColor
        Me.tileForm = new_img.FormTile
        Me.tile_size = new_img.tile_size

        Width = new_img.Width
        Height = new_img.Height

        m_zoom = 1
        m_startByte = 0
        m_loaded = True

        m_bpp = 8
        If format = ColorFormat.colors16 Then
            m_bpp = 4
        ElseIf format = ColorFormat.colors2 Then
            m_bpp = 1
        ElseIf format = ColorFormat.colors4 Then
            m_bpp = 2
        ElseIf format = ColorFormat.direct Then
            m_bpp = 16
        ElseIf format = ColorFormat.BGRA32 OrElse format = ColorFormat.ABGR32 Then
            m_bpp = 32
        End If

        tilePal = New Byte(m_tiles.Length * (tile_size \ m_bpp) - 1) {}

        ' Get the original data for changes in startByte
        m_original = DirectCast(m_tiles.Clone(), Byte())
    End Sub
    Public Sub Set_Tiles(tiles As Byte())
        Me.m_tiles = tiles

        m_zoom = 1
        m_startByte = 0
        m_loaded = True

        tilePal = New Byte(tiles.Length * (tile_size \ m_bpp) - 1) {}

        ' Get the original data for changes in startByte
        m_original = DirectCast(tiles.Clone(), Byte())
    End Sub


#Region "Properties"
    Public ReadOnly Property ID() As Integer
        Get
            Return m_id
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
    Public ReadOnly Property Loaded() As Boolean
        Get
            Return m_loaded
        End Get
    End Property
    Public ReadOnly Property CanEdit() As Boolean
        Get
            Return m_canEdit
        End Get
    End Property

    Public Property Zoom() As Integer
        Get
            Return m_zoom
        End Get
        Set(value As Integer)
            m_zoom = Value
        End Set
    End Property
    Public Property StartByte() As Integer
        Get
            Return m_startByte
        End Get
        Set(value As Integer)
            Change_StartByte(Value)
        End Set
    End Property
    Public Property Height() As Integer
        Get
            Return m_height
        End Get
        Set(value As Integer)
            m_height = Value
            If tileForm = tileForm.Horizontal OrElse tileForm = tileForm.Vertical Then
                If Me.m_height < Me.tile_size Then
                    Me.m_height = Me.tile_size
                End If
                If Me.m_height Mod Me.tile_size <> 0 Then
                    Me.m_height += Me.tile_size - (Me.m_height Mod Me.tile_size)
                End If
            End If
        End Set
    End Property
    Public Property Width() As Integer
        Get
            Return m_width
        End Get
        Set(value As Integer)
            m_width = Value
            If tileForm = tileForm.Horizontal OrElse tileForm = tileForm.Vertical Then
                If Me.m_width < Me.tile_size Then
                    Me.m_width = Me.tile_size
                End If
                If Me.m_width Mod Me.tile_size <> 0 Then
                    Me.m_width += Me.tile_size - (Me.m_width Mod Me.tile_size)
                End If
            End If
        End Set
    End Property
    Public Property FormatColor() As ColorFormat
        Get
            Return format
        End Get
        Set(value As ColorFormat)
            format = Value
            If format = ColorFormat.colors16 Then
                m_bpp = 4
            ElseIf format = ColorFormat.colors2 Then
                m_bpp = 1
            ElseIf format = ColorFormat.colors4 Then
                m_bpp = 2
            ElseIf format = ColorFormat.direct Then
                m_bpp = 16
            ElseIf format = ColorFormat.BGRA32 OrElse format = ColorFormat.ABGR32 Then
                m_bpp = 32
            Else
                m_bpp = 8
            End If

            Array.Resize(tilePal, m_tiles.Length * (tile_size \ m_bpp))
        End Set
    End Property
    Public Property FormTile() As TileForm
        Get
            Return tileForm
        End Get
        Set(value As TileForm)
            tileForm = Value
        End Set
    End Property
    Public ReadOnly Property Tiles() As Byte()
        Get
            Return m_tiles
        End Get
    End Property
    Public Property TilesPalette() As Byte()
        Get
            Return tilePal
        End Get
        Set(value As Byte())
            tilePal = Value
        End Set
    End Property
    Public ReadOnly Property BPP() As Integer
        Get
            Return m_bpp
        End Get
    End Property
    Public Property TileSize() As Integer
        Get
            Return tile_size
        End Get
        Set(value As Integer)
            tile_size = Value
            Array.Resize(tilePal, m_tiles.Length * (tile_size \ m_bpp))
        End Set
    End Property
    Public ReadOnly Property Original() As Byte()
        Get
            Return m_original
        End Get
    End Property
#End Region
End Class

Public Class TestImage
    Inherits ImageBase
    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Throw New NotImplementedException()
    End Sub
    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Throw New NotImplementedException()
    End Sub
End Class

