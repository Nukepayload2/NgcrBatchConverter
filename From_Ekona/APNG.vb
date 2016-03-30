' ----------------------------------------------------------------------
' <copyright file="APNG.cs" company="none">

' Copyright (C) 2012
'
'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by 
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
'
'   This program is distributed in the hope that it will be useful, 
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details. 
'
'   You should have received a copy of the GNU General Public License
'   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
'
' </copyright>

' <author>pleoNeX</author>
' <email>benito356@gmail.com</email>
' <date>24/06/2012 14:47:38</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO

Public NotInheritable Class APNG
    Private Sub New()
    End Sub
    ' Info from:
    ' https://wiki.mozilla.org/APNG_Specification
    ' http://www.w3.org/TR/PNG/

    ''' <summary>
    ''' Save an animation in a APNG file (Firefox supported)
    ''' </summary>
    ''' <param name="pngs">All frames (path of files or bitmaps)</param>
    ''' <param name="apng">The path of the output file</param>
    ''' <param name="delay">The delay between frames (delay/1000)</param>
    ''' <param name="loops">The number of  loops (if 0 = infinite)</param>
    Public Shared Sub Create(pngs As String(), apng As String, delay As Integer, loops As Integer)
        Dim pngSignature As Byte() = New Byte() {137, 80, 78, 71, 13, 10, _
         26, 10}
        Dim ihdr As IHDR = Read_IHDR(pngs(0))

        '#Region "Section acTL"
        Dim actl As New acTL()
        actl.length = BitConverter.GetBytes(8).Reverse().ToArray()
        actl.id = Encoding.ASCII.GetBytes(New Char() {"a"c, "c"c, "T"c, "L"c})
        actl.num_frames = BitConverter.GetBytes(pngs.Length).Reverse().ToArray()
        actl.num_plays = BitConverter.GetBytes(loops).Reverse().ToArray()
        ' Loop
        Dim stream As New List(Of Byte)()
        stream.AddRange(actl.id)
        stream.AddRange(actl.num_frames)
        stream.AddRange(actl.num_plays)
        actl.crc = CRC32.Calculate(stream.ToArray())
        stream.Clear()
        '#End Region

        Dim fctl As New List(Of fcTL)()
        Dim fdat As New List(Of fdAT)()
        Dim i As Integer = 0
        fctl.Add(Read_fcTL(pngs(0), i, delay))
        i += 1
        Dim IDAT As Byte() = Read_IDAT(pngs(0))

        For Each png As String In pngs
            If png = pngs(0) Then
                Continue For
            End If

            fctl.Add(Read_fcTL(png, i, delay))
            i += 1
            fdat.Add(Read_fdAT(png, i))
            i += 1
        Next

        Dim iend As New IEND()
        iend.id = Encoding.ASCII.GetBytes(New Char() {"I"c, "E"c, "N"c, "D"c})
        iend.length = BitConverter.GetBytes(0)
        iend.crc = CRC32.Calculate(iend.id)

        Write(apng, pngSignature, ihdr, actl, IDAT, fctl.ToArray(), _
         fdat.ToArray(), iend)
    End Sub
    Public Shared Sub Create(pngs As System.Drawing.Bitmap(), apng As String, delay As Integer, loops As Integer)
        Dim files As String() = New String(pngs.Length - 1) {}

        For i As Integer = 0 To pngs.Length - 1
            files(i) = Path.GetTempFileName()
            pngs(i).Save(files(i))
        Next

        Create(files, apng, delay, loops)
        For i As Integer = 0 To files.Length - 1
            File.Delete(files(i))
        Next
    End Sub

    Private Shared Sub Write(apng As String, signature As Byte(), ihdr As IHDR, actl As acTL, idat As Byte(), fctl As fcTL(), _
     fdat As fdAT(), iend As IEND)
        Dim bw As New BinaryWriter(New FileStream(apng, FileMode.Create))

        bw.Write(signature)

        bw.Write(ihdr.length)
        bw.Write(ihdr.id)
        bw.Write(ihdr.width)
        bw.Write(ihdr.height)
        bw.Write(ihdr.depth)
        bw.Write(ihdr.colour_type)
        bw.Write(ihdr.compression)
        bw.Write(ihdr.filter)
        bw.Write(ihdr.interlace)
        bw.Write(ihdr.crc)

        bw.Write(actl.length)
        bw.Write(actl.id)
        bw.Write(actl.num_frames)
        bw.Write(actl.num_plays)
        bw.Write(actl.crc)

        bw.Write(fctl(0).length)
        bw.Write(fctl(0).id)
        bw.Write(fctl(0).sequence_numer)
        bw.Write(fctl(0).width)
        bw.Write(fctl(0).height)
        bw.Write(fctl(0).x_offset)
        bw.Write(fctl(0).y_offset)
        bw.Write(fctl(0).delay_num)
        bw.Write(fctl(0).delay_den)
        bw.Write(fctl(0).dispose_op)
        bw.Write(fctl(0).blend_op)
        bw.Write(fctl(0).crc)

        bw.Write(idat)

        For i As Integer = 0 To fdat.Length - 1
            bw.Write(fctl(i + 1).length)
            bw.Write(fctl(i + 1).id)
            bw.Write(fctl(i + 1).sequence_numer)
            bw.Write(fctl(i + 1).width)
            bw.Write(fctl(i + 1).height)
            bw.Write(fctl(i + 1).x_offset)
            bw.Write(fctl(i + 1).y_offset)
            bw.Write(fctl(i + 1).delay_num)
            bw.Write(fctl(i + 1).delay_den)
            bw.Write(fctl(i + 1).dispose_op)
            bw.Write(fctl(i + 1).blend_op)
            bw.Write(fctl(i + 1).crc)

            bw.Write(fdat(i).length)
            bw.Write(fdat(i).id)
            bw.Write(fdat(i).sequence_number)
            bw.Write(fdat(i).data)
            bw.Write(fdat(i).crc)
        Next

        bw.Write(iend.length)
        bw.Write(iend.id)
        bw.Write(iend.crc)

        bw.Flush()
        bw.Close()
    End Sub

    Private Shared Function Read_IHDR(png As String) As IHDR
        Dim br As New BinaryReader(New FileStream(png, FileMode.Open))
        br.BaseStream.Position = &H8
        Dim ihdr As New IHDR()

        ihdr.length = br.ReadBytes(4)
        ihdr.id = br.ReadBytes(4)
        ihdr.width = br.ReadUInt32()
        ihdr.height = br.ReadUInt32()
        ihdr.depth = br.ReadByte()
        ihdr.colour_type = br.ReadByte()
        ihdr.compression = br.ReadByte()
        ihdr.filter = br.ReadByte()
        ihdr.interlace = br.ReadByte()
        ihdr.crc = br.ReadBytes(4)

        br.Close()
        Return ihdr
    End Function
    Private Shared Function Read_fcTL(png As String, seq As Integer, delay As Integer) As fcTL
        Dim br As New BinaryReader(New FileStream(png, FileMode.Open))
        br.BaseStream.Position = &H10
        Dim fctl As New fcTL()

        fctl.length = BitConverter.GetBytes(26).Reverse().ToArray()
        fctl.id = Encoding.ASCII.GetBytes(New Char() {"f"c, "c"c, "T"c, "L"c})
        fctl.sequence_numer = BitConverter.GetBytes(seq).Reverse().ToArray()
        fctl.width = br.ReadBytes(4)
        fctl.height = br.ReadBytes(4)
        fctl.x_offset = BitConverter.GetBytes(0)
        fctl.y_offset = BitConverter.GetBytes(0)
        fctl.delay_num = BitConverter.GetBytes(CUShort(delay)).Reverse().ToArray()
        fctl.delay_den = BitConverter.GetBytes(CUShort(1000)).Reverse().ToArray()
        fctl.dispose_op = 1
        fctl.blend_op = 0

        Dim stream As List(Of Byte) = New List(Of Byte)()
        stream.AddRange(fctl.id)
        stream.AddRange(fctl.sequence_numer)
        stream.AddRange(fctl.width)
        stream.AddRange(fctl.height)
        stream.AddRange(fctl.x_offset)
        stream.AddRange(fctl.y_offset)
        stream.AddRange(fctl.delay_num)
        stream.AddRange(fctl.delay_den)
        stream.Add(fctl.dispose_op)
        stream.Add(fctl.blend_op)
        fctl.crc = CRC32.Calculate(stream.ToArray())
        stream.Clear()

        br.Close()
        Return fctl
    End Function
    Private Shared Function Read_IDAT(png As String) As Byte()
        Dim br As New BinaryReader(New FileStream(png, FileMode.Open))
        Dim buffer As Byte()

        Dim encontrado As Boolean = False
        Dim c As String = vbNullChar & vbNullChar & vbNullChar & vbNullChar
        While Not encontrado
            c = c.Remove(0, 1)

            c += ChrW(br.ReadByte())
            If c = "IDAT" Then
                encontrado = True
            End If
        End While

        br.BaseStream.Position -= 8
        Dim length As Integer = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0)
        br.BaseStream.Position -= 4
        buffer = br.ReadBytes(length + 12)

        br.Close()
        Return buffer
    End Function
    Private Shared Function Read_fdAT(png As String, i As Integer) As fdAT
        Dim br As New BinaryReader(New FileStream(png, FileMode.Open))
        Dim fdat As New fdAT()

        fdat.id = Encoding.ASCII.GetBytes(New Char() {"f"c, "d"c, "A"c, "T"c})
        fdat.sequence_number = BitConverter.GetBytes(i).Reverse().ToArray()

        Dim encontrado As Boolean = False
        Dim c As String = vbNullChar & vbNullChar & vbNullChar & vbNullChar
        While Not encontrado
            c = c.Remove(0, 1)

            c += ChrW(br.ReadByte())
            If c = "IDAT" Then
                encontrado = True
            End If
        End While

        br.BaseStream.Position -= 8
        Dim length As Integer = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0)
        fdat.length = BitConverter.GetBytes(length + 4).Reverse().ToArray()
        br.BaseStream.Position += 4
        fdat.data = br.ReadBytes(length)
        Dim stream As List(Of Byte) = New List(Of Byte)()
        stream.AddRange(fdat.id)
        stream.AddRange(fdat.sequence_number)
        stream.AddRange(fdat.data)
        fdat.crc = CRC32.Calculate(stream.ToArray())

        br.Close()
        Return fdat
    End Function

    Private Structure IHDR
        Public length As Byte()
        Public id As Byte()
        Public width As UInteger
        Public height As UInteger
        Public depth As Byte
        Public colour_type As Byte
        Public compression As Byte
        Public filter As Byte
        Public interlace As Byte
        Public crc As Byte()
    End Structure
    Private Structure acTL
        Public length As Byte()
        Public id As Byte()
        Public num_frames As Byte()
        Public num_plays As Byte()
        Public crc As Byte()
    End Structure
    Private Structure fcTL
        Public length As Byte()
        Public id As Byte()
        Public sequence_numer As Byte()
        Public width As Byte()
        Public height As Byte()
        Public x_offset As Byte()
        Public y_offset As Byte()
        Public delay_num As Byte()
        Public delay_den As Byte()
        Public dispose_op As Byte
        Public blend_op As Byte
        Public crc As Byte()
    End Structure
    Private Structure fdAT
        Public length As Byte()
        Public id As Byte()
        Public sequence_number As Byte()
        Public data As Byte()
        Public crc As Byte()
    End Structure
    Private Structure IEND
        Public length As Byte()
        Public id As Byte()
        Public crc As Byte()
    End Structure
End Class