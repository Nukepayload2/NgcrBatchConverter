
Imports System.Windows.Media.Effects

Public Class TransparencyColorEffect
    Inherits ShaderEffect
    Public Shared ReadOnly InputProperty As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", GetType(TransparencyColorEffect), 0)
    Public Shared ReadOnly TransparentKeyProperty As DependencyProperty = DependencyProperty.Register("TransparentKey", GetType(Color), GetType(TransparencyColorEffect), New UIPropertyMetadata(Colors.Green, PixelShaderConstantCallback(0)))
    Public Sub New()
        MyBase.New()
        Dim pixelShader As PixelShader = New PixelShader()
        pixelShader.UriSource = New Uri("/NCGR²é¿´Æ÷;component/TransparencyColorEffect.ps", UriKind.Relative)
        Me.PixelShader = pixelShader

        Me.UpdateShaderValue(InputProperty)
        Me.UpdateShaderValue(TransparentKeyProperty)
    End Sub
    Public Property Input() As Brush
        Get
            Return CType(Me.GetValue(InputProperty), Brush)
        End Get
        Set(value As Brush)
            Me.SetValue(InputProperty, Value)
        End Set
    End Property
    Public Property TransparentKey() As Color
        Get
            Return CType(Me.GetValue(TransparentKeyProperty), Color)
        End Get
        Set(value As Color)
            Me.SetValue(TransparentKeyProperty, value)
        End Set
    End Property
End Class