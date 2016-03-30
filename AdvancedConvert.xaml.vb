Imports LovePlusControlLibrary
Public Class AdvancedConvert
#Region "        Wpf处理Windows消息"
    Private Const WM_NCHITTEST As Integer = &H84
    '角宽
    Private ReadOnly agWidth As Integer = 12
    '框宽
    Private ReadOnly bThickness As Integer = 4
    Private mousePoint As New Point()
    Protected Overridable Function WndProc(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
        Select Case msg
            Case WM_NCHITTEST
                mousePoint.X = (lParam.ToInt32() And &HFFFF)
                mousePoint.Y = (lParam.ToInt32() >> 16)
                ' 左上  
                If mousePoint.Y - Top <= agWidth AndAlso mousePoint.X - Left <= agWidth Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTTOPLEFT))
                    ' 左下      
                ElseIf ActualHeight + Top - mousePoint.Y <= agWidth AndAlso mousePoint.X - Left <= agWidth Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTBOTTOMLEFT))
                    ' 右上  
                ElseIf mousePoint.Y - Top <= agWidth AndAlso ActualWidth + Left - mousePoint.X <= agWidth Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTTOPRIGHT))
                    ' 右下  
                ElseIf ActualWidth + Left - mousePoint.X <= agWidth AndAlso ActualHeight + Top - mousePoint.Y <= agWidth Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTBOTTOMRIGHT))
                    ' 左  
                ElseIf mousePoint.X - Left <= bThickness Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTLEFT))
                    ' 右  
                ElseIf ActualWidth + Left - mousePoint.X <= bThickness Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTRIGHT))
                    ' 上  
                ElseIf mousePoint.Y - Top <= bThickness Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTTOP))
                    ' 下  
                ElseIf ActualHeight + Top - mousePoint.Y <= bThickness Then
                    handled = True
                    Return New IntPtr(CInt(HitTest.HTBOTTOM))
                Else
                    handled = False
                End If
        End Select
        Return IntPtr.Zero
    End Function
    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)
        Dim hwndSource As Interop.HwndSource = TryCast(PresentationSource.FromVisual(Me), Interop.HwndSource)
        If hwndSource IsNot Nothing Then
            hwndSource.AddHook(New Interop.HwndSourceHook(AddressOf WndProc))
        End If
    End Sub
    Public Enum HitTest As Integer
        HTERROR = -2
        HTTRANSPARENT = -1
        HTNOWHERE = 0
        HTCLIENT = 1
        HTCAPTION = 2
        HTSYSMENU = 3
        HTGROWBOX = 4
        HTSIZE = HTGROWBOX
        HTMENU = 5
        HTHSCROLL = 6
        HTVSCROLL = 7
        HTMINBUTTON = 8
        HTMAXBUTTON = 9
        HTLEFT = 10
        HTRIGHT = 11
        HTTOP = 12
        HTTOPLEFT = 13
        HTTOPRIGHT = 14
        HTBOTTOM = 15
        HTBOTTOMLEFT = 16
        HTBOTTOMRIGHT = 17
        HTBORDER = 18
        HTREDUCE = HTMINBUTTON
        HTZOOM = HTMAXBUTTON
        HTSIZEFIRST = HTLEFT
        HTSIZELAST = HTBOTTOMRIGHT
        HTOBJECT = 19
        HTCLOSE = 20
        HTHELP = 21
    End Enum
