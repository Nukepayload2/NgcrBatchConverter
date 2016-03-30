Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Drawing
Public Class NCER
    Inherits SpriteBase
    Private ncer As sNCER

    Public Sub New(file As String, Optional id As Integer = 0, Optional fileName As String = "")
        MyBase.New(file, id, fileName)
    End Sub

    Public Overrides Sub Read(fileIn As String)
        'System.Xml.Linq.XElement xml = Tools.Helper.GetTranslation("NCER");
        'Console.WriteLine("NCER {0}<pre>", Path.GetFileName(file));
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        ncer = New sNCER()

        ' Generic header
        ncer.header.id = br.ReadChars(4)
        ncer.header.endianess = br.ReadUInt16()
        If ncer.header.endianess = &HFFFE Then
            ncer.header.id.Reverse()
        End If
        ncer.header.constant = br.ReadUInt16()
        ncer.header.file_size = br.ReadUInt32()
        ncer.header.header_size = br.ReadUInt16()
        ncer.header.nSection = br.ReadUInt16()

        ' CEBK (CEll BanK)
        ncer.cebk.id = br.ReadChars(4)
        ncer.cebk.section_size = br.ReadUInt32()
        ncer.cebk.nBanks = br.ReadUInt16()
        ncer.cebk.tBank = br.ReadUInt16()
        ncer.cebk.constant = br.ReadUInt32()
        ncer.cebk.block_size = CUInt(br.ReadUInt32() And &HFF)
        ncer.cebk.unknown1 = br.ReadUInt32()
        ncer.cebk.unknown2 = br.ReadUInt64()
        ncer.cebk.banks = New sNCER.Bank(ncer.cebk.nBanks - 1) {}

        Dim tilePos As UInteger = &H0
        ' If unknown1 != 0x00
        '#Region "Read banks"
        For i As Integer = 0 To ncer.cebk.nBanks - 1
            ncer.cebk.banks(i).nCells = br.ReadUInt16()
            ncer.cebk.banks(i).unknown1 = br.ReadUInt16()
            ncer.cebk.banks(i).cell_offset = br.ReadUInt32()

            If ncer.cebk.tBank = &H1 Then
                ncer.cebk.banks(i).xMax = br.ReadInt16()
                ncer.cebk.banks(i).yMax = br.ReadInt16()
                ncer.cebk.banks(i).xMin = br.ReadInt16()
                ncer.cebk.banks(i).yMin = br.ReadInt16()
            End If

            Dim posicion As Long = br.BaseStream.Position
            If ncer.cebk.tBank = &H0 Then
                br.BaseStream.Position += (ncer.cebk.nBanks - (i + 1)) * 8 + ncer.cebk.banks(i).cell_offset
            Else
                br.BaseStream.Position += (ncer.cebk.nBanks - (i + 1)) * &H10 + ncer.cebk.banks(i).cell_offset
            End If

            ncer.cebk.banks(i).oams = New OAM(ncer.cebk.banks(i).nCells - 1) {}
            '#Region "Read cells"
            For j As Integer = 0 To ncer.cebk.banks(i).nCells - 1
                Dim obj0 As UShort = br.ReadUInt16()
                Dim obj1 As UShort = br.ReadUInt16()
                Dim obj2 As UShort = br.ReadUInt16()

                ncer.cebk.banks(i).oams(j) = Actions.OAMInfo(New UShort() {obj0, obj1, obj2})
                ncer.cebk.banks(i).oams(j).num_cell = CUShort(j)

                If ncer.cebk.unknown1 <> &H0 Then
                    ncer.cebk.banks(i).oams(j).obj2.tileOffset += tilePos
                End If

                ' Calculate the size
                Dim cellSize As Size = Actions.Get_OAMSize(ncer.cebk.banks(i).oams(j).obj0.shape, ncer.cebk.banks(i).oams(j).obj1.size)
                ncer.cebk.banks(i).oams(j).height = CUShort(cellSize.Height)
                ncer.cebk.banks(i).oams(j).width = CUShort(cellSize.Width)
            Next
            '#End Region

            ' Sort the oam using the priority value
            Dim oams As New List(Of OAM)()
            oams.AddRange(ncer.cebk.banks(i).oams)
            oams.Sort(AddressOf Comparision_Cell)
            ncer.cebk.banks(i).oams = oams.ToArray()

            ' Calculate the next tileOffset if unknonw1 != 0
            If ncer.cebk.unknown1 <> &H0 AndAlso ncer.cebk.banks(i).nCells <> &H0 Then
                Dim last_oam As OAM = Get_LastOAM(ncer.cebk.banks(i))

                Dim ultimaCeldaSize As Integer = CInt(last_oam.height * last_oam.width)
                ultimaCeldaSize \= CInt(64 << CByte(ncer.cebk.block_size))
                If last_oam.obj0.depth = 1 Then
                    ultimaCeldaSize *= 2
                End If
                If ultimaCeldaSize = 0 Then
                    ultimaCeldaSize = 1
                End If

                tilePos += CUInt((last_oam.obj2.tileOffset - tilePos) + ultimaCeldaSize)
            End If
            'Console.WriteLine("--------------");
            br.BaseStream.Position = posicion
        Next
        '#End Region

        '#Region "LABL"
        br.BaseStream.Position = ncer.header.header_size + ncer.cebk.section_size
        Dim offsets As New List(Of UInteger)()
        Dim names As List(Of [String]) = New List(Of String)()
        ncer.labl.names = New String(ncer.cebk.nBanks - 1) {}

        ncer.labl.id = br.ReadChars(4)
        If New [String](ncer.labl.id) <> "LBAL" Then
            GoTo Tercera
        End If
        ncer.labl.section_size = br.ReadUInt32()

        ' Name offset
        For i As Integer = 0 To ncer.cebk.nBanks - 1
            Dim offset As UInteger = br.ReadUInt32()
            If offset >= ncer.labl.section_size - 8 Then
                br.BaseStream.Position -= 4
                Exit For
            End If

            offsets.Add(offset)
        Next
        ncer.labl.offset = offsets.ToArray()

        ' Names
        For i As Integer = 0 To ncer.labl.offset.Length - 1
            names.Add("")
            Dim c As Byte = br.ReadByte()
            While c <> &H0
                names(i) += ChrW(c)
                c = br.ReadByte()
            End While
        Next
