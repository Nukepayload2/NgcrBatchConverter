Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing
Public Class NCCG
    Inherits ImageBase
    Private nccg As sNCCG

    Public Sub New(file As String, id As Integer, fileName As String)
        MyBase.New(file, id, FileName)
    End Sub

    Public Overrides Sub Read(file__1 As String)
        ' Image with:
        ' Horizontal as tile form
        Dim br As New BinaryReader(File.OpenRead(file__1))
        nccg = New sNCCG()

        ' Nitro generic header
        nccg.generic.id = br.ReadChars(4)
        nccg.generic.endianess = br.ReadUInt16()
        nccg.generic.constant = br.ReadUInt16()
        nccg.generic.file_size = br.ReadUInt32()
        nccg.generic.header_size = br.ReadUInt16()
        nccg.generic.nSection = br.ReadUInt16()

        ' CHAR section
        nccg.charS.type = br.ReadChars(4)
        nccg.charS.size = br.ReadUInt32()
        nccg.charS.width = br.ReadUInt32()
        nccg.charS.height = br.ReadUInt32()
        nccg.charS.depth = br.ReadUInt32()

        Dim tiles As Byte() = br.ReadBytes(CInt(nccg.charS.size - &H14))

        ' ATTR section
        nccg.attr.type = br.ReadChars(4)
        nccg.attr.size = br.ReadUInt32()
        nccg.attr.width = br.ReadUInt32()
        nccg.attr.height = br.ReadUInt32()
        nccg.attr.unknown = br.ReadBytes(CInt(nccg.attr.size) - &H10)

        ' LINK section
        nccg.link.type = br.ReadChars(4)
        nccg.link.size = br.ReadUInt32()
        nccg.link.link = New [String](br.ReadChars(CInt(nccg.link.size) - &H8))

        If nccg.generic.nSection = 4 Then
            ' CMNT section
            nccg.cmnt.type = br.ReadChars(4)
            nccg.cmnt.size = br.ReadUInt32()
            nccg.cmnt.unknown = br.ReadBytes(CInt(nccg.cmnt.size) - &H8)
        End If

        br.Close()

        Set_Tiles(tiles, CInt(nccg.charS.width) * 8, CInt(nccg.charS.height) * 8, (If(nccg.charS.depth = 0, ColorFormat.colors16, ColorFormat.colors256)), TileForm.Horizontal, False)
    End Sub

    Public Overrides Sub Write(fileOut As String, palette As PaletteBase)
        Console.WriteLine("Write Tiles - NCCG")
    End Sub

    Public Structure sNCCG
        Public generic As NitroHeader
        Public charS As [CHAR]
        Public attr As ATTR_
        Public link As LINK_
        Public cmnt As CMNT_

        Public Structure [CHAR]
            Public type As Char()
            Public size As UInteger
            Public width As UInteger
            Public height As UInteger
            Public depth As UInteger
        End Structure
        Public Structure ATTR_
            Public type As Char()
            Public size As UInteger
            Public width As UInteger
            Public height As UInteger
            Public unknown As Byte()
        End Structure
        Public Structure LINK_
            Public type As Char()
            Public size As UInteger
            Public link As String
        End Structure
        Public Structure CMNT_
            Public type As Char()
            Public size As UInteger
            Public unknown As Byte()
        End Structure
    End Structure
End Class