#End Region
    Private Sub Rectangle1_PreviewMouseLeftButtonDown(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles Rectangle1.PreviewMouseLeftButtonDown
        DragMove()
    End Sub

    Private Sub AdvancedConvert_Closed(sender As Object, e As System.EventArgs) Handles Me.Closed
        IsSubWinOpened = False
    End Sub

    Private Sub AdvancedConvert_Initialized(sender As Object, e As System.EventArgs) Handles Me.Initialized
        B1.Visibility = Windows.Visibility.Hidden
    End Sub
    Private Sub CloseThis(Optional RaiseSubWinClosed As Boolean = False)
        Dim th As New System.Threading.Thread(Sub()
                                                  For i As Integer = 1 To 12
                                                      System.Threading.Thread.Sleep(50)
                                                      Dispatcher.Invoke(Sub()
                                                                            Height /= 1.3
                                                                        End Sub)
                                                  Next
                                                  Dispatcher.Invoke(Sub()
                                                                        If RaiseSubWinClosed Then ClosedSubWindow()
                                                                        Close()
                                                                    End Sub)
                                              End Sub)
        th.Start()

    End Sub

    Private Sub AdvancedConvert_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        IsSubWinOpened = True
        AddHandler WinClose, Sub()
                                 CloseThis(True)
                             End Sub
        B1.Visibility = Windows.Visibility.Visible
        Height = 20
        Dim thx As New System.Threading.Thread(Sub()
                                                   For i As Integer = 1 To 12
                                                       System.Threading.Thread.Sleep(50)
                                                       Dispatcher.Invoke(Sub()
                                                                             Height *= 1.3
                                                                         End Sub)
                                                   Next
                                               End Sub)
        thx.Start()
    End Sub

    Private Sub LPButtonClose_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles LPButtonClose.PreviewMouseLeftButtonUp
        CloseThis()
    End Sub
    Dim cer As String = ""
    Dim cgr As String = ""
    Dim clr As String = ""
    Dim anr As String = ""

    Private Sub LPHeartNCLR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNCLR.Drop
        clr = CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
        If clr.ToUpperInvariant.EndsWith("NCLR") Then
            LPHeartNCLR.BackColor = Brushes.Pink
        Else
            LPHeartNCLR.BackColor = Brushes.Blue
        End If
        LPHeartNCLR.ToolTip = clr
    End Sub

    Private Sub LPHeartNCER_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNCER.Drop
        cer = CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
        If cer.ToUpperInvariant.EndsWith("NCER") Then
            LPHeartNCER.BackColor = Brushes.Pink
        Else
            LPHeartNCER.BackColor = Brushes.Blue
        End If
        LPHeartNCER.ToolTip = clr
    End Sub

    Private Sub LPHeartNCGR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNCGR.Drop
        cgr = CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
        If cgr.ToUpperInvariant.EndsWith("NCGR") Then
            LPHeartNCGR.BackColor = Brushes.Pink
            If IO.File.Exists(cgr.Substring(0, cgr.Length - 4) + "ncer") Then
                LPHeartNCER.BackColor = Brushes.Pink
                cer = cgr.Substring(0, cgr.Length - 4) + "ncer"
            End If
            If IO.File.Exists(cgr.Substring(0, cgr.Length - 4) + "nclr") Then
                LPHeartNCLR.BackColor = Brushes.Pink
                clr = cgr.Substring(0, cgr.Length - 4) + "nclr"
            End If
            Try
                Dim n As String = IO.Path.GetFileNameWithoutExtension(cgr)
                If IsNumeric("&H" + n) Then
                    Dim d As String = IO.Path.GetDirectoryName(cgr) + "\"
                    If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr") Then
                        LPHeartNCLR.BackColor = Brushes.Pink
                        clr = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr"
                    End If
                    If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".ncer") Then
                        LPHeartNCER.BackColor = Brushes.Pink
                        cer = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".ncer"
                    End If
                    If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".ncer") Then
                        LPHeartNCER.BackColor = Brushes.Pink
                        cer = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".ncer"
                    End If
                    If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr") Then
                        LPHeartNCLR.BackColor = Brushes.Pink
                        clr = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr"
                    End If
                End If
            Catch

            End Try
        Else
            LPHeartNCGR.BackColor = Brushes.Blue
        End If
        LPHeartNCGR.ToolTip = clr
    End Sub

    Private Sub LPButtonGenerate_clk() Handles LPButtonGenerate.PreviewMouseLeftButtonUp
        Try
            Dim path = IO.Path.GetDirectoryName(cgr)
            NCGRDecoder.DecodeNCER(cer, clr, cgr, path)
            LPHeartNCER.BackColor = Brushes.Gray
            LPHeartNCGR.BackColor = Brushes.Gray
            LPHeartNCLR.BackColor = Brushes.Gray
            cer = ""
            clr = ""
            cgr = ""
            Msg("转换完毕")
        Catch ex As Exception
            Msg("转换失败")
        End Try

    End Sub

    Private Sub ListNANR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles ListNANR.Drop
        anr = CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
        ListNANR.Items.Clear()
        Try
            For Each s In NCGRDecoder.GetNameListFromNANR(anr)
                ListNANR.Items.Add(s)
            Next
        Catch ex As Exception
            Msg("NANR文件加载失败(拖放了错误的文件？)")
        End Try


    End Sub

    Private Sub ListBatConv_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles ListBatConv.Drop
        Dim f = CType(e.Data.GetData(DataFormats.FileDrop), String())
        ListBatConv.Items.Clear()
        For Each s In f
            If s.ToUpperInvariant.EndsWith("NCGR") Then
                If IO.File.Exists(s.Substring(0, s.Length - 4) + "ncer") AndAlso IO.File.Exists(s.Substring(0, s.Length - 4) + "nclr") Then
                    ListBatConv.Items.Add(s)
                Else
                    Msg("无法批处理此文件:" + vbLf + s)
                End If
            End If
        Next
    End Sub

    Private Sub LPButtonBat_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles LPButtonBat.PreviewMouseLeftButtonUp
        Dim lst As New List(Of String)
        For Each a In ListBatConv.Items
            lst.Add(a.ToString)
        Next
        System.Threading.Tasks.Parallel.ForEach(lst, Sub(s As String)
                                                         Try
                                                             Dim ss As String = s
                                                             Dim p As String = IO.Path.GetDirectoryName(s) + "\"
                                                             Dim pn As String = p + IO.Path.GetFileNameWithoutExtension(s) + "."
                                                             NCGRDecoder.DecodeNCER(pn + "ncer", pn + "nclr", pn + "ncgr", p)
                                                             Dispatcher.BeginInvoke(Sub()
                                                                                        ListBatConv.Items.Remove(ss)
                                                                                    End Sub)
                                                         Catch ex As Exception
                                                             ErrorBox(Me, ex)
                                                         End Try
                                                     End Sub)
    End Sub

    Private Sub LPButtonSpecialFormat_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LPButtonSpecialFormat.PreviewMouseLeftButtonDown
        Dim winforce As New winForceCrack
        winforce.Show()
    End Sub
End Class
