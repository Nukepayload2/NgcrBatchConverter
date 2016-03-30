Imports System.Windows.Media.Effects
Public Class BinarizeFixedThresholdEffect
    Inherits ShaderEffect
    Public Shared ReadOnly InputProperty As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", GetType(BinarizeFixedThresholdEffect), 0)
    Public Shared ReadOnly ThreasholdProperty As DependencyProperty = DependencyProperty.Register("Threashold", GetType(Double), GetType(BinarizeFixedThresholdEffect), New UIPropertyMetadata(0.56, PixelShaderConstantCallback(0)))
    Public Sub New()
        MyBase.New()
        PixelShader = New PixelShader With {.UriSource = New Uri("/NCGR²é¿´Æ÷;component/BinarizeFixedThresholdEffect.ps", UriKind.Relative)}
        UpdateShaderValue(InputProperty)
        UpdateShaderValue(ThreasholdProperty)
    End Sub
    Public Property Input() As Brush
        Get
            Return CType(Me.GetValue(InputProperty), Brush)
        End Get
        Set(value As Brush)
            Me.SetValue(InputProperty, Value)
        End Set
    End Property
    Public Property Threashold() As Double
        Get
            Return CType(Me.GetValue(ThreasholdProperty), Double)
        End Get
        Set(value As Double)
            Me.SetValue(ThreasholdProperty, value)
        End Set
    End Property
End Class