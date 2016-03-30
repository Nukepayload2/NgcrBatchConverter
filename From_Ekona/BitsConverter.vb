' ----------------------------------------------------------------------
' <copyright file="BitConverter.cs" company="none">

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
' <date>24/06/2012 14:28:44</date>
' -----------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Public NotInheritable Class BitsConverter
    Private Sub New()
    End Sub
    ' From Byte
    Public Shared Function ByteToBits(data As Byte) As Byte()
        Dim bits As List(Of Byte) = New List(Of Byte)()

        For j As Integer = 7 To 0 Step -1
            bits.Add(CByte((data >> j) And 1))
        Next

        Return bits.ToArray()
    End Function
    Public Shared Function ByteToBit2(data As Byte) As Byte()
        Dim bit2 As Byte() = New Byte(3) {}

        bit2(0) = CByte(data And &H3)
        bit2(1) = CByte((data >> 2) And &H3)
        bit2(2) = CByte((data >> 4) And &H3)
        bit2(3) = CByte((data >> 6) And &H3)

        Return bit2
    End Function
    Public Shared Function ByteToBit4(data As Byte) As Byte()
        Dim bit4 As Byte() = New Byte(1) {}

        bit4(0) = CByte(data And &HF)
        bit4(1) = CByte((data And &HF0) >> 4)

        Return bit4
    End Function
    Public Shared Function BytesToBit4(data As Byte()) As Byte()
        Dim bit4 As Byte() = New Byte(data.Length * 2 - 1) {}
        For i As Integer = 0 To data.Length - 1
            Dim b4 As Byte() = ByteToBit4(data(i))
            bit4(i * 2) = b4(0)
            bit4(i * 2 + 1) = b4(1)
        Next
        Return bit4
    End Function
    Public Shared Function BytesToHexString(bytes As Byte()) As [String]
        Dim result As String = ""

        For i As Integer = 0 To bytes.Length - 1
            result += [String].Format("{0:X}", bytes(i))
        Next

        Return result
    End Function

    ' To Byte
    Public Shared Function BitsToBytes(bits As Byte()) As Byte()
        Dim bytes As List(Of Byte) = New List(Of Byte)()

        For i As Integer = 0 To bits.Length - 1 Step 8
            Dim newByte As Byte = 0
            Dim b As Integer = 0
            Dim j As Integer = 7
            While j >= 0
                newByte += CByte(bits(i + b) << j)
                j -= 1
                b += 1
            End While
            bytes.Add(newByte)
        Next

        Return bytes.ToArray()
    End Function
    Public Shared Function Bit4ToByte(data As Byte()) As Byte
        Return CByte(data(0) + (data(1) << 4))
    End Function
    Public Shared Function Bit4ToByte(b1 As Byte, b2 As Byte) As Byte
        Return CByte(b1 + (b2 << 4))
    End Function
    Public Shared Function Bits4ToByte(data As Byte()) As Byte()
        Dim b As Byte() = New Byte(data.Length \ 2 - 1) {}

        For i As Integer = 0 To data.Length - 1 Step 2
            b(i \ 2) = Bit4ToByte(data(i), data(i + 1))
        Next

        Return b
    End Function
    Public Shared Function StringToBytes(text As [String], num_bytes As Integer) As Byte()
        Dim hexText As String = text.Replace("-", "")
        hexText = hexText.PadRight(num_bytes * 2, "0"c)

        Dim hex As List(Of Byte) = New List(Of Byte)()
        For i As Integer = 0 To hexText.Length - 1 Step 2
            hex.Add(Convert.ToByte(hexText.Substring(i, 2), 16))
        Next

        Return hex.ToArray()
    End Function

End Class