Public Class winForceCrack

    Private Sub winForceCrack_Drop(sender As Object, e As DragEventArgs) Handles Me.Drop
        ImgPreview.ClearValue(Image.SourceProperty)
        Dim f = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If Keyboard.Modifiers = ModifierKeys.Control Then
            System.Threading.Tasks.Parallel.ForEach(f, Sub(fn As String)
                                                           Try
                                                               Dim img As New LPPImage(fn)
                                                               img.WriteBitmap(IO.Path.GetDirectoryName(fn) + "\" + IO.Path.GetFileNameWithoutExtension(fn) + ".png")
                                                           Catch ex As Exception
                                                               LovePlusControlLibrary.ErrorBox(Me, ex)
                                                           End Try
                                                       End Sub)
            LovePlusControlLibrary.Msg("搞定")
        Else
            Try
                Dim img As New LPPImage(f(0))
                ImgPreview.Source = img.GetImageSource
            Catch ex As Exception
                LovePlusControlLibrary.ErrorBox(Me, ex)
            End Try
        End If
    End Sub
End Class
