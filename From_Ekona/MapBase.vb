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


Public MustInherit Class MapBase

#Region "Variables"
    Protected pluginHost As IPluginHost
    Protected m_id As Integer
    Protected m_fileName As String
    Private m_loaded As Boolean

    Private original As Byte()
    Private m_startByte As Integer

    Private m_map As NTFS()
    Private m_width As Integer, m_height As Integer
    Private m_canEdit As Boolean

    Private obj As Object
#End Region

    Public Sub New()
    End Sub
    Public Sub New(fileIn As String, id As Integer, Optional fileName As String = "")
        Me.m_id = id
        If fileName = "" Then
            Me.m_fileName = System.IO.Path.GetFileName(fileIn)
        Else
            Me.m_fileName = fileName
        End If

        Read(fileIn)
    End Sub
    Public Sub New(fileIn As String, id As Integer, pluginHost As IPluginHost, Optional fileName As String = "")
        Me.pluginHost = pluginHost
        Me.m_id = id
        If fileName = "" Then
            Me.m_fileName = System.IO.Path.GetFileName(fileIn)
        Else
            Me.m_fileName = fileName
        End If

        Read(fileIn)
    End Sub
    Public Sub New(mapInfo As NTFS(), editable As Boolean, Optional width As Integer = 0, Optional height As Integer = 0, Optional fileName As String = "")
        Me.m_fileName = fileName
        Set_Map(mapInfo, editable, width, height)
    End Sub

    Public MustOverride Sub Read(fileIn As String)
    Public MustOverride Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)

    Public Function Get_Image(image As ImageBase, palette As PaletteBase) As Image
        If image.FormTile = TileForm.Lineal Then
            image.FormTile = TileForm.Horizontal
        End If

        Dim tiles As Byte(), tile_pal As Byte() = Nothing
        Dim currMap As NTFS() = DirectCast(m_map.Clone(), NTFS())
        tiles = Actions.Apply_Map(currMap, image.Tiles, tile_pal, image.BPP, image.TileSize)

        Dim newImage As ImageBase = New TestImage()
        newImage.Set_Tiles(tiles, image.Width, image.Height, image.FormatColor, image.FormTile, image.CanEdit, _
         image.TileSize)
        newImage.TilesPalette = tile_pal
        newImage.Zoom = image.Zoom

        If m_height <> 0 Then
            newImage.Height = m_height
        End If
        If m_width <> 0 Then
            newImage.Width = m_width
        End If

        Return newImage.Get_Image(palette)
    End Function

    Public Sub Set_Map(mapInfo As NTFS(), editable As Boolean, Optional width As Integer = 0, Optional height As Integer = 0)
        Me.m_map = mapInfo
        Me.m_canEdit = editable
        Me.m_width = width
        Me.m_height = height

        m_startByte = 0
        m_loaded = True

        ' Get the original byte data
        Dim data As List(Of Byte) = New List(Of Byte)()
        For i As Integer = 0 To m_map.Length - 1
            data.AddRange(BitConverter.GetBytes(Actions.MapInfo(m_map(i))))
        Next
        original = data.ToArray()
    End Sub
    Public Sub Set_Map(new_map As MapBase)
        Me.m_map = new_map.Map
        Me.m_width = new_map.Width
        Me.m_height = new_map.Height

        m_startByte = 0
        m_loaded = True

        ' Get the original byte data
        Dim data As List(Of Byte) = New List(Of Byte)()
        For i As Integer = 0 To m_map.Length - 1
            data.AddRange(BitConverter.GetBytes(Actions.MapInfo(m_map(i))))
        Next
        original = data.ToArray()
    End Sub


    Private Sub Change_StartByte(newStart As Integer)
        If newStart < 0 OrElse newStart = m_startByte OrElse newStart >= original.Length Then
            Return
        End If
        m_startByte = newStart

        Dim newData As Byte() = New Byte(original.Length - m_startByte - 1) {}
        Array.Copy(original, m_startByte, newData, 0, newData.Length)
        m_map = New NTFS(newData.Length \ 2 - 1) {}

        For i As Integer = 0 To m_map.Length - 1
            m_map(i) = Actions.MapInfo(BitConverter.ToUInt16(newData, i * 2))
        Next
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
        End Set
    End Property
    Public Property Width() As Integer
        Get
            Return m_width
        End Get
        Set(value As Integer)
            m_width = Value
        End Set
    End Property
    Public ReadOnly Property Map() As NTFS()
        Get
            Return m_map
        End Get
    End Property
#End Region

End Class
