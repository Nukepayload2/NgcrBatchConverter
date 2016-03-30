Imports System.Drawing
Imports System.IO
Imports System.Reflection

''' <summary>
''' Loveplus+专用图像格式
''' </summary>
''' <remarks></remarks>
Public Class LPPImage
    Dim fn As String
    Dim data As New ImageFile
    Sub New(FileName As String)
        fn = FileName
    End Sub

    Public Function GetBitmap() As Bitmap
        Dim simg As New FileStream(fn, FileMode.Open)
        Dim re As New BinaryReader(simg)
        Dim colors As New List(Of Color)
        With data
            .Flag = re.ReadInt32
            If .Flag <> 33883396 Then Throw New FormatException("不是Loveplus+专用的图像格式")
            .Width = re.ReadInt16
            .Height = re.ReadInt16
            .PaletteOffset = re.ReadInt16
            .PaletteLength = re.ReadInt16
            .ImageLength = re.ReadInt32
            Dim palbuf(.PaletteLength - 1) As Byte
            re.BaseStream.Read(palbuf, 0, palbuf.Length)
            ReDim .ImageData(.ImageLength - 1)
            re.BaseStream.Read(.ImageData, 0, .ImageData.Length)

            Dim bm As New Bitmap(.Width, .Height, Imaging.PixelFormat.Format24bppRgb)
            Dim px(.ImageLength * 3 - 1) As Byte
            Dim data = bm.LockBits(New Rectangle(0, 0, .Width, .Height),
                                   Imaging.ImageLockMode.ReadWrite Or Imaging.ImageLockMode.UserInputBuffer,
                                   Imaging.PixelFormat.Format24bppRgb,
                                   New Imaging.BitmapData With {.Scan0 = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(px, 0),
                                                                .Stride = bm.Width * 3})
            For i As Integer = 0 To palbuf.Length - 2 Step 2
                colors.Add(GetColorFormbgr555(CShort(palbuf(i)) Or (CShort(palbuf(i + 1)) << 8)))
            Next
            For i As Integer = 0 To px.Length - 1 Step 3
                Dim dat = .ImageData(i \ 3)
                Dim co = colors(dat)
                px(i) = co.R '那些人把顺序搞反了...
                px(i + 1) = co.G
                px(i + 2) = co.B
            Next
            bm.UnlockBits(data)
            Return bm
        End With
    End Function
    ''' <summary>
    ''' 保存为png
    ''' </summary>
    ''' <param name="Destination"></param>
    ''' <remarks></remarks>
    Public Sub WriteBitmap(Destination As String)
        GetBitmap.Save(Destination, Imaging.ImageFormat.Png)
    End Sub
    ''' <summary>
    ''' 获取解密后的图像
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetImageSource() As ImageSource
        Return GetBitmap.ToImageSource
    End Function
    ''' <summary>
    ''' 保持为png并返回图像
    ''' </summary>
    ''' <param name="Destination"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function WriteAndReturnImageSource(Destination As String) As ImageSource
        Dim bm = GetBitmap()
        bm.Save(Destination, Imaging.ImageFormat.Png)
        Return bm.ToImageSource
    End Function
    Structure ImageFile
        Dim Flag As Integer  '04 05 05 02 是 33883396
        Dim Width As Int16
        Dim Height As Int16
        Dim PaletteOffset As Int16
        Dim PaletteLength As Int16
        Dim ImageLength As Integer
        Dim PalatteData As Int16() 'BGR555调色盘格式
        Dim ImageData As Byte() '一个像素一字节，对应调色盘颜色
    End Structure

    Private Function GetColorFormbgr555(bgr555 As Int16) As Color
        Dim RGB555_MASK_RED = &H7C00
        Dim RGB555_MASK_GREEN = &H3E0
        Dim RGB555_MASK_BLUE = &H1F
        Dim R = (bgr555 And RGB555_MASK_RED) >> 10 '  取值范围0-31
        Dim G = (bgr555 And RGB555_MASK_GREEN) >> 5 ' 取值范围0-31
        Dim B = bgr555 And RGB555_MASK_BLUE ' 取值范围0-31
        Return Color.FromArgb(R * 8, G * 8, B * 8)
    End Function
    Private Function GetColorFormbgr565(bgr565 As Int16) As Color
        Dim RGB565_MASK_RED = &HF800
        Dim RGB565_MASK_GREEN = &H7E0
        Dim RGB565_MASK_BLUE = &H1F
        Dim R = (bgr565 And RGB565_MASK_RED) >> 11 '  取值范围0-31
        Dim G = (bgr565 And RGB565_MASK_GREEN) >> 5 ' 取值范围0-63
        Dim B = bgr565 And RGB565_MASK_BLUE ' 取值范围0-31
        Return Color.FromArgb(R * 8, G * 4, B * 8)
    End Function
End Class
