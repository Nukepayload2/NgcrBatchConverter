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
' *   by pleoNeX
' * 
' 

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Drawing



Public Interface IPluginHost
    Function Get_Object() As Object

    Function Get_Image() As ImageBase
    Function Get_Palette() As PaletteBase
    Function Get_Map() As MapBase
    Function Get_Sprite() As SpriteBase

    Sub Set_Object(objects As Object)

    Sub Set_Image(image As ImageBase)
    Sub Set_Palette(palette As PaletteBase)
    Sub Set_Map(map As MapBase)
    Sub Set_Sprite(sprite As SpriteBase)

    Function PluginList() As String()
    Function Call_Plugin(param As String(), id As Integer, action As Integer) As Object

    Sub Set_Files(folder As sFolder)
    Function Get_Files() As sFolder
    Function Get_DecompressedFiles(id As Integer) As sFolder

    Function Search_File(id As Integer) As [String]
    ' Search file by id
    Function Search_File(id As Short) As sFile
    Function Search_File(name As String) As sFolder
    Function Get_Bytes(path As String, offset As Integer, length As Integer) As Byte()

    Function Search_Folder(id As Integer) As sFolder

    Function Get_Language() As String
    Function Get_LangXML() As String

    Function Get_LanguageFolder() As String

    Function Get_TempFile() As String
    Function Get_TempFolder() As String
    Sub Set_TempFolder(newPath As String)
    Sub Restore_TempFolder()

    Sub Decompress(file As String)
    Sub Decompress(data As Byte())
    Sub Compress(filein As String, fileout As String, format As FormatCompress)

    ''' <summary>
    ''' Change the content of a file
    ''' </summary>
    ''' <param name="id">The id of the file to change</param>
    ''' <param name="newFile">The path where the new file is</param>
    Sub ChangeFile(id As Integer, newFile As String)
End Interface
