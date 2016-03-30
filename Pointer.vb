Option Strict On
Imports System.Runtime.InteropServices.Marshal
Imports System.Runtime.InteropServices
Imports System.Reflection
'Copyright Nukepayload2 2014
'''<summary> 32位指向值类型的指针。使用时一定要小心，否则会导致不可预知的后果。 </summary>
Public Class Pointer(Of T As {Structure})
    Dim BaseAddress As Integer
    ''' <summary>
    ''' 目标元素的大小
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ObjSize As Integer
    '''<summary> 读写目标内存 </summary>
    Public Property Target As T
        Get
            Return CType(TargetElement(0), T)
        End Get
        Set(Value As T)
            TargetElement(0) = Value
        End Set
    End Property

    ''' <summary>获取或设置指向的目标(注意!如果是结构体,则必须带有以下特性:&lt;System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack:=1)&gt;)</summary>
    Default Public Property TargetElement(index As Integer) As T
        Set(s As T) '稳定
            Dim size As Integer = ObjSize
            If size = 0 Then Throw New InvalidOperationException("Void指针不支持此操作，请把ObjSize设置为大于0的值。")
            StructureToPtr(s, New IntPtr(BaseAddress + index * size), False) '为什么用False呢？因为s是托管内存里面的，直接释放会引发AccessViolationException。这样写不会内存泄漏。
        End Set
        Get '不稳定
            Dim size As Integer = ObjSize
            If size = 0 Then Throw New InvalidOperationException("Void指针不支持此操作，请把ObjSize设置为大于0的值。")
            Return DirectCast(PtrToStructure(New IntPtr(BaseAddress + size * index), GetType(T)), T) '拆箱
            End Get
    End Property

    '''<summary>当前的地址加偏移量</summary>
    Public ReadOnly Property Address(Offset As Integer) As Integer
        Get
            Return BaseAddress + Offset
        End Get
    End Property
    ''' <summary>
    ''' 指针指向的地址
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Address As Integer
        Get
            Return BaseAddress
        End Get
        Set(Value As Integer)
            BaseAddress = Value
        End Set
    End Property
    '各种运算符...
    Public Shared Narrowing Operator CType(p As Pointer(Of T)) As IntPtr
        Return New IntPtr(p.BaseAddress)
    End Operator
    Public Shared Narrowing Operator CType(p As Pointer(Of T)) As Integer
        Return p.BaseAddress
    End Operator
    ''' <summary>
    ''' 从指针新建.注意!进行操作后请务必把ObjSize属性设置为目标类型的大小！
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(p As IntPtr) As Pointer(Of T)
        Return New Pointer(Of T)(p, 0)
    End Operator
    ''' <summary>
    ''' 从指针新建.注意!进行操作后请务必把ObjSize属性设置为目标类型的大小！
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(p As Integer) As Pointer(Of T)
        Return New Pointer(Of T)(New IntPtr(p), 0)
    End Operator
    Public Shared Operator +(a As Pointer(Of T), b As Integer) As Pointer(Of T)
        Return New Pointer(Of T)(New IntPtr(a.Address + b), a.ObjSize)
    End Operator
    Public Shared Operator -(a As Pointer(Of T), b As Integer) As Pointer(Of T)
        Return New Pointer(Of T)(New IntPtr(a.Address - b), a.ObjSize)
    End Operator
    Public Shared Operator +(a As Pointer(Of T), b As IntPtr) As Pointer(Of T)
        Return New Pointer(Of T)(New IntPtr(a.Address + b.ToInt32), a.ObjSize)
    End Operator
    Public Shared Operator -(a As Pointer(Of T), b As IntPtr) As Pointer(Of T)
        Return New Pointer(Of T)(New IntPtr(a.Address - b.ToInt32), a.ObjSize)
    End Operator
    '''<summary>初始化一个用于操作值类型的指针。为了高效，结构体指针最好用结构体数组代替。文件指针请用对应的Stream代替。</summary>
    Sub New(Addr As IntPtr, Size As Integer)
        BaseAddress = Addr.ToInt32
        ObjSize = Size
    End Sub
    '''<summary>为值类型分配指针</summary>
    Sub New(obj As T)
        BaseAddress = VarPtr(obj).ToInt32
        ObjSize = SizeOf(GetType(T))
    End Sub
    ''' <summary>
    ''' 获取地址
    ''' </summary>
    ''' <typeparam name="A"></typeparam>
    ''' <param name="ele">要获取地址的元素。如果是非基元结构则引发异常。</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function VarPtr(Of A As Structure)(ele As A) As IntPtr
        Dim GC = GCHandle.Alloc(ele, GCHandleType.Pinned)
        Dim GC2 = GC.AddrOfPinnedObject
        GC.Free()
        Return GC2
    End Function
    ''' <summary>
    ''' 获取数组地址
    ''' </summary>
    ''' <param name="arr">要获取地址的数组</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function VarPtr(arr As Array) As IntPtr
        Return UnsafeAddrOfPinnedArrayElement(arr, 0)
    End Function

    ''' <summary>
    ''' 从数组新建指针
    ''' </summary>
    ''' <param name="arr">要转换成指针的数组</param>
    ''' <param name="TargetSize">指针对应的托管数据类型的大小，注意用Marshal.SizeOf得到的是非托管大小</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FromArray(arr As Array, TargetSize As Integer) As Pointer(Of T)
        Return New Pointer(Of T)(UnsafeAddrOfPinnedArrayElement(arr, 0), TargetSize)
    End Function
    ''' <summary>
    ''' 从值类型新建指针
    ''' </summary>
    ''' <param name="v">某值</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FromValueType(v As ValueType) As Pointer(Of T)
        Dim GC = GCHandle.Alloc(v, GCHandleType.Pinned)
        Dim res = New Pointer(Of T)(GC.AddrOfPinnedObject, SizeOf(v))
        GC.Free()
        Return res
    End Function
    ''' <summary>
    ''' 从地址新建指针
    ''' </summary>
    ''' <param name="p">地址</param>
    ''' <param name="size">指针对应的大小</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FromIntPtr(p As IntPtr, size As Integer) As Pointer(Of T)
        Return New Pointer(Of T)(p, size)
    End Function
    ''' <summary>
    ''' 转换指针类型
    ''' </summary>
    ''' <typeparam name="NewPtr">新的指针类型</typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SwitchType(Of NewPtr As Structure)() As Pointer(Of NewPtr)
        Return New Pointer(Of NewPtr)(CType(Address, IntPtr), SizeOf(GetType(NewPtr)))
    End Function
End Class