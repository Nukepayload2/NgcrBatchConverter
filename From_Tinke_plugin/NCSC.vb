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
Public Class NCSC
    Inherits MapBase
    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(file__1 As String)
        Dim br As New BinaryReader(File.OpenRead(file__1))
        Dim ncsc As New sNCSC()

        ' Nitro generic header
        ncsc.generic.id = br.ReadChars(4)
        ncsc.generic.endianess = br.ReadUInt16()
        ncsc.generic.constant = br.ReadUInt16()
        ncsc.generic.file_size = br.ReadUInt32()
        ncsc.generic.header_size = br.ReadUInt16()
        ncsc.generic.nSection = br.ReadUInt16()

        ' SCRN section
        ncsc.scrn.id = br.ReadChars(4)
        ncsc.scrn.size = br.ReadUInt32()
        ncsc.scrn.width = CUInt(br.ReadUInt32() * 8)
        ncsc.scrn.height = CUInt(br.ReadUInt32() * 8)
        ncsc.scrn.unknown1 = br.ReadUInt32()
        ncsc.scrn.unknown2 = br.ReadUInt32()

        Dim map As NTFS() = New NTFS(CInt((ncsc.scrn.size - &H18) / 2 - 1)) {}
        For i As Integer = 0 To map.Length - 1
            map(i) = Actions.MapInfo(br.ReadUInt16())
        Next

        ' Read other sections
        For n As Integer = 1 To ncsc.generic.nSection - 1
            Dim type As New [String](br.ReadChars(4))

            Select Case type
                Case "ESCR"

                    ncsc.escr.id = "ESCR".ToCharArray()
                    ncsc.escr.size = br.ReadUInt32()
                    ncsc.escr.width = br.ReadUInt32()
                    ncsc.escr.height = br.ReadUInt32()
                    ncsc.escr.unknown = br.ReadUInt32()
                    ncsc.escr.unknown2 = br.ReadUInt32()

                    ncsc.escr.unknownData = New UInteger(CInt(ncsc.escr.width * ncsc.escr.height - 1)) {}
                    For i As Integer = 0 To ncsc.escr.unknownData.Length - 1
                        ncsc.escr.unknownData(i) = br.ReadUInt32()
                    Next
                    Exit Select

                Case "CLRF"

                    ncsc.clrf.id = "CLRF".ToCharArray()
                    ncsc.clrf.size = br.ReadUInt32()
                    ncsc.clrf.width = br.ReadUInt32()
                    ncsc.clrf.height = br.ReadUInt32()
                    ncsc.clrf.unknown = br.ReadBytes(CInt(ncsc.clrf.size) - &H10)
                    Exit Select

                Case "CLRC"

                    ncsc.clrc.id = "CLRC".ToCharArray()
                    ncsc.clrc.size = br.ReadUInt32()
                    ncsc.clrc.unknown = br.ReadBytes(CInt(ncsc.clrc.size) - &H8)
                    Exit Select

                Case "GRID"

                    ncsc.grid.id = "GRID".ToCharArray()
                    ncsc.grid.size = br.ReadUInt32()
                    ncsc.grid.unknown = br.ReadBytes(CInt(ncsc.grid.size) - &H8)
                    Exit Select

                Case "LINK"

                    ncsc.link.id = "LINK".ToCharArray()
                    ncsc.link.size = br.ReadUInt32()
                    ncsc.link.link = New String(br.ReadChars(CInt(ncsc.link.size) - &H8))
                    Exit Select

                Case "CMNT"

                    ncsc.cmnt.id = "CMNT".ToCharArray()
                    ncsc.cmnt.size = br.ReadUInt32()
                    ncsc.cmnt.unknown = br.ReadBytes(CInt(ncsc.cmnt.size) - &H8)
                    Exit Select
            End Select
        Next

        br.Close()
        Set_Map(map, False, CInt(ncsc.scrn.width), CInt(ncsc.scrn.height))
    End Sub
    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
    End Sub

    Public Structure sNCSC
        Public generic As NitroHeader
        Public scrn As SCRN_
        Public escr As ESCR_
        Public clrf As CLRF_
        Public clrc As CLRC_
        Public grid As GRID_
        Public link As LINK_
        Public cmnt As CMNT_

        Public Structure SCRN_
            Public id As Char()
            Public size As UInteger
            Public width As UInteger
            Public height As UInteger
            Public unknown1 As UInteger
            Public unknown2 As UInteger
        End Structure
        Public Structure ESCR_
            Public id As Char()
            Public size As UInteger
            Public width As UInteger
            Public height As UInteger
            Public unknown As UInteger
            Public unknown2 As UInteger

            Public unknownData As UInteger()
        End Structure
        Public Structure CLRF_
            Public id As Char()
            Public size As UInteger
            Public width As UInteger
            Public height As UInteger
            Public unknown As Byte()
        End Structure
        Public Structure CLRC_
            Public id As Char()
            Public size As UInteger
            Public unknown As Byte()
        End Structure
        Public Structure GRID_
            Public id As Char()
            Public size As UInteger
            Public unknown As Byte()
        End Structure
        Public Structure LINK_
            Public id As Char()
            Public size As UInteger
            Public link As String
        End Structure
        Public Structure CMNT_
            Public id As Char()
            Public size As UInteger
            Public unknown As Byte()
        End Structure
    End Structure
End Class