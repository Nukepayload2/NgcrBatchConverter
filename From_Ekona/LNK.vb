' ----------------------------------------------------------------------
' <copyright file="LNK.cs" company="none">

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
' <date>06/08/2012 14:08:22</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO


''' <summary>
''' Specification from http://msdn.microsoft.com/en-us/library/dd871305%28v=prot.13%29.aspx
''' </summary>
Public Class LNK
    Private lnk As SHELL_LINK

    Public Sub New(fileIn As String)
        Read(fileIn)
    End Sub

    Private Sub Read(fileIn As String)
        Dim br As New BinaryReader(File.OpenRead(fileIn))
        lnk = New SHELL_LINK()

        lnk.header = Read_Header(br)

        If lnk.header.linkFlags.hasLinkTargetIDList Then
            lnk.idlist = Read_LinkIDList(br)
        End If
        If lnk.header.linkFlags.hasLinkInfo Then
            lnk.info = Read_LinkInfo(br)
        End If

        lnk.sdata = Read_StringData(br)
        lnk.extra = Read_Extra(br)

        br.Close()
        br = Nothing
    End Sub

    Private Function Read_Header(br As BinaryReader) As SHELL_LINK_HEADER
        Dim header As New SHELL_LINK_HEADER()

        header.headerSize = br.ReadUInt32()
        If header.headerSize <> &H4C Then
            Throw New FormatException("Incorrect file size!")
        End If

        header.linkCLSID = br.ReadBytes(&H10)
        For i As Integer = 0 To 15
            If header.linkCLSID(i) <> CLSID(i) Then
                Throw New FormatException("Invalid CLSID!")
            End If
        Next

        header.linkFlags = Read_LinkFlags(br.ReadUInt32())
        header.fileAttributes = Read_FileAttribute(br.ReadUInt32())
        header.creationTime.dateTime = br.ReadUInt64()
        header.accessTime.dateTime = br.ReadUInt64()
        header.writeTime.dateTime = br.ReadUInt64()
        header.fileSize = br.ReadUInt32()
        header.iconIndex = br.ReadInt32()
        header.showCommand = CType(br.ReadUInt32(), SHOW_COMMAND)
        header.hotKey.low = DirectCast(br.ReadByte(), HOTKEYS_FLAGS.LOW_BYTE)
        header.hotKey.hight = DirectCast(br.ReadByte(), HOTKEYS_FLAGS.HIGH_BYTE)
        header.reserved1 = br.ReadUInt16()
        header.reserved2 = br.ReadUInt32()
        header.reserved3 = br.ReadUInt32()

        Return header
    End Function
    Private Function Read_LinkFlags(value As UInteger) As LINK_FLAGS
        Dim flags As New LINK_FLAGS()

        flags.hasLinkTargetIDList = Get_Boolean(value)
        value >>= 1
        flags.hasLinkInfo = Get_Boolean(value)
        value >>= 1
        flags.hasName = Get_Boolean(value)
        value >>= 1
        flags.hasRelativePath = Get_Boolean(value)
        value >>= 1
        flags.hasWorkingDir = Get_Boolean(value)
        value >>= 1
        flags.hasArguments = Get_Boolean(value)
        value >>= 1
        flags.hasIconLocation = Get_Boolean(value)
        value >>= 1
        flags.isUnicode = Get_Boolean(value)
        value >>= 1
        flags.forceNoLinkInfo = Get_Boolean(value)
        value >>= 1
        flags.hasExpString = Get_Boolean(value)
        value >>= 1
        flags.runInSeparateProcess = Get_Boolean(value)
        value >>= 1
        flags.unused1 = Get_Boolean(value)
        value >>= 1
        flags.hasDarwinID = Get_Boolean(value)
        value >>= 1
        flags.runAsUser = Get_Boolean(value)
        value >>= 1
        flags.hasExpIcon = Get_Boolean(value)
        value >>= 1
        flags.noPidlAlias = Get_Boolean(value)
        value >>= 1
        flags.unused2 = Get_Boolean(value)
        value >>= 1
        flags.runWithShimLayer = Get_Boolean(value)
        value >>= 1
        flags.forceNoLinkTrack = Get_Boolean(value)
        value >>= 1
        flags.enableTargetMetadata = Get_Boolean(value)
        value >>= 1
        flags.disableLinkPathTracking = Get_Boolean(value)
        value >>= 1
        flags.disableKnownFolderAlias = Get_Boolean(value)
        value >>= 1
        flags.allowLinkToLink = Get_Boolean(value)
        value >>= 1
        flags.unaliasOnSave = Get_Boolean(value)
        value >>= 1
        flags.preferEnvironmentPath = Get_Boolean(value)
        value >>= 1
        flags.keepLocalIDListForUNCTarget = Get_Boolean(value)

        Return flags
    End Function
    Private Function Read_FileAttribute(value As UInteger) As FILE_ATTRIBUTE_FLAGS
        Dim flags As New FILE_ATTRIBUTE_FLAGS()

        flags.[readOnly] = Get_Boolean(value)
        value >>= 1
        flags.hidden = Get_Boolean(value)
        value >>= 1
        flags.system = Get_Boolean(value)
        value >>= 1
        flags.reserved1 = Get_Boolean(value)
        value >>= 1
        flags.directory = Get_Boolean(value)
        value >>= 1
        flags.archive = Get_Boolean(value)
        value >>= 1
        flags.reserved2 = Get_Boolean(value)
        value >>= 1
        flags.normal = Get_Boolean(value)
        value >>= 1
        flags.temporary = Get_Boolean(value)
        value >>= 1
        flags.sparse_file = Get_Boolean(value)
        value >>= 1
        flags.compressed = Get_Boolean(value)
        value >>= 1
        flags.offline = Get_Boolean(value)
        value >>= 1
        flags.offline = Get_Boolean(value)
        value >>= 1
        flags.not_content_indexed = Get_Boolean(value)
        value >>= 1
        flags.encrypted = Get_Boolean(value)
        value >>= 1

        Return flags
    End Function

    Private Function Read_LinkIDList(br As BinaryReader) As LINKTARGET_IDLIST
        Dim idlist As New LINKTARGET_IDLIST()

        idlist.IDListSize = br.ReadUInt16()
        idlist.IDList = Read_IDList(br)

        Return idlist
    End Function
    Private Function Read_IDList(br As BinaryReader) As IDLIST
        Dim idlist As New IDLIST()
        idlist.itemIDList = New List(Of ITEM_IDLIST)()

        Dim size As UShort = br.ReadUInt16()
        While size <> 0
            Dim item As New ITEM_IDLIST()
            item.itemIDSize = size
            item.data = br.ReadBytes(size - 2)
            idlist.itemIDList.Add(item)

            size = br.ReadUInt16()
        End While
        idlist.terminalID = size

        Return idlist
    End Function
    Private Function Read_LinkInfo(br As BinaryReader) As LINKINFO
        Dim info As New LINKINFO()
        Dim info_pos As UInteger = CUInt(br.BaseStream.Position)

        info.linkInfoSize = br.ReadUInt32()
        info.linkInfoHeaderSize = br.ReadUInt32()

        Dim value As UInteger = br.ReadUInt32()
        info.volumeIDAndLocalBasePath = Get_Boolean(value)
        value >>= 1
        info.commonNetworkRelativeLinkAndPathSuffix = Get_Boolean(value)

        info.volumeIDOffset = br.ReadUInt32()
        info.localBasePathOffset = br.ReadUInt32()
        info.commonNetworkRelativeLinkOffset = br.ReadUInt32()
        info.commonPathSuffixOffset = br.ReadUInt32()

        If info.linkInfoHeaderSize >= &H24 Then
            info.localBasePathOffsetUnicode = br.ReadUInt32()
            info.commonPathSuffixOffsetUnicode = br.ReadUInt32()
        End If

        If info.volumeIDAndLocalBasePath Then
            ' Volume ID
            Dim volumeID_pos As UInteger = info_pos + info.volumeIDOffset
            br.BaseStream.Position = volumeID_pos
            info.volumeID = New VOLUMEID()

            info.volumeID.volumeIDSize = br.ReadUInt32()
            info.volumeID.driveType = CType(br.ReadUInt32(), DRIVE_TYPE)
            info.volumeID.driveSerialNumber = br.ReadUInt32()
            info.volumeID.volumeLabelOffset = br.ReadUInt32()

            If info.volumeID.volumeLabelOffset <> &H14 Then
                info.volumeID.data = Get_String(br, False, volumeID_pos + info.volumeID.volumeLabelOffset)
            Else
                info.volumeID.volumeLabelOffsetUnicode = br.ReadUInt32()
                info.volumeID.data = Get_String(br, True, volumeID_pos + info.volumeID.volumeLabelOffsetUnicode)
            End If

            ' Local Base Path
            info.localBasePath = Get_String(br, False, info_pos + info.localBasePathOffset)
        End If

        If info.commonNetworkRelativeLinkAndPathSuffix Then
            ' Common Network Relative Link
            Dim cnrl_pos As UInteger = info_pos + info.commonNetworkRelativeLinkOffset
            info.cnrl = New COMMON_NETWORK_RELATIVE_LINK()

            info.cnrl.cnrl_size = br.ReadUInt32()
            Dim flags As UInteger = br.ReadUInt32()
            info.cnrl.validDevice = Get_Boolean(flags)
            flags >>= 1
            info.cnrl.validNetType = Get_Boolean(flags)
            info.cnrl.netNameOffset = br.ReadUInt32()
            info.cnrl.deviceNameOffset = br.ReadUInt32()

            Dim networkprovider As UInteger = br.ReadUInt32()
            If info.cnrl.validNetType Then
                info.cnrl.networkProviderType = CType(networkprovider, PROVIDER_TYPE)
            End If

            If info.cnrl.netNameOffset > &H14 Then
                info.cnrl.netNameOffsetUnicode = br.ReadUInt32()
                info.cnrl.deviceNameOffsetUnicode = br.ReadUInt32()
            End If

            info.cnrl.netName = Get_String(br, False, cnrl_pos + info.cnrl.netNameOffset)
            If info.cnrl.validDevice Then
                info.cnrl.deviceName = Get_String(br, False, cnrl_pos + info.cnrl.deviceNameOffset)
            End If
            If info.cnrl.netNameOffset > &H14 Then
                info.cnrl.netNameUnicode = Get_String(br, True, cnrl_pos + info.cnrl.netNameOffsetUnicode)
                info.cnrl.deviceNameUnicode = Get_String(br, True, cnrl_pos + info.cnrl.deviceNameOffsetUnicode)
            End If
        End If

        info.commonPathSuffix = Get_String(br, False, info_pos + info.commonPathSuffixOffset)

        If info.linkInfoHeaderSize >= &H24 AndAlso info.volumeIDAndLocalBasePath Then
            info.localBasePathUnicode = Get_String(br, True, info_pos + info.localBasePathOffsetUnicode)
        End If
        If info.linkInfoHeaderSize >= &H24 Then
            info.commonPathSuffixUnicode = Get_String(br, True, info_pos + info.commonPathSuffixOffsetUnicode)
        End If

        Return info
    End Function
    Private Function Read_StringData(br As BinaryReader) As STRING_DATA
        Dim sdata As New STRING_DATA()

        If lnk.header.linkFlags.hasName Then
            sdata.nameString = New NAME_STRING()
            sdata.nameString.countCharacters = br.ReadUInt16()
            sdata.nameString.value = Get_String(br, sdata.nameString.countCharacters, True)
        End If
        If lnk.header.linkFlags.hasRelativePath Then
            sdata.relativePath = New RELATIVE_PATH()
            sdata.relativePath.countCharacters = br.ReadUInt16()
            sdata.relativePath.value = Get_String(br, sdata.relativePath.countCharacters, True)
        End If
        If lnk.header.linkFlags.hasWorkingDir Then
            sdata.workingDir = New WORKING_DIR()
            sdata.workingDir.countCharacters = br.ReadUInt16()
            sdata.workingDir.value = Get_String(br, sdata.workingDir.countCharacters, True)
        End If
        If lnk.header.linkFlags.hasArguments Then
            sdata.commandLineArgs = New COMMAND_LINE_ARGUMENTS()
            sdata.commandLineArgs.countCharacters = br.ReadUInt16()
            sdata.commandLineArgs.value = Get_String(br, sdata.commandLineArgs.countCharacters, True)
        End If
        If lnk.header.linkFlags.hasIconLocation Then
            sdata.iconLocation = New ICON_LOCATION()
            sdata.iconLocation.countCharacters = br.ReadUInt16()
            sdata.iconLocation.value = Get_String(br, sdata.iconLocation.countCharacters, True)
        End If

        Return sdata
    End Function
    Private Function Read_Extra(br As BinaryReader) As EXTRA_DATA
        Dim extra As New EXTRA_DATA()

        While True
            Dim size As UInteger = br.ReadUInt32()
            If size < &H4 Then
                extra.terminal.terminal = size
                Return extra
            End If

            Dim sign As UInteger = br.ReadUInt32()
            Select Case sign
                Case &HA0000001UI
                    extra.environment.blockSize = size
                    extra.environment.blockSignature = sign
                    extra.environment.targetAnsi = Get_String(br, 260, False)
                    extra.environment.targetUnicode = Get_String(br, 260, True)
                    Exit Select

                Case &HA0000002UI
                    Dim cp As New CONSOLE_PROPS()
                    cp.blockSize = size
                    cp.blockSignature = sign
                    cp.fillAttributes = CType(br.ReadUInt16(), FILL_ATTRIBUTES)
                    cp.popupFillAttributes = br.ReadUInt16()
                    cp.screenBufferSizeX = br.ReadUInt16()
                    cp.screenBufferSizeY = br.ReadUInt16()
                    cp.windowSizeX = br.ReadUInt16()
                    cp.windowSizeY = br.ReadUInt16()
                    cp.windowOriginX = br.ReadUInt16()
                    cp.windowOriginY = br.ReadUInt16()
                    cp.unused1 = br.ReadUInt32()
                    cp.unused2 = br.ReadUInt32()
                    cp.fontSize = br.ReadUInt32()
                    cp.fontFamily = CType(br.ReadUInt32(), FONT_FAMILY)
                    cp.fontWeight = br.ReadUInt32()
                    cp.faceName = Get_String(br, 32, True)
                    cp.cursorSize = br.ReadUInt32()
                    cp.fullScreen = br.ReadUInt32()
                    cp.quickEdit = br.ReadUInt32()
                    cp.insertMode = br.ReadUInt32()
                    cp.autoPosition = br.ReadUInt32()
                    cp.historyBufferSize = br.ReadUInt32()
                    cp.numberOfHistoryBuffer = br.ReadUInt32()
                    cp.historyNoDup = br.ReadUInt32()
                    cp.colorTable = New UInteger(15) {}
                    For i As Integer = 0 To 15
                        cp.colorTable(i) = br.ReadUInt32()
                    Next
                    extra.console = cp
                    Exit Select

                Case &HA0000003UI
                    extra.tracker.blockSize = size
                    extra.tracker.blockSignature = sign
                    extra.tracker.length = br.ReadUInt32()
                    extra.tracker.version = br.ReadUInt32()
                    extra.tracker.machineID = Get_String(br, &H10, False)
                    extra.tracker.droid = br.ReadBytes(&H20)
                    extra.tracker.droidBirth = br.ReadBytes(&H20)
                    Exit Select

                Case &HA0000004UI
                    extra.consoleFe.blockSize = size
                    extra.consoleFe.blockSignature = sign
                    extra.consoleFe.codePage = br.ReadUInt32()
                    Exit Select

                Case &HA0000005UI
                    extra.specialFolder.blockSize = size
                    extra.specialFolder.blockSignature = sign
                    extra.specialFolder.specialFolderID = br.ReadUInt32()
                    extra.specialFolder.offset = br.ReadUInt32()
                    Exit Select

                Case &HA0000006UI
                    extra.darwin.blockSize = size
                    extra.darwin.blockSignature = sign
                    extra.darwin.darwinDataAnsi = Get_String(br, 260, False)
                    extra.darwin.darwinDataUnicode = Get_String(br, 260, True)
                    Exit Select

                Case &HA0000007UI
                    extra.iconEnvironment.blockSize = size
                    extra.iconEnvironment.blockSignature = sign
                    extra.iconEnvironment.targetAnsi = Get_String(br, 260, False)
                    extra.iconEnvironment.targetUnicode = Get_String(br, 260, True)
                    Exit Select

                Case &HA0000008UI
                    extra.shim.blockSize = size
                    extra.shim.blockSignature = sign
                    extra.shim.layerName = Get_String(br, CInt(extra.shim.blockSize) - 8, True)
                    Exit Select

                Case &HA0000009UI
                    extra.propertyStore.blockSize = size
                    extra.propertyStore.blockSignature = sign
                    extra.propertyStore.propertyStore = Nothing
                    Exit Select

                Case &HA000000BUI
                    extra.knownFolder.blockSize = size
                    extra.knownFolder.blockSignature = sign
                    extra.knownFolder.knownFolderID = br.ReadBytes(&H10)
                    extra.knownFolder.offset = br.ReadUInt32()
                    Exit Select

                Case &HA000000CUI
                    extra.vistaIDList.blockSize = size
                    extra.vistaIDList.blockSignature = sign
                    extra.vistaIDList.idlist = Read_IDList(br)
                    Exit Select
            End Select
        End While
        Return Nothing
    End Function

    Private Function Get_Boolean(value As UInteger) As Boolean
        Dim v As UInteger = CUInt(value And 1)
        Return (If(v = 0, False, True))
    End Function
    Private Function Get_String(br As BinaryReader, unicode As Boolean, Optional offset As UInteger = 0) As String
        If offset <> 0 Then
            br.BaseStream.Position = offset
        End If

        Dim t As String = ""
        Dim c As Char
        While True
            If unicode Then
                c = Encoding.Unicode.GetChars(br.ReadBytes(2))(0)
            Else
                c = br.ReadChar()
            End If

            If c = ControlChars.NullChar Then
                Exit While
            End If

            t += c
        End While

        Return t
    End Function
    Private Function Get_String(br As BinaryReader, size As Integer, unicode As Boolean) As String
        If Not unicode Then
            Return New String(Encoding.[Default].GetChars(br.ReadBytes(size)))
        Else
            Return New String(Encoding.Unicode.GetChars(br.ReadBytes(size * 2)))
        End If
    End Function

    Public Shared Function Check(fileIn As String) As Boolean
        Dim br As New BinaryReader(File.OpenRead(fileIn))

        Dim hsize As UInteger = br.ReadUInt32()
        If hsize <> &H4C Then
            Return False
        End If

        Dim linkCLSID As Byte() = br.ReadBytes(&H10)
        For i As Integer = 0 To 15
            If linkCLSID(i) <> CLSID(i) Then
                Return False
            End If
        Next

        br.Close()
        br = Nothing

        Return True
    End Function

    Public ReadOnly Property Path() As String
        Get
            Return lnk.info.commonPathSuffix & lnk.info.localBasePath
        End Get
    End Property
    Public ReadOnly Property FileAttribute() As FILE_ATTRIBUTE_FLAGS
        Get
            Return lnk.header.fileAttributes
        End Get
    End Property

#Region "Structures"
    Public Structure SHELL_LINK
        Public header As SHELL_LINK_HEADER
        Public idlist As LINKTARGET_IDLIST
        ' Optional
        Public info As LINKINFO
        ' Optional
        Public sdata As STRING_DATA
        ' Optional
        Public extra As EXTRA_DATA
        ' Optional
    End Structure

    Public Structure SHELL_LINK_HEADER
        Public headerSize As UInteger
        ' Must be 0x4C
        Public linkCLSID As Byte()
        ' Must be 00021401-0000-0000-C000-000000000046.
        Public linkFlags As LINK_FLAGS
        Public fileAttributes As FILE_ATTRIBUTE_FLAGS
        Public creationTime As FILE_TIME
        Public accessTime As FILE_TIME
        Public writeTime As FILE_TIME
        Public fileSize As UInteger
        Public iconIndex As Integer
        Public showCommand As SHOW_COMMAND
        Public hotKey As HOTKEYS_FLAGS
        Public reserved1 As UShort
        ' Must be 00
        Public reserved2 As UInteger
        ' Must be 00
        Public reserved3 As UInteger
        ' Must be 00
    End Structure
    Public Structure LINK_FLAGS
        ' 4 bytes
        Public hasLinkTargetIDList As Boolean
        Public hasLinkInfo As Boolean
        Public hasName As Boolean
        Public hasRelativePath As Boolean
        Public hasWorkingDir As Boolean
        Public hasArguments As Boolean
        Public hasIconLocation As Boolean
        Public isUnicode As Boolean
        ' Should be true
        Public forceNoLinkInfo As Boolean
        ' LinkInfo ignored
        Public hasExpString As Boolean
        Public runInSeparateProcess As Boolean
        Public unused1 As Boolean
        ' Must be ignored
        Public hasDarwinID As Boolean
        Public runAsUser As Boolean
        Public hasExpIcon As Boolean
        Public noPidlAlias As Boolean
        Public unused2 As Boolean
        ' Must be ignored
        Public runWithShimLayer As Boolean
        Public forceNoLinkTrack As Boolean
        Public enableTargetMetadata As Boolean
        Public disableLinkPathTracking As Boolean
        Public diableKnownFolderTracking As Boolean
        Public disableKnownFolderAlias As Boolean
        Public allowLinkToLink As Boolean
        Public unaliasOnSave As Boolean
        Public preferEnvironmentPath As Boolean
        Public keepLocalIDListForUNCTarget As Boolean
    End Structure
    Public Structure FILE_ATTRIBUTE_FLAGS
        ' 4 bytes
        Public [readOnly] As Boolean
        Public hidden As Boolean
        Public system As Boolean
        Public reserved1 As Boolean
        ' Must be 0
        Public directory As Boolean
        Public archive As Boolean
        Public reserved2 As Boolean
        ' Must be 0
        Public normal As Boolean
        Public temporary As Boolean
        Public sparse_file As Boolean
        Public reparse_point As Boolean
        Public compressed As Boolean
        Public offline As Boolean
        Public not_content_indexed As Boolean
        Public encrypted As Boolean
    End Structure
    Public Structure FILE_TIME
        ' 8 bytes
        ' FROM: http://msdn.microsoft.com/en-us/library/cc230273%28v=prot.10%29.aspx
        ' "The FILETIME structure is a 64-bit value that represents the number of
        ' 100-nanosecond intervals that have elapsed since January 1, 1601, Coordinated Universal Time (UTC)."

        'uint dwLowDateTime;
        'uint dwHightDateTime;
        Public dateTime As ULong
    End Structure
    Public Enum SHOW_COMMAND As UInteger
        SW_SHOWNORMAL = &H1
        ' Default
        SW_SHOWMAXIMIZED = &H3
        SW_SHOWMINNOACTIVE = &H7
    End Enum
    Public Structure HOTKEYS_FLAGS
        ' 2 bytes
        Public low As LOW_BYTE
        Public hight As HIGH_BYTE

        Public Enum LOW_BYTE As Byte
            K_0 = &H30
            K_1 = &H31
            K_2 = &H32
            K_3 = &H33
            K_4 = &H34
            K_5 = &H35
            K_6 = &H36
            K_7 = &H37
            K_8 = &H38
            K_9 = &H39
            K_A = &H41
            K_B = &H42
            K_C = &H43
            K_D = &H44
            K_E = &H45
            K_F = &H46
            K_G = &H47
            K_H = &H48
            K_I = &H49
            K_J = &H4A
            K_K = &H4B
            K_L = &H4C
            K_M = &H4D
            K_N = &H4E
            K_O = &H4F
            K_P = &H50
            K_Q = &H51
            K_R = &H52
            K_S = &H53
            K_T = &H54
            K_U = &H55
            K_V = &H56
            K_W = &H57
            K_X = &H58
            K_Y = &H59
            K_Z = &H5A
            VK_F1 = &H70
            VK_F2 = &H71
            VK_F3 = &H72
            VK_F4 = &H73
            VK_F5 = &H74
            VK_F6 = &H75
            VK_F7 = &H76
            VK_F8 = &H77
            VK_F9 = &H78
            VK_F10 = &H79
            VK_F11 = &H7A
            VK_F12 = &H7B
            VK_F13 = &H7C
            VK_F14 = &H7D
            VK_F15 = &H7E
            VK_F16 = &H7F
            VK_F17 = &H80
            VK_F18 = &H81
            VK_F19 = &H82
            VK_F20 = &H83
            VK_F21 = &H84
            VK_F22 = &H85
            VK_F23 = &H86
            VK_F24 = &H87
            VK_NUMLOCK = &H90
            VK_SCROLL = &H91
        End Enum
        Public Enum HIGH_BYTE As Byte
            HOTKEYF_SHIFT = &H1
            HOTKEYF_CONTROL = &H2
            HOTKEYF_ALT = &H4
        End Enum
    End Structure

    Public Structure LINKTARGET_IDLIST
        ' The presence of this optional structure is
        ' specified by the HasLinkTargetIDList bit
        Public IDListSize As UShort
        Public IDList As IDLIST
    End Structure
    Public Structure IDLIST
        Public itemIDList As List(Of ITEM_IDLIST)
        Public terminalID As UShort
        ' Must be 0000
    End Structure
    Public Structure ITEM_IDLIST
        Public itemIDSize As UShort
        Public data As Byte()
    End Structure

    Public Structure LINKINFO
        Public linkInfoSize As UInteger
        Public linkInfoHeaderSize As UInteger
        ' 0x1C->no optional fields; >= 0x24 optional fields
        ' Flags, in total 4 bytes
        Public volumeIDAndLocalBasePath As Boolean
        Public commonNetworkRelativeLinkAndPathSuffix As Boolean

        ' Offsets
        Public volumeIDOffset As UInteger
        Public localBasePathOffset As UInteger
        Public commonNetworkRelativeLinkOffset As UInteger
        Public commonPathSuffixOffset As UInteger
        Public localBasePathOffsetUnicode As UInteger
        Public commonPathSuffixOffsetUnicode As UInteger

        Public volumeID As VOLUMEID
        Public localBasePath As String
        ' NULL-terminated
        Public cnrl As COMMON_NETWORK_RELATIVE_LINK
        Public commonPathSuffix As String
        ' NULL-terminated
        Public localBasePathUnicode As String
        ' UNICODE & NULL-terminated
        Public commonPathSuffixUnicode As String
        ' UNICODE & NULL-terminated
    End Structure
    Public Structure VOLUMEID
        Public volumeIDSize As UInteger
        ' MUST be > 0x10
        Public driveType As DRIVE_TYPE
        Public driveSerialNumber As UInteger
        Public volumeLabelOffset As UInteger
        ' NULL-terminated
        Public volumeLabelOffsetUnicode As UInteger
        ' UNICODE & NULL-terminated
        Public data As String
    End Structure
    Public Enum DRIVE_TYPE As UInteger
        DRIVE_UNKNOWN = &H0
        DRIVE_NO_ROOT_DIR = &H1
        DRIVE_REMOVABLE = &H2
        DRIVE_FIXED = &H3
        DRIVE_REMOTE = &H4
        DRIVE_CDROM = &H5
        DRIVE_RAMDISK = &H6
    End Enum
    Public Structure COMMON_NETWORK_RELATIVE_LINK
        Public cnrl_size As UInteger

        ' Flags - 4 bytes
        Public validDevice As Boolean
        Public validNetType As Boolean

        ' Offsets
        Public netNameOffset As UInteger
        Public deviceNameOffset As UInteger
        Public networkProviderType As PROVIDER_TYPE
        Public netNameOffsetUnicode As UInteger
        Public deviceNameOffsetUnicode As UInteger

        Public netName As String
        ' Null-terminated
        Public deviceName As String
        ' Null-terminated
        Public netNameUnicode As String
        ' Unicode & Null-terminated
        Public deviceNameUnicode As String
        ' Unicode & Null-terminated
    End Structure
    Public Enum PROVIDER_TYPE As UInteger
        WNNC_NET_AVID = &H1A0000
        WNNC_NET_DOCUSPACE = &H1B0000
        WNNC_NET_MANGOSOFT = &H1C0000
        WNNC_NET_SERNET = &H1D0000
        WNNC_NET_RIVERFRONT1 = &H1E0000
        WNNC_NET_RIVERFRONT2 = &H1F0000
        WNNC_NET_DECORB = &H200000
        WNNC_NET_PROTSTOR = &H210000
        WNNC_NET_FJ_REDIR = &H220000
        WNNC_NET_DISTINCT = &H230000
        WNNC_NET_TWINS = &H240000
        WNNC_NET_RDR2SAMPLE = &H250000
        WNNC_NET_CSC = &H260000
        WNNC_NET_3IN1 = &H270000
        WNNC_NET_EXTENDNET = &H290000
        WNNC_NET_STAC = &H2A0000
        WNNC_NET_FOXBAT = &H2B0000
        WNNC_NET_YAHOO = &H2C0000
        WNNC_NET_EXIFS = &H2D0000
        WNNC_NET_DAV = &H2E0000
        WNNC_NET_KNOWARE = &H2F0000
        WNNC_NET_OBJECT_DIRE = &H30000
        WNNC_NET_MASFAX = &H310000
        WNNC_NET_HOB_NFS = &H320000
        WNNC_NET_SHIVA = &H330000
        WNNC_NET_IBMAL = &H340000
        WNNC_NET_LOCK = &H350000
        WNNC_NET_TERMSRV = &H360000
        WNNC_NET_SRT = &H370000
        WNNC_NET_QUINCY = &H380000
        WNNC_NET_OPENAFS = &H390000
        WNNC_NET_AVID1 = &H3A0000
        WNNC_NET_DFS = &H3B0000
        WNNC_NET_KWNP = &H3C0000
        WNNC_NET_ZENWORKS = &H3D0000
        WNNC_NET_DRIVEONWEB = &H3E0000
        WNNC_NET_VMWARE = &H3F0000
        WNNC_NET_RSFX = &H400000
        WNNC_NET_MFILES = &H410000
        WNNC_NET_MS_NFS = &H420000
        WNNC_NET_GOOGLE = &H430000
    End Enum

    Public Structure STRING_DATA
        Public nameString As NAME_STRING
        Public relativePath As RELATIVE_PATH
        Public workingDir As WORKING_DIR
        Public commandLineArgs As COMMAND_LINE_ARGUMENTS
        Public iconLocation As ICON_LOCATION
    End Structure
    Public Structure NAME_STRING
        Public countCharacters As UShort
        Public value As String
    End Structure
    Public Structure RELATIVE_PATH
        Public countCharacters As UShort
        Public value As String
    End Structure
    Public Structure WORKING_DIR
        Public countCharacters As UShort
        Public value As String
    End Structure
    Public Structure COMMAND_LINE_ARGUMENTS
        Public countCharacters As UShort
        Public value As String
    End Structure
    Public Structure ICON_LOCATION
        Public countCharacters As UShort
        Public value As String
    End Structure

    Public Structure EXTRA_DATA
        Public console As CONSOLE_PROPS
        Public consoleFe As CONSOLE_FE_PROPS
        Public darwin As DRAWIN_PROPS
        Public environment As ENVIRONMENT_PROPS
        Public iconEnvironment As ICON_ENVIRONMENT_PROPS
        Public knownFolder As KNOWN_FOLDER_PROPS
        Public propertyStore As PROPERTY_STORE_PROPS
        Public shim As SHIM_PROPS
        Public specialFolder As SPECIAL_FOLDER_PROPS
        Public tracker As TRACKER_PROPS
        Public vistaIDList As VISTA_AND_ABOVE_IDLIST_PROPS
        Public terminal As TERMINAL_BLOCK
    End Structure
    Public Structure CONSOLE_PROPS
        Public blockSize As UInteger
        ' MUST be 0xCC
        Public blockSignature As UInteger
        ' MUST be 0xA0000002
        Public fillAttributes As FILL_ATTRIBUTES
        Public popupFillAttributes As UShort
        Public screenBufferSizeX As UShort
        Public screenBufferSizeY As UShort
        Public windowSizeX As UShort
        Public windowSizeY As UShort
        Public windowOriginX As UShort
        Public windowOriginY As UShort
        Public unused1 As UInteger
        Public unused2 As UInteger
        Public fontSize As UInteger
        Public fontFamily As FONT_FAMILY
        Public fontWeight As UInteger
        ' More than 700 bold font
        Public faceName As String
        ' 64 bytes UNICODE
        Public cursorSize As UInteger
        Public fullScreen As UInteger
        ' Different to 0 -> Full-screen
        Public quickEdit As UInteger
        ' Different to 0 -> ON
        Public insertMode As UInteger
        ' Different to 0 -> ON
        Public autoPosition As UInteger
        ' Different to 0 -> Auto
        Public historyBufferSize As UInteger
        Public numberOfHistoryBuffer As UInteger
        Public historyNoDup As UInteger
        ' Different to 0 -> Allowed
        Public colorTable As UInteger()
        ' RGB 32-bits colors
    End Structure
    Public Enum FILL_ATTRIBUTES As UShort
        FOREGROUND_BLUE = &H1
        FOREGROUND_GREEN = &H2
        FOREGROUND_RED = &H4
        FOREGROUND_INTENSITY = &H8
        BACKGROUND_BLUE = &H10
        BACKGROUND_GREEN = &H20
        BACKGROUND_RED = &H40
        BACKGROUND_INTENSITY = &H80
    End Enum
    Public Enum FONT_FAMILY As UInteger
        FF_DONTCARE = &H0
        FF_ROMAN = &H10
        FF_SWISS = &H20
        FF_MODERN = &H30
        FF_SCRIPT = &H40
        FF_DECORATIVE = &H50
    End Enum
    Public Structure CONSOLE_FE_PROPS
        Public blockSize As UInteger
        ' MUST be 0xC
        Public blockSignature As UInteger
        ' MUST be 0xA0000004
        Public codePage As UInteger
    End Structure
    Public Structure DRAWIN_PROPS
        Public blockSize As UInteger
        ' Must be 0x314
        Public blockSignature As UInteger
        ' Must be 0xA0000006
        Public darwinDataAnsi As String
        ' 260 bytes, Null-terminated
        Public darwinDataUnicode As String
        ' 520 bytes Unicode, Null-terminated
    End Structure
    Public Structure ENVIRONMENT_PROPS
        Public blockSize As UInteger
        ' Must be 0x314
        Public blockSignature As UInteger
        ' Must be 0xA0000001
        Public targetAnsi As String
        ' 260 bytes, null-terminated
        Public targetUnicode As String
        ' 520 bytes, null-terminated, unicode
    End Structure
    Public Structure ICON_ENVIRONMENT_PROPS
        Public blockSize As UInteger
        ' Must be 0x314
        Public blockSignature As UInteger
        ' Must be 0xA0000007
        Public targetAnsi As String
        ' 260 bytes, null-terminated
        Public targetUnicode As String
        ' 520 bytes, null-terminated, unicode
    End Structure
    Public Structure KNOWN_FOLDER_PROPS
        Public blockSize As UInteger
        ' Must be 0x1C
        Public blockSignature As UInteger
        ' Must be 0xA000000B
        Public knownFolderID As Byte()
        Public offset As UInteger
    End Structure
    Public Structure PROPERTY_STORE_PROPS
        Public blockSize As UInteger
        ' Must be >= 0x0C
        Public blockSignature As UInteger
        ' Mst be 0xA0000009
        Public propertyStore As Byte()
    End Structure
    Public Structure SHIM_PROPS
        Public blockSize As UInteger
        ' Must be >= 0x88
        Public blockSignature As UInteger
        ' Must be 0xA0000008
        Public layerName As String
        ' Unicode
    End Structure
    Public Structure SPECIAL_FOLDER_PROPS
        Public blockSize As UInteger
        ' Must be 0x10
        Public blockSignature As UInteger
        ' Must be 0xA0000005
        Public specialFolderID As UInteger
        Public offset As UInteger
    End Structure
    Public Structure TRACKER_PROPS
        Public blockSize As UInteger
        ' Must be 0x60
        Public blockSignature As UInteger
        ' Must be 0xA0000003
        Public length As UInteger
        ' Must be >= 0x58
        Public version As UInteger
        ' Must be 0x00
        Public machineID As String
        Public droid As Byte()
        ' Two GUID
        Public droidBirth As Byte()
        ' Two GUID
    End Structure
    Public Structure VISTA_AND_ABOVE_IDLIST_PROPS
        Public blockSize As UInteger
        ' Must be >= 0x0A
        Public blockSignature As UInteger
        ' Must be 0xA000000C
        Public idlist As IDLIST
    End Structure
    Public Structure TERMINAL_BLOCK
        Public terminal As UInteger
        ' Less than 0x04
    End Structure

    Shared CLSID As Byte() = {&H1, &H14, &H2, &H0, &H0, &H0, _
     &H0, &H0, &HC0, &H0, &H0, &H0, _
     &H0, &H0, &H0, &H46}
#End Region

End Class

