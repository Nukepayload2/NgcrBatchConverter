Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing
Public Class NCLR
    Inherits PaletteBase
    Private nclr As sNCLR

    Public Sub New(file As String, Optional id As Integer = 0, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        nclr = New sNCLR()

        Dim br As New BinaryReader(File.OpenRead(fileIn))

        ' Generic header
        nclr.header.id = br.ReadChars(4)
        nclr.header.endianess = br.ReadUInt16()
        If nclr.header.endianess = &HFFFE Then
            nclr.header.id.Reverse()
        End If
        nclr.header.constant = br.ReadUInt16()
        nclr.header.file_size = br.ReadUInt32()
        nclr.header.header_size = br.ReadUInt16()
        nclr.header.nSection = br.ReadUInt16()

        ' PLTT section
        Dim pltt As New TTLP()

        pltt.ID = br.ReadChars(4)
        pltt.length = br.ReadUInt32()
        pltt.depth = DirectCast(CByte(br.ReadUInt16()), ColorFormat)
        pltt.unknown1 = br.ReadUInt16()
        pltt.unknown2 = br.ReadUInt32()

        pltt.pal_length = br.ReadUInt32()
        If pltt.pal_length = 0 OrElse pltt.pal_length > pltt.length Then
            pltt.pal_length = CUInt(pltt.length - &H18)
        End If

        Dim colors_startOffset As UInteger = br.ReadUInt32()
        pltt.num_colors = CUInt(If((pltt.depth = ColorFormat.colors16), &H10, &H100))
        If pltt.pal_length \ 2 < pltt.num_colors Then
            pltt.num_colors = CUInt(pltt.pal_length \ 2)
        End If
        pltt.palettes = New Color(CInt(pltt.pal_length / (pltt.num_colors * 2) - 1))() {}

        br.BaseStream.Position = &H18 + colors_startOffset
        For i As Integer = 0 To pltt.palettes.Length - 1
            pltt.palettes(i) = Actions.BGR555ToColor(br.ReadBytes(CInt(pltt.num_colors) * 2))
        Next

        nclr.pltt = pltt

        ' PMCP section
        If nclr.header.nSection = 1 OrElse br.BaseStream.Position >= br.BaseStream.Length Then
            GoTo [End]
        End If

        Dim pmcp As New PMCP()
        pmcp.ID = br.ReadChars(4)
        pmcp.blockSize = br.ReadUInt32()
        pmcp.unknown1 = br.ReadUInt16()
        pmcp.unknown2 = br.ReadUInt16()
        pmcp.unknown3 = br.ReadUInt32()
        pmcp.first_palette_num = br.ReadUInt16()

        nclr.pmcp = pmcp
[End]:

        br.Close()
        Set_Palette(pltt.palettes, pltt.depth, True)
    End Sub

    Public Overrides Sub Write(fileOut As String)
        Update_Struct()
        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        bw.Write(nclr.header.id)
        bw.Write(nclr.header.endianess)
        bw.Write(nclr.header.constant)
        bw.Write(nclr.header.file_size)
        bw.Write(nclr.header.header_size)
        bw.Write(nclr.header.nSection)

        bw.Write(nclr.pltt.ID)
        bw.Write(nclr.pltt.length)
        bw.Write(CUShort(nclr.pltt.depth))
        bw.Write(nclr.pltt.unknown1)
        bw.Write(nclr.pltt.unknown2)
        bw.Write(nclr.pltt.pal_length)
        bw.Write(&H10)
        ' Colors start offset from 0x14
        For i As Integer = 0 To nclr.pltt.palettes.Length - 1
            bw.Write(Actions.ColorToBGR555(nclr.pltt.palettes(i)))
        Next

        bw.Flush()
        bw.Close()
    End Sub

    Private Sub Update_Struct()
        nclr.pltt.palettes = Palette
        nclr.pltt.depth = Depth

        nclr.pltt.pal_length = 0
        For i As Integer = 0 To nclr.pltt.palettes.Length - 1
            nclr.pltt.pal_length += CUInt(nclr.pltt.palettes(i).Length * 2)
        Next
        nclr.pltt.length = CUInt(nclr.pltt.pal_length + &H18)
        nclr.header.file_size = CUInt(nclr.pltt.length + &H10)
    End Sub

    Public Structure sNCLR
        ' Nintendo CoLor Resource
        Public header As NitroHeader
        Public pltt As TTLP
        Public pmcp As PMCP
    End Structure
    Public Structure TTLP
        ' PaLeTTe
        Public ID As Char()
        Public length As UInt32
        Public depth As ColorFormat
        Public unknown1 As UInt16
        Public unknown2 As UInt32
        ' padding?
        Public pal_length As UInt32
        Public num_colors As UInt32
        ' Number of colors
        Public palettes As Color()()
    End Structure
    Public Structure PMCP
        Public ID As Char()
        Public blockSize As UInteger
        Public unknown1 As UShort
        Public unknown2 As UShort
        ' always BEEF?
        Public unknown3 As UInteger
        Public first_palette_num As UShort
    End Structure
End Class