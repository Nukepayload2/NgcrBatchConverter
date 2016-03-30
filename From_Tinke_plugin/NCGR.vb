Imports System.Collections.Generic
Imports System.Linq
Imports System.IO
Imports System.Text
Public Class NCGR
    Inherits ImageBase
    Private ncgr As sNCGR

    Public Sub New(file As String, Optional id As Integer = 0, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        ncgr = New sNCGR()

        ' Read the common header
        ncgr.header.id = br.ReadChars(4)
        ncgr.header.endianess = br.ReadUInt16()
        If ncgr.header.endianess = &HFFFE Then
            ncgr.header.id.Reverse()
        End If
        ncgr.header.constant = br.ReadUInt16()
        ncgr.header.file_size = br.ReadUInt32()
        ncgr.header.header_size = br.ReadUInt16()
        ncgr.header.nSection = br.ReadUInt16()

        ' Read the first section: CHAR (CHARacter data)
        ncgr.rahc.id = br.ReadChars(4)
        ncgr.rahc.size_section = br.ReadUInt32()
        ncgr.rahc.nTilesY = br.ReadUInt16()
        ncgr.rahc.nTilesX = br.ReadUInt16()
        ncgr.rahc.depth = DirectCast(CByte(br.ReadUInt32()), ColorFormat)
        ncgr.rahc.unknown1 = br.ReadUInt16()
        ncgr.rahc.unknown2 = br.ReadUInt16()
        ncgr.rahc.tiledFlag = br.ReadUInt32()
        If (ncgr.rahc.tiledFlag And &HFF) = &H0 Then
            ncgr.order = TileForm.Horizontal
        Else
            ncgr.order = TileForm.Lineal
        End If

        ncgr.rahc.size_tiledata = br.ReadUInt32()
        ncgr.rahc.unknown3 = br.ReadUInt32()
        ncgr.rahc.data = br.ReadBytes(CInt(ncgr.rahc.size_tiledata))

        If ncgr.rahc.nTilesX <> &HFFFF Then
            ncgr.rahc.nTilesX = CUShort(ncgr.rahc.nTilesX * 8)
            ncgr.rahc.nTilesY = CUShort(ncgr.rahc.nTilesY * 8)
        End If

        If ncgr.header.nSection = 2 AndAlso br.BaseStream.Position < br.BaseStream.Length Then
            ' If there isn't SOPC section
            ' Read the second section: SOPC
            ncgr.sopc.id = br.ReadChars(4)
            ncgr.sopc.size_section = br.ReadUInt32()
            ncgr.sopc.unknown1 = br.ReadUInt32()
            ncgr.sopc.charSize = br.ReadUInt16()
            ncgr.sopc.nChar = br.ReadUInt16()
        End If

        br.Close()
        Set_Tiles(ncgr.rahc.data, ncgr.rahc.nTilesX, ncgr.rahc.nTilesY, ncgr.rahc.depth, ncgr.order, True)

        If ncgr.rahc.nTilesX = &HFFFF Then
            Dim size As System.Drawing.Size = Actions.Get_Size(CInt(ncgr.rahc.size_tiledata), BPP)
            ncgr.rahc.nTilesX = CUShort(size.Width)
            ncgr.rahc.nTilesY = CUShort(size.Height)
            Height = size.Height
            Width = size.Width
        End If
    End Sub
    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Update_Struct()
        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        ' Common header
        bw.Write(ncgr.header.id)
        bw.Write(ncgr.header.endianess)
        bw.Write(ncgr.header.constant)
        bw.Write(ncgr.header.file_size)
        bw.Write(ncgr.header.header_size)
        bw.Write(ncgr.header.nSection)

        ' RAHC section
        bw.Write(ncgr.rahc.id)
        bw.Write(ncgr.rahc.size_section)
        bw.Write(ncgr.rahc.nTilesY)
        bw.Write(ncgr.rahc.nTilesX)
        bw.Write(CUInt(ncgr.rahc.depth))
        bw.Write(ncgr.rahc.unknown1)
        bw.Write(ncgr.rahc.unknown2)
        bw.Write(ncgr.rahc.tiledFlag)
        bw.Write(ncgr.rahc.size_tiledata)
        bw.Write(ncgr.rahc.unknown3)
        bw.Write(ncgr.rahc.data)

        ' SOPC section
        If ncgr.header.nSection = 2 Then
            bw.Write(ncgr.sopc.id)
            bw.Write(ncgr.sopc.size_section)
            bw.Write(ncgr.sopc.unknown1)
            bw.Write(ncgr.sopc.charSize)
            bw.Write(ncgr.sopc.nChar)
        End If

        bw.Flush()
        bw.Close()
    End Sub

    Private Sub Update_Struct()
        ncgr.rahc.nTilesX = CUShort(Width / 8)
        ncgr.rahc.nTilesY = CUShort(Height / 8)

        ncgr.rahc.data = Tiles
        If Me.FormTile = TileForm.Lineal AndAlso ncgr.order = TileForm.Horizontal Then
            ncgr.rahc.data = Actions.HorizontalToLineal(Tiles, ncgr.rahc.nTilesX, ncgr.rahc.nTilesY, BPP, TileSize)
            Set_Tiles(ncgr.rahc.data, Me.Width, Me.Height, Me.FormatColor, ncgr.order, True)
        End If

        ncgr.rahc.depth = FormatColor

        ncgr.rahc.size_tiledata = CUInt(ncgr.rahc.data.Length)
        ncgr.rahc.size_section = CUInt(ncgr.rahc.size_tiledata + &H24)
        ncgr.header.file_size = CUInt(ncgr.rahc.size_section + &H10)
    End Sub

    Public Structure sNCGR
        ' Nintendo Character Graphic Resource
        Public header As NitroHeader
        Public rahc As RAHC
        Public sopc As SOPC
        Public order As TileForm
        Public other As [Object]
        Public id As UInt32
    End Structure
    Public Structure RAHC
        ' CHARacter
        Public id As Char()
        ' Always RAHC = 0x52414843
        Public size_section As UInt32
        Public nTilesY As UInt16
        Public nTilesX As UInt16
        Public depth As ColorFormat
        Public unknown1 As UInt16
        Public unknown2 As UInt16
        Public tiledFlag As UInt32
        Public size_tiledata As UInt32
        Public unknown3 As UInt32
        ' Always 0x18 (24) (data offset?)
        Public data As Byte()
        ' image data
    End Structure
    Public Structure SOPC
        ' Unknown section
        Public id As Char()
        Public size_section As UInt32
        Public unknown1 As UInt32
        Public charSize As UInt16
        Public nChar As UInt16
    End Structure

End Class