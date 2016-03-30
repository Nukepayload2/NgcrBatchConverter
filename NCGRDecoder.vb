Imports System.IO
Imports System.Drawing
Imports System.Text
Imports LovePlusControlLibrary
Imports System.Runtime.InteropServices

Public Class NCGRDecoder
    Const M = 48
    Const N = 1078

    Private Shared Sub fseek(strm As MemoryStream, offset As Long, location As SeekOrigin)
        strm.Seek(offset, location)
    End Sub

    Private Shared Function getc(strm As MemoryStream) As Byte
        Return CByte(strm.ReadByte())
    End Function

    Private Shared Function putc(Value As Byte, strm As MemoryStream) As Byte
        strm.WriteByte(Value)
        Return Value
    End Function

    Private Shared Function fprintf(strm As MemoryStream, format As String, ParamArray Values() As Object) As Integer
        Dim fm As String() = format.Substring(1, format.Length - 1).Split({"%"}, StringSplitOptions.None)
        Dim i As Integer = 0
        For Each v In Values
            Select Case fm(i)
                Case "c"
                    putc(CByte(v), strm)
                Case Else
                    Throw New NotSupportedException
            End Select
            i += 1
        Next
        Return Values.Length
    End Function

    Private Shared Sub fclose(strm As MemoryStream)
        strm.Close()
    End Sub

    Public Shared Sub DecodeFile(NCGRfile As String, NSCRfile As String, NCLRfile As String, Outfile As String)
        Dim cgr As New BinaryReader(New FileStream(NCGRfile, FileMode.Open))
        Dim scr As New BinaryReader(New FileStream(NSCRfile, FileMode.Open))
        Dim clr As New BinaryReader(New FileStream(NCLRfile, FileMode.Open))
        Dim outbyte As Byte() = DecodeDataFromNSCR(cgr.ReadBytes(CInt(cgr.BaseStream.Length)), scr.ReadBytes(CInt(scr.BaseStream.Length)), clr.ReadBytes(CInt(clr.BaseStream.Length)))
        cgr.Close()
        scr.Close()
        clr.Close()
        Dim bm As New BinaryWriter(New FileStream(Outfile, FileMode.OpenOrCreate))
        bm.Write(outbyte)
        bm.Close()
    End Sub
    ''' <summary>
    ''' 使用NSCR解密图像，大部分思路是DNAsdw提供的,Nukepayload2修改纠正
    ''' </summary>
    ''' <param name="NCGRfile"></param>
    ''' <param name="NSCRfile"></param>
    ''' <param name="NCLRfile"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DecodeDataFromNSCR(NCGRfile As Byte(), NSCRfile As Byte(), NCLRfile As Byte()) As Byte()
        Dim bmp_size, pixel, width, x_number, height, y_number, color_mode, clr16_number, hv_mode, i, j, k, templow, temphigh, tempscr, tempclr, tempR, tempG, tempB As Integer
        Dim ncgr As New MemoryStream(NCGRfile)
        Dim nscr As New MemoryStream(NSCRfile)
        Dim nclr As New MemoryStream(NCLRfile)
        'Nukepayload2增加的部分
        Dim SecSize_High As Byte '区段大小的高位
        fseek(nscr, &H15, SeekOrigin.Begin)
        SecSize_High = getc(nscr)
        Dim NeedToFix As Boolean = False '后面也得用
        If SecSize_High > 8 Then NeedToFix = True
        '----
        fseek(nscr, 24L, 0)
        templow = getc(nscr)
        temphigh = getc(nscr)
        If NeedToFix Then 'Nukepayload2更正.SectionSize以及Width问题修复
            width = ((templow + (temphigh << 8)) << 3) \ SecSize_High
        Else
            width = templow + (temphigh << 8)
        End If
        x_number = width \ 8
        templow = getc(nscr)
        temphigh = getc(nscr)
        height = templow + (temphigh << 8)
        y_number = height \ 8
        pixel = width * height
        bmp_size = N + pixel
        Dim bmpbuff(bmp_size - 1) As Byte
        Dim bmp As New MemoryStream(bmpbuff)
        fseek(bmp, 0L, 0)
        fprintf(bmp, "%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c", Asc("B"), Asc("M"),
                bmp_size And &HFF, bmp_size >> 8 And &HFF,
         bmp_size >> 16 And &HFF, bmp_size >> 24 And &HFF, 0, 0, 0, 0,
         54, 4, 0, 0, 40, 0,
         0, 0, width And &HFF, width >> 8 And &HFF, 0, 0,
         height And &HFF, height >> 8 And &HFF, 0, 0, 1, 0,
         8, 0, 0, 0, 0, 0,
         pixel And &HFF, pixel >> 8 And &HFF, pixel >> 16 And &HFF, pixel >> 24 And &HFF)
        fseek(bmp, 54L, 0)
        fseek(nclr, 40L, 0)
        For i = 0 To 255
            templow = getc(nclr)
            temphigh = getc(nclr)
            tempclr = templow + (temphigh << 8)
            tempR = (tempclr And &H1F) * 8
            tempG = (tempclr >> 5 And &H1F) * 8
            tempB = (tempclr >> 10 And &H1F) * 8
            fprintf(bmp, "%c%c%c%c", tempB, tempG, tempR, 0)
        Next
        fseek(nclr, 24L, 0)
        color_mode = getc(nclr)
        '色盘先别close
        fseek(nscr, 36L, 0)
        Const Color_256 = 4
        Const Color_16 = 3
        Const Rotate_None = 0
        Const Rotate_H = 4
        Const Rotate_V = 8
        Const Rotate_HV = 12
        If SecSize_High <> 12 Then
            If color_mode = Color_256 Then
                For i = 0 To x_number * y_number - 1
                    templow = getc(nscr)
                    temphigh = getc(nscr)
                    hv_mode = temphigh And &HC
                    temphigh = temphigh And 3
                    tempscr = templow + (temphigh << 8)
                    fseek(ncgr, M + tempscr * 64, 0)
                    If hv_mode = Rotate_None Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 7
                                putc(getc(ncgr), bmp)
                            Next
                            fseek(bmp, -(width + 8), SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_H Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8 - 7), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 7
                                putc(getc(ncgr), bmp)
                                fseek(bmp, -2L, SeekOrigin.Current)
                            Next
                            fseek(bmp, -(width - 8), SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_V Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width * 8 - (i Mod x_number) * 8), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 7
                                putc(getc(ncgr), bmp)
                            Next
                            fseek(bmp, width - 8, SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_HV Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width * 8 - (i Mod x_number) * 8 - 7), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 7
                                putc(getc(ncgr), bmp)
                                fseek(bmp, -2L, SeekOrigin.Current)
                            Next
                            fseek(bmp, width + 8, SeekOrigin.Current)
                        Next
                    End If
                Next
            ElseIf color_mode = Color_16 Then
                For i = 0 To x_number * y_number - 1 'ImgSplitIdx 688,len 672
                    templow = getc(nscr)
                    temphigh = getc(nscr)
                    clr16_number = temphigh >> 4 And &HF
                    hv_mode = temphigh And &HC
                    temphigh = temphigh And 3
                    tempscr = templow + (temphigh << 8)
                    'If i >= 688 AndAlso SecSize_High = 12 AndAlso tempscr = 0 Then
                    '后面672字节一片花...
                    'End If
                    fseek(ncgr, M + tempscr * 32, 0)
                    If hv_mode = Rotate_None Then 'loveplus
                        fseek(bmp, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 3
                                temphigh = getc(ncgr)
                                templow = temphigh And &HF
                                temphigh = (temphigh >> 4) And &HF
                                templow += clr16_number << 4
                                temphigh += clr16_number << 4
                                putc(CByte(templow), bmp)
                                putc(CByte(temphigh), bmp)
                            Next
                            fseek(bmp, -(width + 8), SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_H Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8 - 6), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 3
                                temphigh = getc(ncgr)
                                templow = temphigh And &HF
                                temphigh = temphigh >> 4 And &HF
                                templow += clr16_number << 4
                                temphigh += clr16_number << 4
                                putc(CByte(temphigh), bmp)
                                putc(CByte(templow), bmp)
                                fseek(bmp, -4L, SeekOrigin.Current)
                            Next
                            fseek(bmp, -(width - 8), SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_V Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width * 8 - (i Mod x_number) * 8), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 3
                                temphigh = getc(ncgr)
                                templow = temphigh And &HF
                                temphigh = temphigh >> 4 And &HF
                                templow += clr16_number << 4
                                temphigh += clr16_number << 4
                                putc(CByte(templow), bmp)
                                putc(CByte(temphigh), bmp)
                            Next
                            fseek(bmp, width - 8, SeekOrigin.Current)
                        Next
                    ElseIf hv_mode = Rotate_HV Then
                        fseek(bmp, -((i \ x_number) * width * 8 + width * 8 - (i Mod x_number) * 8 - 6), SeekOrigin.End)
                        For j = 0 To 7
                            For k = 0 To 3
                                temphigh = getc(ncgr)
                                templow = temphigh And &HF
                                temphigh = temphigh >> 4 And &HF
                                templow += clr16_number << 4
                                temphigh += clr16_number << 4
                                putc(CByte(temphigh), bmp)
                                putc(CByte(templow), bmp)
                                fseek(bmp, -4L, SeekOrigin.Current)
                            Next
                            fseek(bmp, width + 8, SeekOrigin.Current)
                        Next
                    End If
                Next
            End If
        Else
            '特殊照顾需要修复的文件
            '生成tile次序表,因为源文件的表因为溢出而提供了错误的Tile编号。
            Dim arr1(1024) As UShort
            Dim arr2(511) As UShort
            Dim j1 As Integer = 0
            Dim j2 As Integer = 0
            For c As UShort = 0 To 1535
                If c Mod &H30 < &H20 Then
                    arr1(j1) = c
                    j1 += 1
                Else
                    arr2(j2) = c
                    j2 += 1
                End If
            Next
            Dim arr() As UShort = arr1.Concat(arr2).ToArray
            Dim a As Integer = 0
            For i = 0 To x_number * y_number - 1 'ImgSplitIdx 688,len 672
                templow = getc(nscr)
                temphigh = getc(nscr)
                clr16_number = temphigh >> 4 And &HF
                hv_mode = temphigh And &HC
                temphigh = temphigh And 3
                '自己计算tempscr
                tempscr = arr(a)
                a += 1
                fseek(ncgr, M + tempscr * 32, 0)
                If hv_mode = Rotate_None Then 'loveplus
                    fseek(bmp, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8), SeekOrigin.End)
                    For j = 0 To 7
                        For k = 0 To 3
                            temphigh = getc(ncgr)
                            templow = temphigh And &HF
                            temphigh = (temphigh >> 4) And &HF
                            templow += clr16_number << 4
                            temphigh += clr16_number << 4
                            putc(CByte(templow), bmp)
                            putc(CByte(temphigh), bmp)
                        Next
                        fseek(bmp, -(width + 8), SeekOrigin.Current)
                    Next
                End If
            Next
            '剩下的应该还有16*32*2字节的NSCR
            '0............. 256....
            '.............. .......
            '.............. .......
            '.............. .......
            '.............. .......
            '...........255 ....383
            width = 128
            height = 256
            y_number = 32
            x_number = 16
            pixel = width * height
            Dim bmp2_size As Integer = N + pixel
            Dim bufbmp2(N + pixel - 1) As Byte
            Dim bmp2 As New MemoryStream(bufbmp2)
            fseek(bmp2, 0L, 0)
            fprintf(bmp2, "%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c", Asc("B"), Asc("M"),
                    bmp2_size And &HFF, bmp2_size >> 8 And &HFF,
             bmp2_size >> 16 And &HFF, bmp2_size >> 24 And &HFF, 0, 0, 0, 0,
             54, 4, 0, 0, 40, 0,
             0, 0, width And &HFF, width >> 8 And &HFF, 0, 0,
             height And &HFF, height >> 8 And &HFF, 0, 0, 1, 0,
             8, 0, 0, 0, 0, 0,
             pixel And &HFF, pixel >> 8 And &HFF, pixel >> 16 And &HFF, pixel >> 24 And &HFF)
            fseek(bmp2, 54L, 0)
            fseek(nclr, 40L, 0)
            For i = 0 To 255
                templow = getc(nclr)
                temphigh = getc(nclr)
                tempclr = templow + (temphigh << 8)
                tempR = (tempclr And &H1F) * 8
                tempG = (tempclr >> 5 And &H1F) * 8
                tempB = (tempclr >> 10 And &H1F) * 8
                fprintf(bmp2, "%c%c%c%c", tempB, tempG, tempR, 0)
            Next

            For i = 0 To 16 * 32 - 1
                templow = getc(nscr)
                temphigh = getc(nscr)
                clr16_number = temphigh >> 4 And &HF
                hv_mode = temphigh And &HC
                temphigh = temphigh And 3
                '自己计算tempscr
                a += 1
                tempscr = arr(a)
                fseek(ncgr, M + tempscr * 32, 0)
                fseek(bmp2, -((i \ x_number) * width * 8 + width - (i Mod x_number) * 8), SeekOrigin.End)
                For j = 0 To 7
                    For k = 0 To 3
                        temphigh = getc(ncgr)
                        templow = temphigh And &HF
                        temphigh = (temphigh >> 4) And &HF
                        templow += clr16_number << 4
                        temphigh += clr16_number << 4
                        putc(CByte(templow), bmp2)
                        putc(CByte(temphigh), bmp2)
                    Next
                    fseek(bmp2, -(width + 8), SeekOrigin.Current)
                Next
            Next
            '拼合图像
            Dim bm_1 As New Bitmap(bmp)
            Dim bm_2 As New Bitmap(bmp2)
            Dim bm As New Bitmap(384, 256)
            Dim gr = Graphics.FromImage(bm)
            gr.DrawImage(bm_1, New Point(0, 0))
            gr.DrawImage(bm_2, New Point(256, 0))
            fclose(nclr)
            fclose(nscr)
            fclose(ncgr)
            fclose(bmp)
            fclose(bmp2)
            Dim bmp3 As New MemoryStream
            bm.RotateFlip(RotateFlipType.Rotate270FlipNone) '顺手把图正过来
            bm.Save(bmp3, Imaging.ImageFormat.Bmp)
            Dim arr3() As Byte = bmp3.ToArray
            bmp3.Close()
            Return arr3
        End If
        fclose(nclr)
        fclose(nscr)
        fclose(ncgr)
        fclose(bmp)
        Return bmpbuff
    End Function
    Const NDS_CG_WIDTH_HEIGHT = 256
    
    Delegate Sub ErrorMessageCallback(er As Exception)
    ''' <summary>
    ''' 从NANR,NCER,NCGR,NCLR获取GIF图片
    ''' </summary>
    ''' <param name="NCERPath"></param>
    ''' <param name="NCLRPath"></param>
    ''' <param name="NANRPath"></param>
    ''' <param name="NCGRPath">任意一张就足够了，需要保证文件名是原始的，否则NANR不会识别。</param>
    ''' <param name="ErrorMessage">添加帧的时候不会抛出异常，而是会通过这个委托传送错误消息并继续处理</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetGifFromNANR(NCERPath As String, NCLRPath As String, NANRPath As String, NCGRPath As String, ErrorMessage As ErrorMessageCallback) As GifBitmapEncoder
        Dim cer As New NCER(NCERPath)
        Dim clr As New NCLR(NCLRPath)
        Dim anr As New NANR(NANRPath)
        Dim cgr As New NCGR(NCGRPath)
        Dim bmps As New List(Of Bitmap)
        Dim pa As String = IO.Path.GetDirectoryName(NANRPath)
        For Each s In anr.Names
            Try
                cgr.Read(pa + "\" + s)
                For i As Integer = 0 To cer.NumBanks - 1
                    bmps.Add(Actions.Get_Image(cer.Banks(i), cer.BlockSize, cgr, clr, NDS_CG_WIDTH_HEIGHT, NDS_CG_WIDTH_HEIGHT, False, False, False, True, True, , cgr.Zoom))
                Next
            Catch ex As Exception
                ErrorMessage(ex)
            End Try
        Next
        Dim gifW As New GifBitmapEncoder
        For Each bmp1 In bmps
            gifW.Frames.Add(BitmapFrame.Create(CType(bmp1.ToImageSource, BitmapSource)))
        Next
        Return gifW
    End Function
    Private Declare Function DeleteObject Lib "gdi32" Alias "DeleteObject" (ByVal hObject As IntPtr) As Integer
    ''' <summary>
    ''' 把Bitmap变成ImageSource
    ''' </summary>
    ''' <param name="图">位图</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CImgS(图 As System.Drawing.Bitmap) As ImageSource
        Dim Hbitmap As IntPtr = 图.GetHbitmap
        Dim ImgSource As ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions)
        DeleteObject(Hbitmap)
        Return ImgSource
    End Function
    Private Shared Function IsEmptyOrSolidColorBitmap(bmp As Bitmap) As Boolean
        Try
            Dim bound As Integer
            Dim stride1 As Integer
            Dim stp As Integer
            If bmp.PixelFormat = Imaging.PixelFormat.Format24bppRgb Then
                bound = ((bmp.Width * 3 + 3) And &HFFFFFFFC) * bmp.Height
                stride1 = ((bmp.Width * 3 + 3) And &HFFFFFFFC)
                stp = 3
            ElseIf bmp.PixelFormat = Imaging.PixelFormat.Format32bppArgb Then
                bound = ((bmp.Width * 4 + 4) And &HFFFFFFFC) * bmp.Height
                stp = 4
                stride1 = ((bmp.Width * 4 + 4) And &HFFFFFFFC)
            Else
                Throw New NotSupportedException("PixelFormat不支持")
            End If
            Dim px(bound) As Byte
            Dim data = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height),
                                   Imaging.ImageLockMode.ReadWrite Or Imaging.ImageLockMode.UserInputBuffer,
                                   Imaging.PixelFormat.Format24bppRgb,
                                   New Imaging.BitmapData With {.Scan0 = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(px, 0),
                                                                .Stride = stride1})
            Dim tmp As Byte = px(0)
            For i As Integer = 0 To bound Step stp
                If tmp <> px(i) Then
                    bmp.UnlockBits(data)
                    Return False
                End If
            Next
            bmp.UnlockBits(data)
        Catch ex As Exception
            Return True
        End Try
        Return True
    End Function
    Public Shared Sub DecodeNCER(NCERPath As String, NCLRPath As String, NCGRPath As String, PicDir As String)
        Dim cer As New NCER(NCERPath)
        Dim clr As New NCLR(NCLRPath)
        Dim cgr As New NCGR(NCGRPath)
        Dim j As Integer = 0
        For i As Integer = 0 To cer.NumBanks - 1
            Try
                Dim fn As String
                fn = PicDir + "\" + IO.Path.GetFileNameWithoutExtension(NCGRPath) + "_" + i.ToString + ".png"
                Dim img As Bitmap
                img = Actions.Get_Image(cer.Banks(i), cer.BlockSize, cgr, clr, NDS_CG_WIDTH_HEIGHT, NDS_CG_WIDTH_HEIGHT, False, False, False, True, True, , cgr.Zoom)
                img.Save(fn, Imaging.ImageFormat.Png)
                Dim r As Boolean = IsEmptyOrSolidColorBitmap(img)
                If r Then
                    Debug.WriteLine("过滤空图片")
                Else
                    j += 1
                End If
            Catch ex As Exception
                Msg(ex.ToString)
            End Try
        Next
    End Sub
    Public Shared Function DecodeNSCR(NCGRPath As String, NCLRPath As String, NSCRPath As String) As ImageSource
        Dim scr As New NSCR(NSCRPath, 0)
        Dim clr As New NCLR(NCLRPath)
        Dim cgr As New NCGR(NCGRPath)
        Return CImgS(CType(scr.Get_Image(cgr, clr), Bitmap))
    End Function
    Public Shared Function GetNameListFromNANR(NANRpath As String) As String()
        Dim anr As New NANR(NANRpath)
        Dim l As New List(Of String)
        For Each s In anr.Names
            l.Add(s)
        Next
        Return l.ToArray
    End Function
    <Obsolete("只能用来测试")>
    Public Shared Sub PointerTest()
        Dim ms As New StringBuilder
        Dim l(10) As Long
        Dim p = Pointer(Of Long).FromArray(l, 8)
        p(1) = 2
        p(2) = 4
        p(3) = 6
        ms.AppendLine("下标2的元素是:" + p(2).ToString)
        p += 3
        ms.AppendLine("下标3的元素是:" + p.Target.ToString)
        p -= 2
        ms.AppendLine("下标1的元素是:" + p(0).ToString)
        Dim bs(7) As Byte
        Dim pbyte = Pointer(Of Byte).FromArray(bs, 1)
        p.Address = pbyte.Address
        p(0) = &H123456789ABCDEFL
        ms.AppendLine("用Pointer(Of T)把&H" + Hex(p(0)).ToString + "L转换成Byte()是:")
        For i As Integer = 0 To bs.Length - 2
            ms.Append("&H" + Hex(bs(i)) + ",")
        Next
        ms.AppendLine("&H" + Hex(bs(7)))
        Dim pp As Pointer(Of Long) = Pointer(Of Long).FromArray(bs, 8)
        ms.AppendLine("还原之后是:&H" + Hex(pp(0)))
        Dim cer(1) As TestStruc
        Dim pcer = Pointer(Of TestStruc).FromArray(cer, System.Runtime.InteropServices.
                                                    Marshal.SizeOf(cer(1)))
        Dim ti = Now
        For i As Integer = 1 To 100000
            pcer(1) = New TestStruc With {.a = 12345}
        Next
        Dim a As UInteger = pcer(1).a
        For i As Integer = 1 To 100000
            Dim t = pcer(1)
            If a <> pcer(1).a Then MsgBox("第" + i.ToString + "次", vbExclamation, "PtrToStructure出错了") '产生不稳定后果时提示
            a = pcer(1).a
        Next
        
        Msg(ms.ToString + vbLf +
            "结构体指针测试:写和读TestStruc 100000次:" + pcer(1).a.ToString + " 用时:" + (Now - ti).Milliseconds.ToString)

    End Sub

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Structure TestStruc
        Dim a As UInteger
        Dim b As Byte
        Dim c As Long
    End Structure

    Public Function AnalizeNSCR(fn As String) As String
        Dim sb As New StringBuilder
        Try
            Dim m As New MemoryStream(IO.File.ReadAllBytes(fn))
            With m
                sb.AppendLine("头部标记:" + .ReadString(4))
                sb.AppendLine("头部标记排列方式(FEFF小尾):&H" + .ReadUShort.ToString("X"))
                sb.AppendLine("文件大小(字节):" + .ReadUInt(8).ToString)
                sb.AppendLine("文件头大小:" + .ReadUShort.ToString)
                sb.AppendLine("区段数量:" + .ReadUShort.ToString)
                sb.AppendLine("NRCS标记:" + .ReadString(4))
                sb.AppendLine("区段大小:" + .ReadUInt.ToString)
                sb.AppendLine("宽:" + .ReadUShort.ToString)
                sb.AppendLine("高:" + .ReadUShort.ToString)
                sb.AppendLine("额外的32*32Tile方形数量(不足则舍弃):" + .ReadUInt.ToString)
                Dim datalen As UInteger = .ReadUInt
                If datalen > 8096 Then Throw New ArgumentException("给定文件太大")
                sb.AppendLine("数据长度:" + datalen.ToString)
                sb.AppendLine("开始分析Tile")
                For i As Integer = 0 To CInt(datalen \ 2 - 1)
                    sb.AppendLine("第" + i.ToString + "组数据:")
                    Dim low As Byte = .Read1Byte
                    Dim high As Byte = .Read1Byte
                    sb.Append("色盘号:&H" + (high >> 4).ToString("X"))
                    sb.Append(",水平翻转:&H" + (high >> 3 And 1).ToString("X"))
                    sb.Append(",竖直翻转:&H" + (high >> 2 And 1).ToString("X"))
                    sb.AppendLine(",Tile号:&H" + ((high And 3) << 8 Or low).ToString("X"))
                Next
                sb.AppendLine("剩余:" + (.Length - .Position).ToString)
            End With
        Catch ex As Exception
            sb.AppendLine("出现异常:" + ex.ToString)
        End Try
        Return sb.ToString
    End Function

End Class
