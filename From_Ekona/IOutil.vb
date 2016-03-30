' ----------------------------------------------------------------------
' <copyright file="IOutil.cs" company="none">

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
' <date>04/07/2012 12:55:15</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports Microsoft.Win32


Public NotInheritable Class IOutil
    Private Sub New()
    End Sub
    Public Shared Sub Append(ByRef bw As BinaryWriter, file__1 As String)
        Dim br As New BinaryReader(File.OpenRead(file__1))
        Append(bw, br)

        br.Close()
        br = Nothing
    End Sub
    Public Shared Sub Append(ByRef bw As BinaryWriter, ByRef br As BinaryReader)
        Const block_size As Integer = &H80000
        ' 512 KB
        Dim size As Integer = CInt(br.BaseStream.Length)

        While br.BaseStream.Position + block_size < size
            bw.Write(br.ReadBytes(block_size))
            bw.Flush()
        End While

        Dim rest As Integer = size - CInt(br.BaseStream.Position)
        bw.Write(br.ReadBytes(rest))
        bw.Flush()
    End Sub

    Public Shared Function LastSelectedFile() As String
        Dim recent As String = Environment.GetFolderPath(Environment.SpecialFolder.Recent)
        Dim info As New DirectoryInfo(recent)
        Dim files As FileInfo() = info.GetFiles().OrderBy(Function(p) p.LastAccessTime).ToArray()

        If files.Length > 0 Then
            For i As Integer = 1 To files.Length
                Dim link As New LNK(files(files.Length - i).FullName)
                If Not link.FileAttribute.archive Then
                    Continue For
                End If

                Return link.Path
            Next
        End If

        Return Nothing
    End Function
    Public Shared Function GetLastOpenSaveFile(extention As String) As String
        ' IT DOESN'T WORK YET
        Dim regKey As RegistryKey = Registry.CurrentUser
        Dim lastUsedFolder As String = String.Empty
        regKey = regKey.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU")

        If String.IsNullOrEmpty(extention) Then
            Return lastUsedFolder
        End If

        Dim myKey As RegistryKey = regKey.OpenSubKey(extention)

        If myKey Is Nothing AndAlso regKey.GetSubKeyNames().Length > 0 Then
            Return lastUsedFolder
        End If

        Dim names As String() = myKey.GetValueNames()
        If Not IsNothing(names) AndAlso names.Length > 0 Then
            'lastUsedFolder = new String(Encoding.ASCII.GetChars((byte[])myKey.GetValue(names[names.Length - 2])));
            File.WriteAllBytes("G:\reg.bin", DirectCast(myKey.GetValue(names(names.Length - 1)), Byte()))
        End If

        Return lastUsedFolder
    End Function


End Class

