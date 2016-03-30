Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Public Class NSCR
    Inherits MapBase
    Private nscr As sNSCR

    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        nscr = New sNSCR()

        ' Generic header
        nscr.header.id = br.ReadChars(4)
        nscr.header.endianess = br.ReadUInt16()
        If nscr.header.endianess = &HFFFE Then
            nscr.header.id.Reverse()
        End If
        nscr.header.constant = br.ReadUInt16()
        nscr.header.file_size = br.ReadUInt32()
        nscr.header.header_size = br.ReadUInt16()
        nscr.header.nSection = br.ReadUInt16()

        ' Read section
        nscr.nrcs.id = br.ReadChars(4)
        nscr.nrcs.section_size = br.ReadUInt32()
        nscr.nrcs.width = br.ReadUInt16()
        nscr.nrcs.height = br.ReadUInt16()
        nscr.nrcs.padding = br.ReadUInt32()
        nscr.nrcs.data_size = br.ReadUInt32()
        nscr.nrcs.mapData = New NTFS(CInt(nscr.nrcs.data_size \ 2 - 1)) {}

        For i As Integer = 0 To nscr.nrcs.mapData.Length - 1
            nscr.nrcs.mapData(i) = Actions.MapInfo(br.ReadUInt16())
        Next

        br.Close()

        Set_Map(nscr.nrcs.mapData, True, nscr.nrcs.width, nscr.nrcs.height)
    End Sub
    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
        Update_Struct()
        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        ' Common header
        bw.Write(nscr.header.id)
        bw.Write(nscr.header.endianess)
        bw.Write(nscr.header.constant)
        bw.Write(nscr.header.file_size)
        bw.Write(nscr.header.header_size)
        bw.Write(nscr.header.nSection)

        ' SCRN section
        bw.Write(nscr.nrcs.id)
        bw.Write(nscr.nrcs.section_size)
        bw.Write(nscr.nrcs.width)
        bw.Write(nscr.nrcs.height)
        bw.Write(nscr.nrcs.padding)
        bw.Write(nscr.nrcs.data_size)

        For i As Integer = 0 To nscr.nrcs.mapData.Length - 1
            Dim npalette As Integer = nscr.nrcs.mapData(i).nPalette << 12
            Dim yFlip As Integer = nscr.nrcs.mapData(i).yFlip << 11
            Dim xFlip As Integer = nscr.nrcs.mapData(i).xFlip << 10
            Dim data As Integer = npalette + yFlip + xFlip + nscr.nrcs.mapData(i).nTile
            bw.Write(CUShort(data))
        Next

        bw.Flush()
        bw.Close()

    End Sub

    Private Sub Update_Struct()
        nscr.nrcs.width = CUShort(Width)
        nscr.nrcs.height = CUShort(Height)
        nscr.nrcs.mapData = Map
        nscr.nrcs.data_size = CUInt(Map.Length * 2)
        nscr.nrcs.section_size = CUInt(nscr.nrcs.data_size + &H14)
        nscr.header.file_size = CUInt(nscr.nrcs.section_size + &H10)
    End Sub

    Public Structure sNSCR
        ' Nintendo SCreen Resource
        Public header As NitroHeader
		Public Structure NitroHeader
    ' Generic Header in Nitro formats
            Public id As Char()
            Public endianess As UInt16
    ' 0xFFFE -> little endian
            Public constant As UInt16
    ' Always 0x0100
            Public file_size As UInt32
            Public header_size As UInt16
    ' Always 0x10
            Public nSection As UInt16
    ' Number of sections
        End Structure
        Public nrcs As NRCS_
        Public Structure NRCS_
            Public id As Char()
            ' NRCS = 0x4E524353
            Public section_size As UInt32
            Public width As UInt16
            Public height As UInt16
            Public padding As UInt32
            ' Always 0x0
            Public data_size As UInt32
            Public mapData As NTFS()
        End Structure
    End Structure
End Class
