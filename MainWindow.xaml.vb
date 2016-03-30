Imports LovePlusControlLibrary
Imports System.Windows.Media.Animation
Imports System.Text

Class MainWindow
    Dim NCGRpath As String = ""
    Dim NSCRpath As String = ""
    Dim NCLRpath As String = ""
    Enum decode_status
        success
        cannot_open_NCGR
        cannot_open_NSCR
        cannot_open_NCLR
        cannot_create_bmp
    End Enum

    Declare Function decodefiles Lib "libNCGR2bmp.dll" Alias "#3" (NCGRfile As String, NSCRfile As String, NCLRfile As String, outfile As String) As decode_status
    Private Function GetPath(e As DragEventArgs) As String
        Return CType(e.Data.GetData(DataFormats.FileDrop), String())(0)
    End Function

    Private Sub LPHeartNCGR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNCGR.Drop
        NCGRpath = GetPath(e).Replace("\", "/")
        If NCGRpath.Replace("""", "").ToUpperInvariant.EndsWith(".NCGR") Then
            LPHeartNCGR.BackColor = Brushes.Pink
            LPHeartNCGR.ToolTip = NCGRpath
            txtpath.Text = IO.Path.GetDirectoryName(NCGRpath) + "\" + IO.Path.GetFileNameWithoutExtension(NCGRpath) + ".bmp"
            If EnableAutoMatch.IsChecked Then
                Dim tmp As String = NCGRpath.Replace("""", "")
                tmp = tmp.Substring(0, tmp.Length - 4)
                NSCRpath = tmp + "NSCR"
                NCLRpath = tmp + "NCLR"
                If IO.File.Exists(NSCRpath) Then
                    SetNSCR()
                Else
                    LPHeartNSCR.BackColor = Brushes.Blue
                    LPHeartNSCR.ToolTip = "文件不存在:" + NSCRpath
                End If
                If IO.File.Exists(NCLRpath) Then
                    SetNCLR()
                Else
                    LPHeartNCLR.BackColor = Brushes.Blue
                    LPHeartNCLR.ToolTip = "文件不存在:" + NCLRpath
                End If
                Try
                    Dim n As String = IO.Path.GetFileNameWithoutExtension(NCGRpath)
                    If IsNumeric("&H" + n) Then
                        Dim d As String = IO.Path.GetDirectoryName(NCGRpath) + "\"
                        If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr") Then
                            LPHeartNCLR.BackColor = Brushes.Pink
                            NCLRpath = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr"
                        End If
                        If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nscr") Then
                            LPHeartNSCR.BackColor = Brushes.Pink
                            NSCRpath = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nscr"
                        End If
                        If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nscr") Then
                            LPHeartNSCR.BackColor = Brushes.Pink
                            NSCRpath = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nscr"
                        End If
                        Debug.WriteLine(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr")
                        If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr") Then
                            LPHeartNCLR.BackColor = Brushes.Pink
                            NCLRpath = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr"
                        End If
                    End If
                Catch

                End Try
            End If
        Else
            Msg("文件类型错误,这里需要的是NCGR", LPMessageBox.LPMesageBoxStyle.fault, "错误")
            NCGRpath = ""
            LPHeartNCGR.ToolTip = ""
            LPHeartNCGR.BackColor = Brushes.Gray
        End If
    End Sub

    Private Sub SetNSCR()
        If NSCRpath.Replace("""", "").ToUpperInvariant.EndsWith(".NSCR") Then
            LPHeartNSCR.BackColor = Brushes.Pink
            LPHeartNSCR.ToolTip = NSCRpath
        Else
            If Not EnableAutoMatch.IsChecked Then
                Msg("文件类型错误,这里需要的是NSCR", LPMessageBox.LPMesageBoxStyle.fault, "错误")
            Else
                Msg("未能匹配NSCR文件", , "匹配")
            End If
            NSCRpath = ""
            LPHeartNSCR.ToolTip = ""
            LPHeartNSCR.BackColor = Brushes.Gray
        End If
    End Sub
    Private Sub LPHeartNSCR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNSCR.Drop
        NSCRpath = GetPath(e).Replace("\", "/")
        SetNSCR()
    End Sub
    Private Sub SetNCLR()
        If NCLRpath.Replace("""", "").ToUpperInvariant.EndsWith(".NCLR") Then
            LPHeartNCLR.BackColor = Brushes.Pink
            LPHeartNCLR.ToolTip = NCLRpath
        Else
            If Not EnableAutoMatch.IsChecked Then
                Msg("文件类型错误,这里需要的是NCLR", LPMessageBox.LPMesageBoxStyle.fault, "错误")
            Else
                Msg("未能匹配NCLR文件", , "匹配")
            End If
            NCLRpath = ""
            LPHeartNCLR.ToolTip = ""
            LPHeartNCLR.BackColor = Brushes.Gray
        End If
    End Sub
    Private Sub LPHeartNCLR_Drop(sender As System.Object, e As System.Windows.DragEventArgs) Handles LPHeartNCLR.Drop
        NCLRpath = GetPath(e).Replace("\", "/")
        SetNCLR()
    End Sub

    Private Sub LPButtonOpenLoc_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles LPButtonOpenLoc.PreviewMouseLeftButtonUp
        If txtpath.Text.Length <> 0 Then
            Try
                Microsoft.VisualBasic.Shell("explorer.exe " + IO.Path.GetDirectoryName(txtpath.Text))
            Catch ex As Exception
                ErrorBox(Me, ex)
            End Try
        Else
            Msg("文件路径是空的", LPMessageBox.LPMesageBoxStyle.fault, "错误")
        End If
    End Sub

    Enum DecodeMode
        External
        Internal
    End Enum

    Private Sub LoadDecodedFile()
        Dim dec = BmpBitmapDecoder.Create(New Uri(txtpath.Text), BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
        ImgResult.Source = dec.Frames(0)
    End Sub

    Private Sub LPButtonGenerateImage_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles LPButtonGenerateImage.PreviewMouseLeftButtonUp
        ImgResult.ClearValue(Image.SourceProperty)

        If NCGRpath.Length <> 0 AndAlso NSCRpath.Length <> 0 AndAlso NCLRpath.Length <> 0 AndAlso txtpath.Text.Length <> 0 Then
            'Try
            If Not InternalMode.IsChecked Then
                Dim s = decodefiles(NCGRpath, NSCRpath, NCLRpath, txtpath.Text.Replace("\", "/"))
                Select Case s
                    Case decode_status.success
                        LoadDecodedFile()
                    Case decode_status.cannot_open_NCGR
                        Throw New ComponentModel.Win32Exception("无法打开NCGR")
                    Case decode_status.cannot_open_NCLR
                        Throw New ComponentModel.Win32Exception("无法打开NCLR")
                    Case decode_status.cannot_open_NSCR
                        Throw New ComponentModel.Win32Exception("无法打开NSCR")
                    Case decode_status.cannot_create_bmp
                        Throw New ComponentModel.Win32Exception("无法创建图像")
                End Select
            Else
                NCGRDecoder.DecodeFile(NCGRpath, NSCRpath, NCLRpath, txtpath.Text)
                'Dim enc As New PngBitmapEncoder
                'ImgResult.Source = NCGRDecoder.DecodeNSCR(NCGRpath, NCLRpath, NSCRpath)
                'enc.Frames.Add(BitmapFrame.Create(CType(ImgResult.Source, BitmapSource)))
                'Dim ios As New IO.FileStream(txtpath.Text, IO.FileMode.OpenOrCreate)
                'enc.Save(ios)
                'ios.Flush()
                'ios.Close()
                LoadDecodedFile()
            End If
            If EnableAutoClear.IsChecked Then
                LPHeartNCGR.BackColor = Brushes.Gray
                LPHeartNSCR.BackColor = Brushes.Gray
                LPHeartNCLR.BackColor = Brushes.Gray
                NSCRpath = ""
                NCGRpath = ""
                NCLRpath = ""
            End If

            'Catch ex As Exception
            '   ErrorBox(Me, ex)
            'End Try
        End If
    End Sub

    Private Sub Border1_PreviewMouseLeftButtonDown(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles Border1.PreviewMouseLeftButtonDown, Label1.PreviewMouseLeftButtonDown, Label2.PreviewMouseLeftButtonDown, TextBlock1.PreviewMouseLeftButtonDown, LPHeartNCLR.MouseLeftButtonDown, LPHeartNSCR.MouseLeftButtonDown, LPHeartNCGR.MouseLeftButtonDown, ImgResult.MouseLeftButtonDown
        DragMove()
    End Sub

    Private Sub LPButtonExit_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles LPButtonExit.PreviewMouseLeftButtonUp
        If IsSubWinOpened Then
            CloseAllSubWindow()
        Else
            ClosedSubWindow()
        End If
    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As System.EventArgs) Handles Me.Initialized
        G1.Visibility = Windows.Visibility.Hidden
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Dim th As New System.Threading.Thread(Sub()
                                                  System.Threading.Thread.Sleep(100)
                                                  Dispatcher.BeginInvoke(Sub()
                                                                             G1.Visibility = Windows.Visibility.Visible
                                                                             Dim sc As New ScaleTransform
                                                                             Dim anim As New DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
                                                                             G1.RenderTransform = sc
                                                                             G1.RenderTransformOrigin = New Point(0.5, 0.5)
                                                                             sc.BeginAnimation(ScaleTransform.ScaleXProperty, anim)
                                                                             sc.BeginAnimation(ScaleTransform.ScaleYProperty, anim)
                                                                         End Sub)
                                              End Sub)
        th.Start()
        AddHandler SubWinClosed, Sub()
                                     Dim sc As New ScaleTransform
                                     Dim anim As New DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500))
                                     G1.RenderTransform = sc
                                     G1.RenderTransformOrigin = New Point(0.5, 0.5)
                                     sc.BeginAnimation(ScaleTransform.ScaleXProperty, anim)
                                     sc.BeginAnimation(ScaleTransform.ScaleYProperty, anim)
                                     Dim th2 As New System.Threading.Thread(Sub()
                                                                                System.Threading.Thread.Sleep(600)
                                                                                Dispatcher.BeginInvoke(Sub() Close())
                                                                            End Sub)
                                     th2.Start()
                                 End Sub
        If Not Keyboard.Modifiers = ModifierKeys.Control Then Button1.Visibility = Windows.Visibility.Collapsed
    End Sub
    Dim WithEvents batfiledlg As New Microsoft.Win32.OpenFileDialog() With {.Title = "选择要转换的NCGR文件,按Ctrl多选", .Multiselect = True, .Filter = "任天堂彩色图像资源文件|*.ncgr"}
    Private Sub BatchConvert_PreviewMouseLeftButtonUp(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles BatchConvert.PreviewMouseLeftButtonUp
        batfiledlg.ShowDialog()
    End Sub
    Shared Semaphore1 As New System.Threading.SemaphoreSlim(CpuCoreCount)
    Private Sub batfiledlg_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles batfiledlg.FileOk
        If batfiledlg.FileNames.Count = 0 Then Return
        Dim todo As Integer = batfiledlg.FileNames.Length
        Dim ok As Integer = 0
        Dim th1 As New System.Threading.
            Thread(Sub()
                       System.Threading.Thread.Sleep(20)
                       IgnoreNotSupported = True
                       System.Threading.Tasks.Parallel.
       ForEach(batfiledlg.FileNames,
               Sub(fn As String)
                   Semaphore1.Wait()
                   Try
                       Dim tmp As String = fn.Replace("""", "")
                       tmp = tmp.Substring(0, tmp.Length - 4)
                       Dim NSCR = tmp + "NSCR"
                       Dim NCLR = tmp + "NCLR"
                       Dim PathName = tmp + "BMP"
                       If IO.File.Exists(NSCR) AndAlso IO.File.Exists(NCLR) Then
                           NCGRDecoder.DecodeFile(fn, NSCR, NCLR, PathName)
                       Else

                           Dim n As String = IO.Path.GetFileNameWithoutExtension(tmp)
                           If IsNumeric("&H" + n) Then
                               Dim d As String = IO.Path.GetDirectoryName(tmp) + "\"
                               If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr") Then
                                   NCLR = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nclr"
                               End If
                               If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nscr") Then
                                   NSCR = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nscr"
                               End If
                               If IO.File.Exists(d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nscr") Then
                                   NSCR = d + (CLng("&H" + n) + 1L).ToString("x").strbyte8 + ".nscr"
                               End If
                               If IO.File.Exists(d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr") Then
                                   NCLR = d + (CLng("&H" + n) + 2L).ToString("x").strbyte8 + ".nclr"
                               End If
                           End If
                           NCGRDecoder.DecodeFile(fn, NSCR, NCLR, PathName)
                       End If

                       System.Threading.Interlocked.Increment(ok)
                       If ok = todo Then Dispatcher.BeginInvoke(Sub() Msg("转换完毕！")) : IgnoreNotSupported = False

                   Catch ex As Exception
                       Dispatcher.
                           BeginInvoke(Sub() Msg("转换" + vbLf +
                                                        fn + vbLf +
                                                        "时发生异常，可能因为没有对应的另外两个文件",
                                                        LPMessageBox.LPMesageBoxStyle.fault,
                                                        "错误"))
                   Finally
                       Semaphore1.Release()
                   End Try
               End Sub)
                   End Sub)
        th1.Start()

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button1.Click
        NCGRDecoder.PointerTest()
    End Sub

    Private Sub MainWindow_StateChanged(sender As Object, e As System.EventArgs) Handles Me.StateChanged
        If WindowState = Windows.WindowState.Maximized Then
            WindowState = Windows.WindowState.Normal
            Left = 0
            Top = 0
            Width = SystemParameters.WorkArea.Width
            Height = SystemParameters.WorkArea.Height
        End If
    End Sub

    Private Sub LPButtonAdvanced_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles LPButtonAdvanced.PreviewMouseLeftButtonUp
        Dim adv As New AdvancedConvert
        adv.Owner = Me
        adv.Show()
    End Sub

    Private Sub txtpath_PreviewMouseLeftButtonDown(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles txtpath.PreviewMouseLeftButtonDown
        DragMove()
    End Sub
End Class
