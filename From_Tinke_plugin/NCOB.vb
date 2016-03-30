Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Public Class NCOB
    Inherits SpriteBase
    Private ncob As sNCOB
    Private img As ImageBase

    Public Sub New(file As String, id As Integer, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        ncob = New sNCOB()
        Dim br As New BinaryReader(File.OpenRead(fileIn))

        ' Read the header
        ncob.generic.id = br.ReadChars(4)
        ncob.generic.endianess = br.ReadUInt16()
        ncob.generic.constant = br.ReadUInt16()
        ncob.generic.file_size = br.ReadUInt32()
        ncob.generic.header_size = br.ReadUInt16()
        ncob.generic.nSection = br.ReadUInt16()

        For i As Integer = 0 To ncob.generic.nSection - 1
            Dim type As String = New String(br.ReadChars(4))

            Select Case type
                Case "CELL"
                    ncob.cell.type = "CELL".ToCharArray()
                    ncob.cell.size = br.ReadUInt32()
                    ncob.cell.num_banks = br.ReadUInt32()
                    ncob.cell.banks = New Bank(CInt(ncob.cell.num_banks - 1)) {}

                    For b As Integer = 0 To CInt(ncob.cell.num_banks - 1)
                        ncob.cell.banks(b) = New Bank()
                        ncob.cell.banks(b).oams = New OAM(CInt(br.ReadUInt32() - 1)) {}

                        For o As Integer = 0 To ncob.cell.banks(b).oams.Length - 1
                            Dim oam As New OAM()
                            oam.obj1.xOffset = br.ReadInt16()
                            oam.obj0.yOffset = br.ReadInt16()
                            Dim unk1 As UShort = br.ReadUInt16()
                            If unk1 <> 0 Then
                                MessageBox.Show("Unk1 different to 0")
                            End If
                            oam.obj1.flipX = br.ReadByte()
                            oam.obj1.flipY = br.ReadByte()
                            Dim unk2 As UInteger = br.ReadUInt32()
                            If unk2 <> 0 Then
                                MessageBox.Show("Unk2 different to 0")
                            End If
                            oam.obj0.shape = br.ReadByte()
                            oam.obj1.size = br.ReadByte()
                            oam.obj2.priority = br.ReadByte()
                            oam.obj2.index_palette = br.ReadByte()
                            oam.obj2.tileOffset = br.ReadUInt32()

                            oam.width = CUShort(Actions.Get_OAMSize(oam.obj0.shape, oam.obj1.size).Width)
                            oam.height = CUShort(Actions.Get_OAMSize(oam.obj0.shape, oam.obj1.size).Height)
                            oam.num_cell = CUShort(o)

                            ncob.cell.banks(b).oams(o) = oam
                        Next
                    Next
                    Exit Select

                Case "CHAR"
                    ncob.chars.type = "CHAR".ToCharArray()
                    ncob.chars.size = br.ReadUInt32()
                    ncob.chars.unknown = br.ReadUInt32()
                    ncob.chars.data_size = br.ReadUInt32()
                    ncob.chars.data = br.ReadBytes(CInt(ncob.chars.data_size))

                    Exit Select
                Case Else

                    Dim size As UInteger = br.ReadUInt32()
                    br.BaseStream.Position += size - 8
                    Exit Select
            End Select
        Next

        br.Close()

        img = New RawImage(ncob.chars.data, TileForm.Horizontal, ColorFormat.colors16, &H20, ncob.chars.data.Length \ &H20, False)
        Set__Banks(ncob.cell.banks, 0, False)
    End Sub

    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
        Throw New NotImplementedException()
    End Sub

    Public ReadOnly Property Image_() As ImageBase
        Get
            Return img
        End Get
    End Property

    Public Structure sNCOB
        Public generic As NitroHeader
        Public cell As CELL_
        Public chars As [CHAR]
        Public grp As GRP_
        Public anim As ANIM_
        Public actl As ACTL_
        Public mode As MODE_
        Public labl As LABL_
        Public cmnt As CMNT_
        Public ccmt As CCMT_
        Public ecmt As ECMT_
        Public fcmt As FCMT_
        Public clbl As CLBL_
        Public extr As EXTR_
        Public link As LINK_

        Public Structure CELL_
            Public type As Char()
            Public size As UInteger
            Public num_banks As UInteger
            Public banks As Bank()
        End Structure
        Public Structure [CHAR]
            Public type As Char()
            Public size As UInteger
            Public unknown As UInteger
            Public data_size As UInteger
            Public data As Byte()
        End Structure
        Public Structure GRP_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger
            Public unknown As UInteger

            Public data As ULong()
        End Structure
        Public Structure ANIM_
            Public type As Char()
            Public size As UInteger
            Public unknown As Byte()
        End Structure
        Public Structure ACTL_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger

            Public unknown As Byte()()
            ' 0x0C per block
        End Structure
        Public Structure MODE_
            Public type As Char()
            Public size As UInteger
            Public unknown1 As UInteger
            Public unknown2 As UInteger
        End Structure
        Public Structure LABL_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger
            Public names As String()
            ' 0x40 per name
        End Structure
        Public Structure CMNT_
            Public type As Char()
            Public size As UInteger
            Public unknown As UInteger
        End Structure
        Public Structure CCMT_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger

            Public unknown As ULong()
        End Structure
        Public Structure ECMT_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger

            Public size_e As UInteger()
            Public name As String()
            ' SJIS
        End Structure
        Public Structure FCMT_
            Public type As Char()
            Public size As UInteger

            Public data As Byte()
        End Structure
        Public Structure CLBL_
            Public type As Char()
            Public size As UInteger
            Public num_element As UInteger
            Public data As UInteger()
        End Structure
        Public Structure EXTR_
            Public type As Char()
            Public size As UInteger
            Public unknown As UInteger
        End Structure
        Public Structure LINK_
            Public type As Char()
            Public size As UInteger
            Public link As String
        End Structure
    End Structure
End Class
