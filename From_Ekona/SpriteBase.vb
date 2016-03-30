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


Public MustInherit Class SpriteBase
#Region "Variables"
    Protected pluginHost As IPluginHost
    Protected m_fileName As String
    Protected m_id As Integer
    Private m_loaded As Boolean
    Private m_canEdit As Boolean

    Private m_banks As Bank()
    Private block_size As UInteger
    Private zoom As Integer

    Private obj As Object
#End Region

#Region "Properties"
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

    Public Property Banks() As Bank()
        Get
            Return m_banks
        End Get
        Set(value As Bank())
            m_banks = value
        End Set
    End Property
    Public ReadOnly Property NumBanks() As Integer
        Get
            Return m_banks.Length
        End Get
    End Property
    Public ReadOnly Property BlockSize() As UInteger
        Get
            Return block_size
        End Get
    End Property
#End Region

    Public Sub New()
    End Sub
    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        If fileName = "" Then
            Me.m_fileName = Path.GetFileName(file)
        Else
            Me.m_fileName = fileName
        End If
        Me.m_id = id

        Read(file)
    End Sub
    Public Sub New(file As String, id As Integer, pluginHost As IPluginHost, Optional fileName As String = "")
        Me.pluginHost = pluginHost
        If fileName = "" Then
            Me.m_fileName = Path.GetFileName(file)
        Else
            Me.m_fileName = fileName
        End If
        Me.m_id = id

        Read(file)
    End Sub


    Public MustOverride Sub Read(fileIn As String)
    Public MustOverride Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)

    Public Sub Set__Banks(banks As Bank(), block_size As UInteger, editable As Boolean)
        Me.m_banks = banks
        Me.block_size = block_size
        Me.m_canEdit = editable
        m_loaded = True

        ' Sort the cell using the priority value
        For b As Integer = 0 To banks.Length - 1
            Dim cells As New List(Of OAM)()
            cells.AddRange(banks(b).oams)
            'cells.Sort()
            banks(b).oams = cells.ToArray()
        Next
    End Sub

    Public Function Get_Image(image As ImageBase, pal As PaletteBase, index As Integer, width As Integer, height As Integer, grid As Boolean, _
     cell As Boolean, number As Boolean, trans As Boolean, img As Boolean) As Image
        Return Actions.Get_Image(m_banks(index), block_size, image, pal, width, height, _
         grid, cell, number, trans, img)
    End Function
    Public Function Get_Image(image As ImageBase, pal As PaletteBase, bank As Bank, width As Integer, height As Integer, grid As Boolean, _
     cell As Boolean, number As Boolean, trans As Boolean, img As Boolean) As Image
        Return Actions.Get_Image(bank, block_size, image, pal, width, height, _
         grid, cell, number, trans, img)
    End Function
    Public Function Get_Image(image As ImageBase, pal As PaletteBase, bank As Bank, width As Integer, height As Integer, grid As Boolean, _
     cell As Boolean, number As Boolean, trans As Boolean, img As Boolean, currOAM As Integer) As Image
        Return Actions.Get_Image(bank, block_size, image, pal, width, height, _
         grid, cell, number, trans, img, currOAM)
    End Function
    Public Function Get_Image(image As ImageBase, pal As PaletteBase, index As Integer, width As Integer, height As Integer, grid As Boolean, _
     cell As Boolean, number As Boolean, trans As Boolean, img As Boolean, currOAM As Integer) As Image
        Return Actions.Get_Image(m_banks(index), block_size, image, pal, width, height, _
         grid, cell, number, trans, img, currOAM)
    End Function
    Public Function Get_Image(image As ImageBase, pal As PaletteBase, index As Integer, width As Integer, height As Integer, grid As Boolean, _
     cell As Boolean, number As Boolean, trans As Boolean, img As Boolean, currOAM As Integer, draw_index As Integer()) As Image
        Return Actions.Get_Image(m_banks(index), block_size, image, pal, width, height, _
         grid, cell, number, trans, img, currOAM, _
         1, draw_index)
    End Function

End Class

Public Structure Bank
    Public oams As OAM()
    Public name As String

    Public height As UShort
    Public width As UShort
End Structure
Public Structure OAM
    Public obj0 As Obj0
    Public obj1 As Obj1
    Public obj2 As Obj2

    Public width As UShort
    Public height As UShort
    Public num_cell As UShort
End Structure

Public Structure Obj0
    ' 16 bits
    Public yOffset As Int32
    ' Bit0-7 -> signed
    Public rs_flag As Byte
    ' Bit8 -> Rotation / Scale flag
    Public objDisable As Byte
    ' Bit9 -> if r/s == 0
    Public doubleSize As Byte
    ' Bit9 -> if r/s != 0
    Public objMode As Byte
    ' Bit10-11 -> 0 = normal; 1 = semi-trans; 2 = window; 3 = invalid
    Public mosaic_flag As Byte
    ' Bit12 
    Public depth As Byte
    ' Bit13 -> 0 = 4bit; 1 = 8bit
    Public shape As Byte
    ' Bit14-15 -> 0 = square; 1 = horizontal; 2 = vertial; 3 = invalid
End Structure
Public Structure Obj1
    ' 16 bits
    Public xOffset As Int32
    ' Bit0-8 (unsigned)
    ' If R/S == 0
    Public unused As Byte
    ' Bit9-11
    Public flipX As Byte
    ' Bit12
    Public flipY As Byte
    ' Bit13
    ' If R/S != 0
    Public select_param As Byte
    'Bit9-13 -> Parameter selection
    Public size As Byte
    ' Bit14-15
End Structure
Public Structure Obj2
    ' 16 bits
    Public tileOffset As UInteger
    ' Bit0-9
    Public priority As Byte
    ' Bit10-11
    Public index_palette As Byte
    ' Bit12-15
End Structure

