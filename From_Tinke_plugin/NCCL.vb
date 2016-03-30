Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing
Public Class NCCL
    Inherits PaletteBase
    Private nccl As sNCCL

    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(file__1 As String)
        Dim br As New BinaryReader(File.OpenRead(file__1))
        nccl = New sNCCL()

        ' Generic header
        nccl.generic.id = br.ReadChars(4)
        ' Should be NCCL
        nccl.generic.endianess = br.ReadUInt16()
        nccl.generic.constant = br.ReadUInt16()
        nccl.generic.file_size = br.ReadUInt32()
        nccl.generic.header_size = br.ReadUInt16()
        nccl.generic.nSection = br.ReadUInt16()


        ' PALT (PALeTte) section
        nccl.palt.type = br.ReadChars(4)
        ' Should be PALT
        nccl.palt.size = br.ReadUInt32()
        nccl.palt.num_colors = br.ReadUInt32()
        ' Number of colors per palette
        nccl.palt.num_palette = br.ReadUInt32()

        Dim palette As Color()() = New Color(CInt(nccl.palt.num_palette - 1))() {}
        For i As Integer = 0 To CInt(nccl.palt.num_palette - 1)
            ' Each color is 2bytes (BGR555 encoding)
            palette(i) = Actions.BGR555ToColor(br.ReadBytes(CInt(nccl.palt.num_colors) * 2))
        Next

        ' CMNT section
        If nccl.generic.nSection = 2 Then
            nccl.cmnt.type = br.ReadChars(4)
            nccl.cmnt.size = br.ReadUInt32()
            nccl.cmnt.unknown = br.ReadBytes(CInt(nccl.cmnt.size) - 8)
        End If

        br.Close()

        Set_Palette(palette, False)
        Me.FileName = Path.GetFileName(file__1)
    End Sub

    Public Overrides Sub Write(fileOut As String)
        Throw New NotSupportedException("Not supported")
    End Sub

    Public Structure sNCCL
        Public generic As NitroHeader
        ' Generic header
        Public palt As PALT_
        Public cmnt As CMNT_

        Public Structure PALT_
            Public type As Char()
            ' Should be PALT
            Public size As UInteger
            Public num_colors As UInteger
            ' Number of colors per palette
            Public num_palette As UInteger
        End Structure
        Public Structure CMNT_
            Public type As Char()
            ' Should be CMNT
            Public size As UInteger
            ' Should be 0x0C
            Public unknown As Byte()
        End Structure
    End Structure
End Class