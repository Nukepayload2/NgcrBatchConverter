Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Public Class NANR
    Private fileName As String
    Private nanr As sNANR

    Public Sub New(file As String)
        fileName = Path.GetFileName(file)
        Read(file)
    End Sub

    Public Sub Read(file__1 As String)
        Dim br As New BinaryReader(File.OpenRead(file__1))
        nanr = New sNANR()

        ' Generic header
        nanr.header.id = br.ReadChars(4)
        nanr.header.endianess = br.ReadUInt16()
        If nanr.header.endianess = &HFFFE Then
            nanr.header.id.Reverse()
        End If
        nanr.header.constant = br.ReadUInt16()
        nanr.header.file_size = br.ReadUInt32()
        nanr.header.header_size = br.ReadUInt16()
        nanr.header.nSection = br.ReadUInt16()

        '#Region "ABNK"
        ' ABNK (Animation BaNK)
        nanr.abnk.id = br.ReadChars(4)
        nanr.abnk.length = br.ReadUInt32()
        nanr.abnk.nBanks = br.ReadUInt16()
        nanr.abnk.tFrames = br.ReadUInt16()
        nanr.abnk.constant = br.ReadUInt32()
        nanr.abnk.offset1 = br.ReadUInt32()
        nanr.abnk.offset2 = br.ReadUInt32()
        nanr.abnk.padding = br.ReadUInt64()
        nanr.abnk.anis = New sNANR.Animation(nanr.abnk.nBanks - 1) {}

        ' Bank header
        For i As Integer = 0 To nanr.abnk.nBanks - 1
            br.BaseStream.Position = &H30 + i * &H10

            Dim ani As New sNANR.Animation()
            ani.nFrames = br.ReadUInt32()
            ani.dataType = br.ReadUInt16()
            ani.unknown1 = br.ReadUInt16()
            ani.unknown2 = br.ReadUInt16()
            ani.unknown3 = br.ReadUInt16()
            ani.offset_frame = br.ReadUInt32()
            ani.frames = New sNANR.Frame(CInt(ani.nFrames - 1)) {}

            ' Frame header
            For j As Integer = 0 To CInt(ani.nFrames - 1)
                br.BaseStream.Position = &H18 + nanr.abnk.offset1 + j * &H8 + ani.offset_frame

                Dim frame As New sNANR.Frame()
                frame.offset_data = br.ReadUInt32()
                frame.unknown1 = br.ReadUInt16()
                frame.constant = br.ReadUInt16()

                ' Frame data
                br.BaseStream.Position = &H18 + nanr.abnk.offset2 + frame.offset_data
                frame.data.nCell = br.ReadUInt16()

                ani.frames(j) = frame
            Next

            nanr.abnk.anis(i) = ani
        Next
        '#End Region

        '#Region "LABL"
        br.BaseStream.Position = nanr.header.header_size + nanr.abnk.length
        Dim offsets As New List(Of UInteger)()
        Dim names As List(Of [String]) = New List(Of String)()
        nanr.labl.names = New String(nanr.abnk.nBanks - 1) {}

        nanr.labl.id = br.ReadChars(4)
        If New [String](nanr.labl.id) <> "LBAL" Then
            GoTo Tercera
        End If
        nanr.labl.section_size = br.ReadUInt32()

        ' Offset
        For i As Integer = 0 To nanr.abnk.nBanks - 1
            Dim offset As UInteger = br.ReadUInt32()
            If offset >= nanr.labl.section_size - 8 Then
                br.BaseStream.Position -= 4
                Exit For
            End If

            offsets.Add(offset)
        Next
        nanr.labl.offset = offsets.ToArray()

        ' Names
        For i As Integer = 0 To nanr.labl.offset.Length - 1
            names.Add("")
            Dim c As Byte = br.ReadByte()
            While c <> &H0
                names(i) += ChrW(c)
                c = br.ReadByte()
            End While
        Next
Tercera:
        For i As Integer = 0 To nanr.abnk.nBanks - 1
            If names.Count > i Then
                nanr.labl.names(i) = names(i)
            Else
                nanr.labl.names(i) = i.ToString()
            End If
        Next
        '#End Region

        '#Region "UEXT"
        nanr.uext.id = br.ReadChars(4)
        If New [String](nanr.uext.id) <> "TXEU" Then
            GoTo Fin
        End If

        nanr.uext.section_size = br.ReadUInt32()
        nanr.uext.unknown = br.ReadUInt32()
Fin:
        '#End Region

        br.Close()
    End Sub

    Public ReadOnly Property Names() As [String]()
        Get
            Return nanr.labl.names
        End Get
    End Property
    Public ReadOnly Property Struct() As sNANR
        Get
            Return nanr
        End Get
    End Property

    Public Structure sNANR
        Public header As NitroHeader
        Public abnk As ABNK_
        Public labl As LABL_
        Public uext As UEXT_

        Public Structure ABNK_
            Public id As Char()
            Public length As UInteger
            Public nBanks As UShort
            Public tFrames As UShort
            Public constant As UInteger
            Public offset1 As UInteger
            Public offset2 As UInteger
            Public padding As ULong
            Public anis As Animation()
        End Structure
        Public Structure Animation
            Public nFrames As UInteger
            Public dataType As UShort
            Public unknown1 As UShort
            Public unknown2 As UShort
            Public unknown3 As UShort
            Public offset_frame As UInteger
            Public frames As Frame()
        End Structure
        Public Structure Frame
            Public offset_data As UInteger
            Public unknown1 As UShort
            Public constant As UShort
            Public data As Frame_Data
        End Structure
        Public Structure Frame_Data
            Public nCell As UShort
            ' DataType 1
            Public transform As UShort()
            ' See http://nocash.emubase.de/gbatek.htm#lcdiobgrotationscaling
            Public xDisplacement As Short
            Public yDisplacement As Short
            'DataType 2 (the Displacement above)
            Public constant As UShort
            ' 0xBEEF
        End Structure

        Public Structure LABL_
            Public id As Char()
            Public section_size As UInt32
            Public offset As UInt32()
            Public names As String()
        End Structure
        Public Structure UEXT_
            Public id As Char()
            Public section_size As UInt32
            Public unknown As UInt32
        End Structure
    End Structure

End Class