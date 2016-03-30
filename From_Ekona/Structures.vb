
' Copyright (C) 2011  pleoNeX
' Nukepayload2 ÐÞ¸Ä
Imports System.Collections.Generic
Imports System.Drawing


Public Structure sFile
    Public offset As UInt32
    ' Offset where the files inside of the file in path
    Public size As UInt32
    ' Length of the file
    Public name As String
    ' File name
    Public id As UInt16
    ' Internal id
    Public path As String
    ' Path where the file is
    Public format As Format
    ' Format file 
    Public tag As Object
    ' Extra information
End Structure
Public Structure sFolder
    Public files As List(Of sFile)
    ' List of files
    Public folders As List(Of sFolder)
    ' List of folders
    Public name As String
    ' Folder name
    Public id As UInt16
    ' Internal id
    Public tag As Object
    ' Extra information
End Structure


Public Enum Format
    Palette
    Tile
    Map
    Cell
    Animation
    FullImage
    Text
    Video
    Sound
    Font
    Compressed
    Unknown
    System
    Script
    Pack
    Model3D
    Texture
End Enum
Public Enum FormatCompress
    ' From DSDecmp
    LZOVL
    ' keep this as the first one, as only the end of a file may be LZ-ovl-compressed (and overlay files are oftenly double-compressed)
    LZ10
    LZ11
    HUFF4
    HUFF8
    RLE
    HUFF
    NDS
    GBA
    Invalid
End Enum

Public Structure NTFS
    ' Nintedo Tile Format Screen
    Public nPalette As Byte
    ' The parameters (two bytes) is PPPP Y X NNNNNNNNNN
    Public xFlip As Byte
    Public yFlip As Byte
    Public nTile As UShort
End Structure
Public Structure NTFT
    ' Nintendo Tile Format Tile
    Public tiles As Byte()
    Public nPalette As Byte()
    ' Number of the palette that this tile uses
End Structure

Public Structure NitroHeader
    ' Generic Header in Nitro formats
    Public id As Char()
    Public endianess As UInt16
    ' 0xFFFE -> little endian
    Public constant As UInt16
    ' Always 0x0100
    Public file_size As UInt32
    Public header_size As UInt16
    ' Always 0x10
    Public nSection As UInt16
    ' Number of sections
End Structure

