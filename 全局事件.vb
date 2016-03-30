Module 全局事件
    Public Event WinClose()
    Public Event SubWinClosed()
    Public Sub CloseAllSubWindow()
        RaiseEvent WinClose()
    End Sub
    Public Sub ClosedSubWindow()
        RaiseEvent SubWinClosed()
    End Sub
    Public IsSubWinOpened As Boolean = False
End Module