Tercera:
        For i As Integer = 0 To ncer.cebk.nBanks - 1
            If names.Count > i Then
                ncer.labl.names(i) = names(i)
            Else
                ncer.labl.names(i) = i.ToString()
            End If
        Next
        '#End Region

        '#Region "UEXT"
        ncer.uext.id = br.ReadChars(4)
        If New [String](ncer.uext.id) <> "TXEU" Then
            GoTo Fin
        End If

        ncer.uext.section_size = br.ReadUInt32()
        ncer.uext.unknown = br.ReadUInt32()
Fin:
        '#End Region

        br.Close()
        
        Set__Banks(Convert_Banks(), ncer.cebk.block_size, True)
    End Sub
    Private Function Get_LastOAM(bank As sNCER.Bank) As OAM
        For i As Integer = 0 To bank.oams.Length - 1
            If bank.oams(i).num_cell = bank.oams.Length - 1 Then
                Return bank.oams(i)
            End If
        Next

        Return New OAM()
    End Function
    Private Function Comparision_Cell(c1 As OAM, c2 As OAM) As Integer
        If c1.obj2.priority < c2.obj2.priority Then
            Return 1
        ElseIf c1.obj2.priority > c2.obj2.priority Then
            Return -1
        Else
            ' Same priority
            If c1.num_cell < c2.num_cell Then
                Return 1
            ElseIf c1.num_cell > c2.num_cell Then
                Return -1
            Else
                ' Same cell
                Return 0
            End If
        End If
    End Function
    Private Function Comparision_Cell2(c1 As OAM, c2 As OAM) As Integer
        If c1.num_cell > c2.num_cell Then
            Return 1
        ElseIf c1.num_cell < c2.num_cell Then
            Return -1
        Else
            Return 0
        End If
    End Function


    Private Function Convert_Banks() As Bank()
        Dim banks As Bank() = New Bank(ncer.cebk.banks.Length - 1) {}
        For i As Integer = 0 To banks.Length - 1
            banks(i).height = 0
            banks(i).width = 0
            banks(i).oams = ncer.cebk.banks(i).oams
            If ncer.labl.names.Length > i Then
                banks(i).name = ncer.labl.names(i)
            End If
        Next
        Return banks
    End Function

    Public Overrides Sub Write(fileOut As String, image As ImageBase, palette As PaletteBase)
        Update_Struct()

        Dim bw As New BinaryWriter(File.OpenWrite(fileOut))

        ' Generic header
        bw.Write(ncer.header.id)
        bw.Write(ncer.header.endianess)
        bw.Write(ncer.header.constant)
        bw.Write(ncer.header.file_size)
        bw.Write(ncer.header.header_size)
        bw.Write(ncer.header.nSection)

        ' CEBK section (CEll BanK)
        bw.Write(ncer.cebk.id)
        bw.Write(ncer.cebk.section_size)
        bw.Write(ncer.cebk.nBanks)
        bw.Write(ncer.cebk.tBank)
        bw.Write(ncer.cebk.constant)
        bw.Write(ncer.cebk.block_size)
        bw.Write(&H0)
        ' I don't like when it's different to 0 ;)
        bw.Write(ncer.cebk.unknown2)

        ' Banks
        For i As Integer = 0 To ncer.cebk.banks.Length - 1
            bw.Write(ncer.cebk.banks(i).nCells)
            bw.Write(ncer.cebk.banks(i).unknown1)
            bw.Write(ncer.cebk.banks(i).cell_offset)

            If ncer.cebk.tBank = 1 Then
                bw.Write(ncer.cebk.banks(i).xMax)
                bw.Write(ncer.cebk.banks(i).yMax)
                bw.Write(ncer.cebk.banks(i).xMin)
                bw.Write(ncer.cebk.banks(i).yMin)
            End If
        Next

        ' OAMs
        For i As Integer = 0 To ncer.cebk.banks.Length - 1
            For j As Integer = 0 To ncer.cebk.banks(i).nCells - 1
                Dim oam As OAM = ncer.cebk.banks(i).oams(j)
                Dim obj As UShort() = Actions.OAMInfo(oam)

                bw.Write(BitConverter.GetBytes(obj(0)))
                bw.Write(BitConverter.GetBytes(obj(1)))
                bw.Write(BitConverter.GetBytes(obj(2)))
            Next
        Next

        While bw.BaseStream.Position Mod 4 <> 0
            bw.Write(CByte(&H0))
        End While

        ' LBAL section
        If New String(ncer.labl.id) = "LBAL" OrElse New String(ncer.labl.id) = "LABL" Then
            bw.Write(ncer.labl.id)
            bw.Write(ncer.labl.section_size)
            For i As Integer = 0 To ncer.labl.offset.Length - 1
                bw.Write(ncer.labl.offset(i))
            Next
            For i As Integer = 0 To ncer.labl.offset.Length - 1
                bw.Write((ncer.labl.names(i) & ControlChars.NullChar).ToCharArray())
            Next
        End If

        ' UEXT section
        If New String(ncer.uext.id) = "UEXT" OrElse New String(ncer.uext.id) = "TXEU" Then
            bw.Write(ncer.uext.id)
            bw.Write(ncer.uext.section_size)
            bw.Write(ncer.uext.unknown)
        End If

        bw.Flush()
        bw.Close()
    End Sub
    Private Sub Update_Struct()
        ' Update OAMs and LABL section
        Dim offset_cells As UInteger = 0
        Dim size As UInteger = 0

        For i As Integer = 0 To Banks.Length - 1
            ncer.cebk.banks(i).nCells = CUShort(Banks(i).oams.Length)
            ncer.cebk.banks(i).cell_offset = offset_cells
            offset_cells += CUInt(Banks(i).oams.Length * 6)

            size += CUInt(If(ncer.cebk.tBank = 0, &H8, &H10))
            size += CUInt(6 * Banks(i).oams.Length)

            ncer.cebk.banks(i).oams = Banks(i).oams
            Dim oams As New List(Of OAM)()
            oams.AddRange(ncer.cebk.banks(i).oams)
            oams.Sort(AddressOf Comparision_Cell2)
            ncer.cebk.banks(i).oams = oams.ToArray()
        Next

        ' Update the rest
        ncer.cebk.block_size = BlockSize
        ncer.cebk.nBanks = CUShort(Banks.Length)
        ncer.cebk.section_size = CUInt(&H20 + size)
        If ncer.cebk.section_size Mod 4 <> 0 Then
            ncer.cebk.section_size = CUInt(ncer.cebk.section_size + (4 - (ncer.cebk.section_size Mod 4)))
        End If

        ' Update the header
        ncer.header.file_size = CUInt(&H10 + ncer.cebk.section_size)
        If New String(ncer.labl.id) = "LBAL" OrElse New String(ncer.labl.id) = "LABL" Then
            ncer.header.file_size += ncer.labl.section_size
        End If
        If New String(ncer.uext.id) = "UEXT" OrElse New String(ncer.uext.id) = "TXEU" Then
            ncer.header.file_size += ncer.uext.section_size
        End If
    End Sub

    Public Structure sNCER
        ' Nintendo CEll Resource
        Public header As NitroHeader
        Public cebk As CEBK_
        Public labl As LABL_
        Public uext As UEXT_

        Public Structure CEBK_
            Public id As Char()
            Public section_size As UInt32
            Public nBanks As UInt16
            Public tBank As UInt16
            ' type of banks, 0 ó 1
            Public constant As UInt32
            Public block_size As UInt32
            Public unknown1 As UInt32
            Public unknown2 As UInt64
            ' padding?
            Public banks As Bank()
        End Structure
        Public Structure Bank
            Public nCells As UInt16
            Public unknown1 As UInt16
            Public cell_offset As UInt32
            Public oams As OAM()

            ' Extended mode
            Public xMax As Short
            Public yMax As Short
            Public xMin As Short
            Public yMin As Short
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
