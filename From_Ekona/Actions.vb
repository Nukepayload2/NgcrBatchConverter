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
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging


Public Enum TileForm
    Lineal
    Horizontal
    Vertical
End Enum
Public Enum ColorFormat As Byte
    A3I5 = 1
    ' 8 bits-> 0-4: index; 5-7: alpha
    colors4 = 2
    ' 2 bits for 4 colors
    colors16 = 3
    ' 4 bits for 16 colors
    colors256 = 4
    ' 8 bits for 256 colors
    texel4x4 = 5
    ' 32bits, 2bits per Texel (only in textures)
    A5I3 = 6
    ' 8 bits-> 0-2: index; 3-7: alpha
    direct = 7
    ' 16bits, color with BGR555 encoding
    colors2 = 8
    ' 1 bit for 2 colors
    BGRA32 = 9
    ' 32 bits -> ABGR
    A4I4 = 10
    ABGR32 = 11
End Enum
Public Enum ColorEncoding As Byte
    BGR555 = 1
    BGR = 2
    RGB = 3
End Enum

Public NotInheritable Class Actions
    Private Sub New()
    End Sub
#Region "Palette"
    ''' <summary>
    ''' Convert bytes encoding with BGR555 to colors
    ''' </summary>
    ''' <param name="bytes">Bytes encoded with BGR555</param>
    ''' <returns>Colors</returns>
    Public Shared Function BGR555ToColor(bytes As Byte()) As Color()
        Dim colors As Color() = New Color(bytes.Length \ 2 - 1) {}

        For i As Integer = 0 To bytes.Length \ 2 - 1
            colors(i) = BGR555ToColor(bytes(i * 2), bytes(i * 2 + 1))
        Next

        Return colors
    End Function
    ''' <summary>
    ''' Convert two bytes encoded with BGR555 to a color
    ''' </summary>
    Public Shared Function BGR555ToColor(byte1 As Byte, byte2 As Byte) As Color
        Dim r As Integer, b As Integer, g As Integer
        Dim bgr As Short = BitConverter.ToInt16(New Byte() {byte1, byte2}, 0)

        r = (bgr And &H1F) * &H8
        g = ((bgr And &H3E0) >> 5) * &H8
        b = ((bgr And &H7C00) >> 10) * &H8

        Return Color.FromArgb(r, g, b)
    End Function
    ''' <summary>
    ''' Convert colors to byte with BGR555 encoding
    ''' </summary>
    ''' <param name="colors">Colors to convert</param>
    ''' <returns>Bytes converted</returns>
    Public Shared Function ColorToBGR555(colors As Color()) As Byte()
        Dim data As Byte() = New Byte(colors.Length * 2 - 1) {}

        For i As Integer = 0 To colors.Length - 1
            Dim bgr As Byte() = ColorToBGR555(colors(i))
            data(i * 2) = bgr(0)
            data(i * 2 + 1) = bgr(1)
        Next

        Return data
    End Function
    Public Shared Function ColorToBGRA555(color As Color) As Byte()
        Dim d As Byte() = New Byte(1) {}

        Dim r As Integer = color.R \ 8
        Dim g As Integer = (color.G \ 8) << 5
        Dim b As Integer = (color.B \ 8) << 10
        Dim a As Integer = (color.A \ 255) << 15

        Dim bgra As UShort = CUShort(r + g + b + a)
        Array.Copy(BitConverter.GetBytes(bgra), d, 2)

        Return d
    End Function
    Public Shared Function ColorToBGR555(color As Color) As Byte()
        Dim d As Byte() = New Byte(1) {}

        Dim r As Integer = color.R \ 8
        Dim g As Integer = (color.G \ 8) << 5
        Dim b As Integer = (color.B \ 8) << 10

        Dim bgr As UShort = CUShort(r + g + b)
        Array.Copy(BitConverter.GetBytes(bgr), d, 2)

        Return d
    End Function

    Public Shared Function Get_Image(colors As Color()) As Bitmap
        Dim height As Integer = (colors.Length \ &H10)
        If colors.Length Mod &H10 <> 0 Then
            height += 1
        End If

        Dim palette As New Bitmap(160, height * 10)

        Dim [end] As Boolean = False
        Dim i As Integer = 0
        While i < 16 And Not [end]
            For j As Integer = 0 To 15
                If colors.Length <= j + 16 * i Then
                    [end] = True
                    Exit For
                End If

                For k As Integer = 0 To 9
                    For q As Integer = 0 To 9
                        palette.SetPixel((j * 10 + q), (i * 10 + k), colors(j + 16 * i))
                    Next
                Next
            Next
            i += 1
        End While

        Return palette
    End Function

    Public Shared Function Palette_16To256(palette As Color()()) As Color()()
        ' Get the colours of all the palettes in BGR555 encoding
        Dim paletteColor As New List(Of Color)()
        For i As Integer = 0 To palette.Length - 1
            paletteColor.AddRange(palette(i))
        Next

        ' Set the colours in one palette
        Dim newPal As Color()() = New Color(0)() {}
        newPal(0) = paletteColor.ToArray()

        Return newPal
    End Function
    Public Shared Function Palette_256To16(palette As Color()()) As Color()()
        Dim newPal As Color()()

        Dim isExact As Integer = CInt(palette(0).Length) Mod &H10

        If isExact = 0 Then
            newPal = New Color(palette(0).Length \ &H10 - 1)() {}
            For i As Integer = 0 To newPal.Length - 1
                newPal(i) = New Color(15) {}
                Array.Copy(palette(0), i * &H10, newPal(i), 0, &H10)
            Next
        Else
            newPal = New Color((palette(0).Length \ &H10))() {}
            For i As Integer = 0 To newPal.Length - 2
                newPal(i) = New Color(15) {}
                Array.Copy(palette(0), i * &H10, newPal(i), 0, &H10)
            Next
            Dim temp As Color() = New Color(isExact - 1) {}
            Array.Copy(palette(0), palette(0).Length \ &H10, temp, 0, isExact)
            newPal(newPal.Length - 1) = temp
        End If

        Return newPal
    End Function
#End Region

#Region "Tiles"
    Public Shared Function AlphaIndexTo32ARGB(palette As Color(), data As Byte(), format As ColorFormat) As Byte()
        Dim direct As Byte() = New Byte(data.Length * 4 - 1) {}

        For i As Integer = 0 To data.Length - 1
            Dim color__1 As Color = Color.Transparent
            If format = ColorFormat.A3I5 Then
                Dim colorIndex As Integer = data(i) And &H1F
                Dim alpha As Integer = (data(i) >> 5)
                alpha = ((alpha * 4) + (alpha \ 2)) * 8
                color__1 = Color.FromArgb(alpha, palette(colorIndex).R, palette(colorIndex).G, palette(colorIndex).B)
            ElseIf format = ColorFormat.A5I3 Then
                Dim colorIndex As Integer = data(i) And &H7
                Dim alpha As Integer = (data(i) >> 3)
                alpha *= 8
                color__1 = Color.FromArgb(alpha, palette(colorIndex).R, palette(colorIndex).G, palette(colorIndex).B)
            End If

            Dim argb32 As Byte() = BitConverter.GetBytes(color__1.ToArgb())
            Array.Copy(argb32, 0, direct, i * 4, 4)
        Next

        Return direct
    End Function
    Public Shared Function Bpp2ToBpp4(data As Byte()) As Byte()
        Dim bpp4 As Byte() = New Byte(data.Length * 2 - 1) {}

        For i As Integer = 0 To data.Length - 1
            Dim b1 As Byte = CByte(data(i) And &H3)
            b1 += CByte(((data(i) >> 2) And &H3) << 4)

            Dim b2 As Byte = CByte((data(i) >> 4) And &H3)
            b2 += CByte(((data(i) >> 6) And &H3) << 4)

            bpp4(i * 2) = b1
            bpp4(i * 2 + 1) = b2
        Next

        Return bpp4
    End Function

    Public Shared Function Get_Image(tiles As Byte(), tile_pal As Byte(), palette As Color()(), format As ColorFormat, width As Integer, height As Integer, _
     Optional start As Integer = 0) As Bitmap
        If tiles.Length = 0 Then
            Return New Bitmap(1, 1)
        End If

        Dim image As New Bitmap(width, height)

        Dim pos As Integer = start
        For h As Integer = 0 To height - 1
            For w As Integer = 0 To width - 1
                Dim num_pal As Integer = 0
                If tile_pal.Length <= w + h * width Then
                    num_pal = 0
                Else
                    num_pal = tile_pal(w + h * width)
                End If

                If num_pal >= palette.Length Then
                    num_pal = 0
                End If

                Dim color As Color = Get_Color(tiles, palette(num_pal), format, pos)

                image.SetPixel(w, h, color)
            Next
        Next
        Return image
    End Function

    Public Shared Function Get_Color(data As Byte(), palette As Color(), format As ColorFormat, ByRef pos As Integer) As Color
        Dim color__1 As Color = Color.Transparent
        Dim alpha As Integer, index As Integer

        Select Case format
            Case ColorFormat.A3I5
                If data.Length <= pos Then
                    Exit Select
                End If
                index = data(pos) And &H1F
                alpha = (data(pos) >> 5)
                alpha = ((alpha * 4) + (alpha \ 2)) * 8
                If palette.Length > index Then
                    color__1 = Color.FromArgb(alpha, palette(index).R, palette(index).G, palette(index).B)
                End If

                pos += 1
                Exit Select
            Case ColorFormat.A4I4
                If data.Length <= pos Then
                    Exit Select
                End If
                index = data(pos) And &HF
                alpha = (data(pos) >> 4)
                alpha *= 16
                If palette.Length > index Then
                    color__1 = Color.FromArgb(alpha, palette(index).R, palette(index).G, palette(index).B)
                End If

                pos += 1
                Exit Select
            Case ColorFormat.A5I3
                If data.Length <= pos Then
                    Exit Select
                End If
                index = data(pos) And &H7
                alpha = (data(pos) >> 3)
                alpha *= 8
                If palette.Length > index Then
                    color__1 = Color.FromArgb(alpha, palette(index).R, palette(index).G, palette(index).B)
                End If

                pos += 1
                Exit Select

            Case ColorFormat.colors2
                If data.Length <= (pos \ 8) Then
                    Exit Select
                End If
                Dim bit1 As Byte = data(pos \ 8)
                index = BitsConverter.ByteToBits(bit1)(pos Mod 8)
                If palette.Length > index Then
                    color__1 = palette(index)
                End If
                pos += 1
                Exit Select
            Case ColorFormat.colors4
                If data.Length <= (pos \ 4) Then
                    Exit Select
                End If
                Dim bit2 As Byte = data(pos \ 4)
                index = BitsConverter.ByteToBit2(bit2)(pos Mod 4)
                If palette.Length > index Then
                    color__1 = palette(index)
                End If
                pos += 1
                Exit Select
            Case ColorFormat.colors16
                If data.Length <= (pos \ 2) Then
                    Exit Select
                End If
                Dim bit4 As Byte = data(pos \ 2)
                index = BitsConverter.ByteToBit4(bit4)(pos Mod 2)
                If palette.Length > index Then
                    color__1 = palette(index)
                End If
                pos += 1
                Exit Select
            Case ColorFormat.colors256
                If data.Length > pos AndAlso palette.Length > data(pos) Then
                    color__1 = palette(data(pos))
                End If
                pos += 1
                Exit Select

            Case ColorFormat.direct
                ' RGB555
                If pos + 2 >= data.Length Then
                    Exit Select
                End If

                Dim byteColor As UShort = BitConverter.ToUInt16(data, pos)
                color__1 = Color.FromArgb((If((byteColor >> 15) = 1, 255, 0)), (byteColor And &H1F) * 8, ((byteColor >> 5) And &H1F) * 8, ((byteColor >> 10) And &H1F) * 8)
                pos += 2
                Exit Select

            Case ColorFormat.BGRA32
                If pos + 4 >= data.Length Then
                    Exit Select
                End If

                color__1 = Color.FromArgb(data(pos + 3), data(pos + 0), data(pos + 1), data(pos + 2))
                pos += 4
                Exit Select

            Case ColorFormat.ABGR32
                If pos + 4 >= data.Length Then
                    Exit Select
                End If

                color__1 = Color.FromArgb(data(pos + 0), data(pos + 1), data(pos + 2), data(pos + 3))
                pos += 4
                Exit Select

            Case ColorFormat.texel4x4
                Throw New NotSupportedException("Compressed texel 4x4 not supported yet")
            Case Else
                Throw New FormatException("Unknown color format")
        End Select

        Return color__1
    End Function

    Public Shared Function HorizontalToLineal(horizontal As Byte(), width As Integer, height As Integer, bpp As Integer, tile_size As Integer) As Byte()
        Dim lineal As Byte() = New Byte(horizontal.Length - 1) {}
        Dim tile_width As Integer = tile_size * bpp \ 8
        ' Calculate the number of byte per line in the tile
        ' pixels per line * bits per pixel / 8 bits per byte
        Dim tilesX As Integer = width \ tile_size
        Dim tilesY As Integer = height \ tile_size

        Dim pos As Integer = 0
        For ht As Integer = 0 To tilesY - 1
            For wt As Integer = 0 To tilesX - 1
                ' Get the tile data
                For h As Integer = 0 To tile_size - 1
                    For w As Integer = 0 To tile_width - 1
                        If (w + h * tile_width * tilesX) + wt * tile_width + ht * tilesX * tile_size * tile_width >= lineal.Length Then
                            Continue For
                        End If
                        If pos >= lineal.Length Then
                            Continue For
                        End If

                        lineal(System.Math.Max(System.Threading.Interlocked.Increment(pos), pos - 1)) = horizontal((w + h * tile_width * tilesX) + wt * tile_width + ht * tilesX * tile_size * tile_width)
                    Next
                Next
            Next
        Next

        Return lineal
    End Function
    Public Shared Function LinealToHorizontal(lineal As Byte(), width As Integer, height As Integer, bpp As Integer, tile_size As Integer) As Byte()
        Dim horizontal As Byte() = New Byte(lineal.Length - 1) {}
        Dim tile_width As Integer = tile_size * bpp \ 8
        ' Calculate the number of byte per line in the tile
        ' pixels per line * bits per pixel / 8 bits per byte
        Dim tilesX As Integer = width \ tile_size
        Dim tilesY As Integer = height \ tile_size

        Dim pos As Integer = 0
        For ht As Integer = 0 To tilesY - 1
            For wt As Integer = 0 To tilesX - 1
                ' Get the tile data
                For h As Integer = 0 To tile_size - 1
                    For w As Integer = 0 To tile_width - 1
                        If (w + h * tile_width * tilesX) + wt * tile_width + ht * tilesX * tile_size * tile_width >= lineal.Length Then
                            Continue For
                        End If
                        If pos >= lineal.Length Then
                            Continue For
                        End If
                        horizontal((w + h * tile_width * tilesX) + wt * tile_width + ht * tilesX * tile_size * tile_width) = lineal(pos)
                        pos += 1

                    Next
                Next
            Next
        Next

        Return horizontal
    End Function

    Public Shared Function Remove_DuplicatedColors(ByRef palette As Color(), ByRef tiles As Byte()) As Integer
        Dim colors As New List(Of Color)()
        Dim first_duplicated_color As Integer = -1

        For i As Integer = 0 To palette.Length - 1
            If Not colors.Contains(palette(i)) Then
                colors.Add(palette(i))
            Else
                ' The color is duplicated
                Dim newIndex As Integer = colors.IndexOf(palette(i))
                Replace_Color(tiles, i, newIndex)
                colors.Add(Color.FromArgb(248, 0, 248))

                If first_duplicated_color = -1 Then
                    first_duplicated_color = i
                End If
            End If
        Next

        palette = colors.ToArray()
        Return first_duplicated_color
    End Function
    Public Shared Function Remove_NotUsedColors(ByRef palette As Color(), ByRef tiles As Byte()) As Integer
        Dim first_notUsed_color As Integer = -1

        Dim colors As Boolean() = New Boolean(palette.Length - 1) {}
        For i As Integer = 0 To palette.Length - 1
            colors(i) = False
        Next

        For i As Integer = 0 To tiles.Length - 1
            colors(tiles(i)) = True
        Next

        For i As Integer = 0 To colors.Length - 1
            If Not colors(i) Then
                first_notUsed_color = i
            End If
        Next

        Return first_notUsed_color
    End Function
    Public Shared Sub Change_Color(ByRef tiles As Byte(), oldIndex As Integer, newIndex As Integer, format As ColorFormat)
        If format = ColorFormat.colors16 Then
            ' Yeah, I should improve it
            tiles = BitsConverter.BytesToBit4(tiles)
        ElseIf format <> ColorFormat.colors256 Then
            Throw New NotSupportedException("Only supported 4bpp and 8bpp ")
        End If

        For i As Integer = 0 To tiles.Length - 1
            If tiles(i) = oldIndex Then
                tiles(i) = CByte(newIndex)
            ElseIf tiles(i) = newIndex Then
                tiles(i) = CByte(oldIndex)
            End If
        Next

        If format = ColorFormat.colors16 Then
            tiles = BitsConverter.Bits4ToByte(tiles)
        End If
    End Sub
    Public Shared Sub Swap_Color(ByRef tiles As Byte(), ByRef palette As Color(), oldIndex As Integer, newIndex As Integer, format As ColorFormat)
        If format = ColorFormat.colors16 Then
            ' Yeah, I should improve it
            tiles = BitsConverter.BytesToBit4(tiles)
        ElseIf format <> ColorFormat.colors256 Then
            Throw New NotSupportedException("Only supported 4bpp and 8bpp ")
        End If

        Dim old_color As Color = palette(oldIndex)
        palette(oldIndex) = palette(newIndex)
        palette(newIndex) = old_color

        For i As Integer = 0 To tiles.Length - 1
            If tiles(i) = oldIndex Then
                tiles(i) = CByte(newIndex)
            ElseIf tiles(i) = newIndex Then
                tiles(i) = CByte(oldIndex)
            End If
        Next

        If format = ColorFormat.colors16 Then
            tiles = BitsConverter.Bits4ToByte(tiles)
        End If
    End Sub
    Public Shared Sub Replace_Color(ByRef tiles As Byte(), oldIndex As Integer, newIndex As Integer)
        For i As Integer = 0 To tiles.Length - 1
            If tiles(i) = oldIndex Then
                tiles(i) = CByte(newIndex)
            End If
        Next
    End Sub
    Public Shared Sub Swap_Palette(ByRef tiles As Byte(), newp As Color(), oldp As Color(), format As ColorFormat, Optional threshold As Decimal = 0)
        If format = ColorFormat.colors16 Then
            ' Yeah, I should improve it
            tiles = BitsConverter.BytesToBit4(tiles)
        ElseIf format <> ColorFormat.colors256 Then
            Throw New NotSupportedException("Only supported 4bpp and 8bpp ")
        End If

        Dim notfound As New List(Of Color)()
        Dim newplist As New List(Of Color)(newp)

        For i As Integer = 0 To tiles.Length - 1
            Dim px As Color = oldp(tiles(i))
            Dim id As Integer = newplist.IndexOf(px)

            If px = Color.Transparent AndAlso id = -1 Then
                id = 0
            End If

            If id = -1 Then
                id = FindNextColor(px, newp, threshold)
            End If

            If id = -1 Then
                ' If the color is not found, maybe is that the pixel own to another cell (overlapping cells).
                ' For this reason, there are two ways to do that:
                ' 1º Get the original hidden color from the original file                               <- In mind
                ' 2º Set this pixel as transparent to show the pixel from the other cell (tiles[i] = 0) <- Done!
                ' If there isn't overlapping cells, throw exception                                     <- In mind
                notfound.Add(px)
                id = 0
            End If

            tiles(i) = CByte(id)
        Next

        'if (notfound.Count > 0)
        '    throw new NotSupportedException("Color not found in the original palette!");

        If format = ColorFormat.colors16 Then
            tiles = BitsConverter.Bits4ToByte(tiles)
        End If
    End Sub

    Public Shared Function Get_Size(fileSize As Integer, bpp As Integer) As Size
        Dim width As Integer, height As Integer
        Dim num_pix As Integer = fileSize * 8 \ bpp

        ' If the image it's a square
        If Math.Pow(CInt(Math.Truncate(Math.Sqrt(num_pix))), 2) = num_pix Then
            width = InlineAssignHelper(height, CInt(Math.Truncate(Math.Sqrt(num_pix))))
        Else
            width = (If(num_pix < &H100, num_pix, &H100))
            height = num_pix \ width
        End If

        If height = 0 Then
            height = 1
        End If
        If width = 0 Then
            width = 1
        End If

        Return New Size(width, height)
    End Function

    Public Shared Function Add_Image(ByRef data As Byte(), newData As Byte(), blockSize As UInteger) As UInteger
        ' Add the image to the end of the data
        ' Return the offset where the data is added
        Dim result As New List(Of Byte)()
        result.AddRange(data)

        While result.Count Mod blockSize <> 0
            result.Add(&H0)
        End While

        Dim offset As UInteger = CUInt(result.Count)

        result.AddRange(newData)
        While result.Count Mod blockSize <> 0
            result.Add(&H0)
        End While

        data = result.ToArray()
        Return offset
    End Function

    Public Shared Function FindNextColor(c As Color, palette As Color(), Optional threshold As Decimal = 0) As Integer
        Dim id As Integer = -1

        Dim min_distance As Decimal = CDec(Math.Sqrt(3)) * 255
        ' Set the max distance
        Dim [module] As Double = Math.Sqrt(c.R * c.R + c.G * c.G + c.B * c.B)
        For i As Integer = 1 To palette.Length - 1
            Dim modulec As Double = Math.Sqrt(palette(i).R * palette(i).R + palette(i).G * palette(i).G + palette(i).B * palette(i).B)
            Dim distance As Decimal = CDec(Math.Abs([module] - modulec))

            If distance < min_distance Then
                min_distance = distance
                id = i
            End If
        Next

        If min_distance > threshold Then
            ' If the distance it's bigger than wanted
            id = -1
        End If

        ' If still it doesn't found the color try with the first one, usually is transparent so for this reason we leave it to the end
        If id = -1 Then
            Dim modulec As Double = Math.Sqrt(palette(0).R * palette(0).R + palette(0).G * palette(0).G + palette(0).B * palette(0).B)
            Dim distance As Decimal = CDec(Math.Abs([module] - modulec))

            If distance <= threshold Then
                id = 0
            End If
        End If

        If id = -1 Then
            Console.Write("Color not found: ")
            Console.WriteLine(c.ToString() & " (distance: " & min_distance.ToString() & ")"c)
        End If

        Return id
    End Function
    Public Shared Sub Indexed_Image(img As Bitmap, cf As ColorFormat, ByRef tiles As Byte(), ByRef palette As Color())
        ' It's a slow method but it should work always
        Dim width As Integer = img.Width
        Dim height As Integer = img.Height

        Dim coldif As New List(Of Color)()
        Dim data As Integer(,) = New Integer(width * height - 1, 1) {}

        ' Get the indexed data
        For h As Integer = 0 To height - 1
            For w As Integer = 0 To width - 1
                Dim pix As Color = img.GetPixel(w, h)
                Dim apix As Color = Color.FromArgb(pix.R, pix.G, pix.B)
                ' Without alpha value
                If pix.A = 0 Then
                    apix = Color.Transparent
                End If

                ' Add the color to the provisional palette
                If Not coldif.Contains(apix) Then
                    coldif.Add(apix)
                End If

                ' Get the index and save the alpha value
                data(w + h * width, 0) = coldif.IndexOf(apix)
                ' Index
                ' Alpha value
                data(w + h * width, 1) = pix.A
            Next
        Next

        Dim max_colors As Integer = 0
        ' Maximum colors per palette
        Dim bpc As Integer = 0
        ' Bits per color
        Select Case cf
            Case ColorFormat.A3I5
                max_colors = 32
                bpc = 8
                Exit Select
            Case ColorFormat.colors4
                max_colors = 4
                bpc = 2
                Exit Select
            Case ColorFormat.colors16
                max_colors = 16
                bpc = 4
                Exit Select
            Case ColorFormat.colors256
                max_colors = 256
                bpc = 8
                Exit Select
            Case ColorFormat.texel4x4
                Throw New NotSupportedException("Texel 4x4 not supported yet.")
            Case ColorFormat.A5I3
                max_colors = 8
                bpc = 8
                Exit Select
            Case ColorFormat.direct
                max_colors = 0
                bpc = 16
                Exit Select
            Case ColorFormat.colors2
                max_colors = 2
                bpc = 1
                Exit Select
            Case ColorFormat.A4I4
                max_colors = 16
                bpc = 8
                Exit Select
        End Select

        ' Not dithering method for now, I hope you input a image with less than the maximum colors
        If coldif.Count > max_colors AndAlso cf <> ColorFormat.direct Then
            Throw New NotSupportedException("The image has more colors than permitted." & vbLf & (coldif.Count + 1).ToString() & " unique colors!")
        End If

        ' Finally get the set the tile array with the correct format
        tiles = New Byte(width * height * bpc \ 8 - 1) {}
        Dim i As Integer = 0, j As Integer = 0
        While i < tiles.Length
            Select Case cf
                Case ColorFormat.colors2, ColorFormat.colors4, ColorFormat.colors16, ColorFormat.colors256
                    Dim b As Integer = 0
                    While b < 8
                        If j < data.Length Then
                            tiles(i) = tiles(i) Or CByte(data(System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1), 0) << b)
                        End If
                        b += bpc
                    End While

                    i += 1
                    Exit Select

                Case ColorFormat.A3I5
                    Dim alpha1 As Byte = CByte(data(j, 1) * 8 \ 256)
                    Dim va1 As Byte = CByte(data(System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1), 0))
                    va1 = va1 Or CByte(alpha1 << 5)
                    tiles(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = va1
                    Exit Select
                Case ColorFormat.A4I4
                    Dim alpha3 As Byte = CByte(data(j, 1) * 16 \ 256)
                    Dim va3 As Byte = CByte(data(System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1), 0))
                    va3 = va3 Or CByte(alpha3 << 4)
                    tiles(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = va3
                    Exit Select
                Case ColorFormat.A5I3
                    Dim alpha2 As Byte = CByte(data(j, 1) * 32 \ 256)
                    Dim va2 As Byte = CByte(data(System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1), 0))
                    va2 = va2 Or CByte(alpha2 << 3)
                    tiles(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = va2
                    Exit Select

                Case ColorFormat.direct
                    Dim v As Byte() = ColorToBGRA555(Color.FromArgb(data(j, 1), coldif(data(System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1), 0))))
                    tiles(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = v(0)
                    tiles(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = v(1)
                    Exit Select

                Case ColorFormat.texel4x4
                    ' Not supported
                    Exit Select
            End Select
        End While

        palette = coldif.ToArray()
    End Sub
#End Region

#Region "Map"
    Public Shared Function Apply_Map(map As NTFS(), tiles As Byte(), ByRef tile_pal As Byte(), bpp As Integer, tile_size As Integer, Optional startInfo As Integer = 0) As Byte()
        Dim tile_length As Integer = tile_size * tile_size * bpp \ 8
        Dim num_tiles As Integer = tiles.Length \ tile_length

        Dim bytes As List(Of Byte) = New List(Of Byte)()
        tile_pal = New Byte((map.Length - startInfo) * tile_size * tile_size - 1) {}

        For i As Integer = startInfo To map.Length - 1
            If map(i).nTile >= num_tiles Then
                map(i).nTile = 0
            End If

            Dim currTile As Byte() = New Byte(tile_length - 1) {}
            If map(i).nTile * tile_length + tile_length > tiles.Length Then
                map(i).nTile = 0
            End If

            If tile_length < tiles.Length Then
                Array.Copy(tiles, map(i).nTile * tile_length, currTile, 0, tile_length)
            End If

            If map(i).xFlip = 1 Then
                currTile = XFlip(currTile, tile_size, bpp)
            End If
            If map(i).yFlip = 1 Then
                currTile = YFlip(currTile, tile_size, bpp)
            End If

            bytes.AddRange(currTile)

            For t As Integer = 0 To tile_size * tile_size - 1
                tile_pal(i * tile_size * tile_size + t) = map(i).nPalette
            Next
        Next

        Return bytes.ToArray()
    End Function
    Public Shared Function XFlip(tile As Byte(), tile_size As Integer, bpp As Integer) As Byte()
        Dim newTile As Byte() = New Byte(tile.Length - 1) {}
        Dim tile_width As Integer = tile_size * bpp \ 8

        For h As Integer = 0 To tile_size - 1
            For w As Integer = 0 To tile_width \ 2 - 1
                Dim b As Byte = tile(((tile_width - 1) - w) + h * tile_width)
                newTile(w + h * tile_width) = Reverse_Bits(b, bpp)

                b = tile(w + h * tile_width)
                newTile(((tile_width - 1) - w) + h * tile_width) = Reverse_Bits(b, bpp)
            Next
        Next
        Return newTile
    End Function
    Public Shared Function Reverse_Bits(b As Byte, length As Integer) As Byte
        Dim rb As Byte = 0

        If length = 4 Then
            rb = CByte((b << 4) + (b >> 4))
        ElseIf length = 8 Then
            Return b
        End If

        Return rb
    End Function
    Public Shared Function YFlip(tile As Byte(), tile_size As Integer, bpp As Integer) As Byte()
        Dim newTile As Byte() = New Byte(tile.Length - 1) {}
        Dim tile_width As Integer = tile_size * bpp \ 8

        For h As Integer = 0 To tile_size \ 2 - 1
            For w As Integer = 0 To tile_width - 1
                newTile(w + h * tile_width) = tile(w + (tile_size - 1 - h) * tile_width)
                newTile(w + (tile_size - 1 - h) * tile_width) = tile(w + h * tile_width)
            Next
        Next
        Return newTile
    End Function

    Public Shared Function Create_BasicMap(num_tiles As Integer, Optional startTile As Integer = 0, Optional palette As Byte = 0) As NTFS()
        Dim map As NTFS() = New NTFS(num_tiles - 1) {}

        For i As Integer = startTile To num_tiles - 1
            map(i) = New NTFS()
            map(i).nPalette = palette
            map(i).yFlip = 0
            map(i).xFlip = 0
            'if (i >= startFillTile)
            '    map[i].nTile = (ushort)fillTile;
            'else
            map(i).nTile = CUShort(i + startTile)
        Next

        Return map
    End Function
    Public Shared Function Create_Map(ByRef data As Byte(), tile_width As Integer, tile_size As Integer, Optional palette As Byte = 0) As NTFS()
        ' Divide the data in tiles
        Dim tiles As Byte()() = New Byte(data.Length \ (tile_width * 8) - 1)() {}
        For i As Integer = 0 To tiles.Length - 1
            tiles(i) = New Byte(tile_width * 8 - 1) {}
            Array.Copy(data, i * (tile_width * 8), tiles(i), 0, tile_width * 8)
        Next

        Dim map As NTFS() = New NTFS(tiles.Length - 1) {}
        Dim newtiles As New List(Of Byte())()
        For i As Integer = 0 To map.Length - 1
            map(i).nPalette = palette
            map(i).xFlip = 0
            map(i).yFlip = 0

            Dim index As Integer = -1
            Dim flipX As Byte = 0
            Dim flipY As Byte = 0

            For t As Integer = 0 To newtiles.Count - 1
                If Compare_Array(newtiles(t), tiles(i)) Then
                    index = t
                    Exit For
                End If
                If Compare_Array(newtiles(t), XFlip(tiles(i), tile_size, tile_width)) Then
                    index = t
                    flipX = 1
                    Exit For
                End If
                If Compare_Array(newtiles(t), YFlip(tiles(i), tile_size, tile_width)) Then
                    index = t
                    flipY = 1
                    Exit For
                End If
                If Compare_Array(newtiles(t), YFlip(XFlip(tiles(i), tile_size, tile_width), tile_size, tile_width)) Then
                    index = t
                    flipX = 1
                    flipY = 1
                    Exit For
                End If
            Next

            If index > -1 Then
                map(i).nTile = CUShort(index)
            Else
                map(i).nTile = CUShort(newtiles.Count)
                newtiles.Add(tiles(i))
            End If
            map(i).xFlip = flipX
            map(i).yFlip = flipY
        Next

        ' Save the new tiles
        data = New Byte(newtiles.Count * tile_width * 8 - 1) {}
        For i As Integer = 0 To newtiles.Count - 1
            For j As Integer = 0 To newtiles(i).Length - 1
                data(j + i * (tile_width * 8)) = newtiles(i)(j)
            Next
        Next
        Return map
    End Function
    Public Shared Function Compare_Array(d1 As Byte(), d2 As Byte()) As Boolean
        If d1.Length <> d2.Length Then
            Return False
        End If

        For i As Integer = 0 To d1.Length - 1
            If d1(i) <> d2(i) Then
                Return False
            End If
        Next

        Return True
    End Function

    Public Shared Function MapInfo(value As UShort) As NTFS
        Dim mapInfo__1 As New NTFS()

        mapInfo__1.nTile = CUShort(value And &H3FF)
        mapInfo__1.xFlip = CByte((value >> 10) And 1)
        mapInfo__1.yFlip = CByte((value >> 11) And 1)
        mapInfo__1.nPalette = CByte((value >> 12) And &HF)

        Return mapInfo__1
    End Function
    Public Shared Function MapInfo(map As NTFS) As UShort
        Dim npalette As Integer = map.nPalette << 12
        Dim yFlip As Integer = map.yFlip << 11
        Dim xFlip As Integer = map.xFlip << 10
        Dim data As Integer = npalette + yFlip + xFlip + map.nTile

        Return CUShort(data)
    End Function
#End Region

#Region "OAM"
    Public Shared Function Get_OAMSize(shape As Byte, size As Byte) As Size
        Dim imageSize As New Size()

        Select Case shape
            Case &H0
                ' Square
                Select Case size
                    Case &H0
                        imageSize = New Size(8, 8)
                        Exit Select
                    Case &H1
                        imageSize = New Size(16, 16)
                        Exit Select
                    Case &H2
                        imageSize = New Size(32, 32)
                        Exit Select
                    Case &H3
                        imageSize = New Size(64, 64)
                        Exit Select
                End Select
                Exit Select
            Case &H1
                ' Horizontal
                Select Case size
                    Case &H0
                        imageSize = New Size(16, 8)
                        Exit Select
                    Case &H1
                        imageSize = New Size(32, 8)
                        Exit Select
                    Case &H2
                        imageSize = New Size(32, 16)
                        Exit Select
                    Case &H3
                        imageSize = New Size(64, 32)
                        Exit Select
                End Select
                Exit Select
            Case &H2
                ' Vertical
                Select Case size
                    Case &H0
                        imageSize = New Size(8, 16)
                        Exit Select
                    Case &H1
                        imageSize = New Size(8, 32)
                        Exit Select
                    Case &H2
                        imageSize = New Size(16, 32)
                        Exit Select
                    Case &H3
                        imageSize = New Size(32, 64)
                        Exit Select
                End Select
                Exit Select
        End Select

        Return imageSize
    End Function

    Public Shared Function Get_Image(bank As Bank, blockSize As UInteger, img As ImageBase, pal As PaletteBase, max_width As Integer, max_height As Integer, _
     draw_grid As Boolean, draw_cells As Boolean, draw_numbers As Boolean, trans As Boolean, image As Boolean, Optional currOAM As Integer = -1, _
     Optional zoom As Integer = 1, Optional index As Integer() = Nothing) As Bitmap
        Dim size As New Size(max_width * zoom, max_height * zoom)
        Dim bank_img As New Bitmap(size.Width, size.Height)
        Dim graphic As Graphics = Graphics.FromImage(bank_img)

        If bank.oams.Length = 0 Then
            graphic.DrawString("No OAM", SystemFonts.CaptionFont, Brushes.Black, New PointF(max_width \ 2, max_height \ 2))
            Return bank_img
        End If

        If draw_grid Then
            For i As Integer = (0 - size.Width) To size.Width - 1 Step 8
                graphic.DrawLine(Pens.LightBlue, (i + size.Width \ 2) * zoom, 0, (i + size.Width \ 2) * zoom, size.Height * zoom)
                graphic.DrawLine(Pens.LightBlue, 0, (i + size.Height \ 2) * zoom, size.Width * zoom, (i + size.Height \ 2) * zoom)
            Next
            graphic.DrawLine(Pens.Blue, (max_width \ 2) * zoom, 0, (max_width \ 2) * zoom, max_height * zoom)
            graphic.DrawLine(Pens.Blue, 0, (max_height \ 2) * zoom, max_width * zoom, (max_height \ 2) * zoom)
        End If


        Dim cell As Image
        For i As Integer = 0 To bank.oams.Length - 1
            Dim draw As Boolean = False
            If index Is Nothing Then
                draw = True
            Else
                For k As Integer = 0 To index.Length - 1
                    If index(k) = i Then
                        draw = True
                    End If
                Next
            End If
            If Not draw Then
                Continue For
            End If

            If bank.oams(i).width = &H0 OrElse bank.oams(i).height = &H0 Then
                Continue For
            End If

            Dim tileOffset As UInteger = bank.oams(i).obj2.tileOffset
            tileOffset = CUInt(tileOffset << CByte(blockSize))

            If image Then
                Dim cell_img As ImageBase = New TestImage()
                cell_img.Set_Tiles(DirectCast(img.Tiles.Clone(), Byte()), bank.oams(i).width, bank.oams(i).height, img.FormatColor, img.FormTile, False)
                cell_img.StartByte = CInt(tileOffset) * &H20

                Dim num_pal As Byte = bank.oams(i).obj2.index_palette
                If num_pal >= pal.NumberOfPalettes Then
                    num_pal = 0
                End If
                For j As Integer = 0 To cell_img.TilesPalette.Length - 1
                    cell_img.TilesPalette(j) = num_pal
                Next

                cell = cell_img.Get_Image(pal)
                'else
                '{
                '    tileOffset /= (blockSize / 2);
                '    int imageWidth = img.Width;
                '    int imageHeight = img.Height;

                '    int posX = (int)(tileOffset % imageWidth);
                '    int posY = (int)(tileOffset / imageWidth);

                '    if (img.ColorFormat == ColorFormat.colors16)
                '        posY *= (int)blockSize * 2;
                '    else
                '        posY *= (int)blockSize;
                '    if (posY >= imageHeight)
                '        posY = posY % imageHeight;

                '    cells[i] = ((Bitmap)img.Get_Image(pal)).Clone(new Rectangle(posX * zoom, posY * zoom, bank.oams[i].width * zoom, bank.oams[i].height * zoom),
                '                                                System.Drawing.Imaging.PixelFormat.DontCare);
                '}

                '#Region "Flip"
                If bank.oams(i).obj1.flipX = 1 AndAlso bank.oams(i).obj1.flipY = 1 Then
                    cell.RotateFlip(RotateFlipType.RotateNoneFlipXY)
                ElseIf bank.oams(i).obj1.flipX = 1 Then
                    cell.RotateFlip(RotateFlipType.RotateNoneFlipX)
                ElseIf bank.oams(i).obj1.flipY = 1 Then
                    cell.RotateFlip(RotateFlipType.RotateNoneFlipY)
                End If
                '#End Region

                If trans Then
                    DirectCast(cell, Bitmap).MakeTransparent(pal.Palette(num_pal)(0))
                End If

                graphic.DrawImageUnscaled(cell, size.Width \ 2 + bank.oams(i).obj1.xOffset * zoom, size.Height \ 2 + bank.oams(i).obj0.yOffset * zoom)
            End If

            If draw_cells Then
                graphic.DrawRectangle(Pens.Black, size.Width \ 2 + bank.oams(i).obj1.xOffset * zoom, size.Height \ 2 + bank.oams(i).obj0.yOffset * zoom, bank.oams(i).width * zoom, bank.oams(i).height * zoom)
            End If
            If i = currOAM Then
                graphic.DrawRectangle(New Pen(Color.Red, 3), size.Width \ 2 + bank.oams(i).obj1.xOffset * zoom, size.Height \ 2 + bank.oams(i).obj0.yOffset * zoom, bank.oams(i).width * zoom, bank.oams(i).height * zoom)
            End If
            If draw_numbers Then
                graphic.DrawString(bank.oams(i).num_cell.ToString(), SystemFonts.CaptionFont, Brushes.Black, size.Width \ 2 + bank.oams(i).obj1.xOffset * zoom, size.Height \ 2 + bank.oams(i).obj0.yOffset * zoom)
            End If
        Next

        Return bank_img
    End Function

    Public Shared Function Get_OAMdata(oam As OAM, image As Byte(), format As ColorFormat) As Byte()
        If format = ColorFormat.colors16 Then
            image = BitsConverter.BytesToBit4(image)
        End If

        Dim data As New List(Of Byte)()
        Dim y1 As Integer = 128 + oam.obj0.yOffset
        Dim y2 As Integer = y1 + oam.height
        Dim x1 As Integer = 256 + oam.obj1.xOffset
        Dim x2 As Integer = x1 + oam.width

        For ht As Integer = 0 To 255
            For wt As Integer = 0 To 511
                If ht >= y1 AndAlso ht < y2 Then
                    If wt >= x1 AndAlso wt < x2 Then
                        data.Add(image(wt + ht * 512))
                    End If
                End If
            Next
        Next

        If format = ColorFormat.colors16 Then
            Return BitsConverter.Bits4ToByte(data.ToArray())
        Else
            Return data.ToArray()
        End If
    End Function
    Public Shared Function Comparision_OAM(c1 As OAM, c2 As OAM) As Integer
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

    Public Shared Function OAMInfo(oam As OAM) As UShort()
        Dim obj As UShort() = New UShort(2) {}

        ' OBJ0
        obj(0) = 0
        obj(0) += CUShort(CSByte(oam.obj0.yOffset) And &HFF)
        obj(0) += CUShort((oam.obj0.rs_flag And 1) << 8)
        If oam.obj0.rs_flag = &H0 Then
            obj(0) += CUShort((oam.obj0.objDisable And 1) << 9)
        Else
            obj(0) += CUShort((oam.obj0.doubleSize And 1) << 9)
        End If
        obj(0) += CUShort((oam.obj0.objMode And 3) << 10)
        obj(0) += CUShort((oam.obj0.mosaic_flag And 1) << 12)
        obj(0) += CUShort((oam.obj0.depth And 1) << 13)
        obj(0) += CUShort((oam.obj0.shape And 3) << 14)

        ' OBJ1
        obj(1) = 0
        If oam.obj1.xOffset < 0 Then
            oam.obj1.xOffset += &H200
        End If
        obj(1) += CUShort(oam.obj1.xOffset And &H1FF)
        If oam.obj0.rs_flag = 0 Then
            obj(1) += CUShort((oam.obj1.unused And &H7) << 9)
            obj(1) += CUShort((oam.obj1.flipX And 1) << 12)
            obj(1) += CUShort((oam.obj1.flipY And 1) << 13)
        Else
            obj(1) += CUShort((oam.obj1.select_param And &H1F) << 9)
        End If
        obj(1) += CUShort((oam.obj1.size And 3) << 14)

        ' OBJ2
        obj(2) = 0
        obj(2) += CUShort(oam.obj2.tileOffset And &H3FF)
        obj(2) += CUShort((oam.obj2.priority And 3) << 10)
        obj(2) += CUShort((oam.obj2.index_palette And &HF) << 12)

        Return obj
    End Function
    Public Shared Function OAMInfo(obj As UShort()) As OAM
        Dim oam As New OAM()

        ' Obj 0
        oam.obj0.yOffset = CByte(obj(0) And &HFF)
        oam.obj0.rs_flag = CByte((obj(0) >> 8) And 1)
        If oam.obj0.rs_flag = 0 Then
            oam.obj0.objDisable = CByte((obj(0) >> 9) And 1)
        Else
            oam.obj0.doubleSize = CByte((obj(0) >> 9) And 1)
        End If
        oam.obj0.objMode = CByte((obj(0) >> 10) And 3)
        oam.obj0.mosaic_flag = CByte((obj(0) >> 12) And 1)
        oam.obj0.depth = CByte((obj(0) >> 13) And 1)
        oam.obj0.shape = CByte((obj(0) >> 14) And 3)

        ' Obj 1
        oam.obj1.xOffset = obj(1) And &H1FF
        If oam.obj1.xOffset >= &H100 Then
            oam.obj1.xOffset -= &H200
        End If
        If oam.obj0.rs_flag = 0 Then
            oam.obj1.unused = CByte((obj(1) >> 9) And 7)
            oam.obj1.flipX = CByte((obj(1) >> 12) And 1)
            oam.obj1.flipY = CByte((obj(1) >> 13) And 1)
        Else
            oam.obj1.select_param = CByte((obj(1) >> 9) And &H1F)
        End If
        oam.obj1.size = CByte((obj(1) >> 14) And 3)

        ' Obj 2
        oam.obj2.tileOffset = CUInt(obj(2) And &H3FF)
        oam.obj2.priority = CByte((obj(2) >> 10) And 3)
        oam.obj2.index_palette = CByte((obj(2) >> 12) And &HF)

        Dim size As Size = Get_OAMSize(oam.obj0.shape, oam.obj1.size)
        oam.width = CUShort(size.Width)
        oam.height = CUShort(size.Height)

        Return oam
    End Function
    Public Shared Function OAMInfo(v1 As UShort, v2 As UShort, v3 As UShort) As OAM
        Return OAMInfo(New UShort() {v1, v2, v3})
    End Function
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
#End Region
End Class