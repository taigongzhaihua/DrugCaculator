using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DrugCalculator.View.Controls;

public class ShadowBorder : Border
{
    public ShadowBorder()
    {
        // 设置默认的阴影效果
        Effect = new DropShadowEffect
        {
            Color = Colors.DodgerBlue,
            BlurRadius = 15,
            ShadowDepth = 0,
            Opacity = 0.2
        };
        Background = new SolidColorBrush(Colors.White);
        BorderThickness = new Thickness(0);
        // BorderBrush = new SolidColorBrush(Colors.DeepSkyBlue);
    }

    // 可选：提供阴影相关的属性以便自定义
    public static readonly DependencyProperty ShadowColorProperty =
        DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(ShadowBorder),
            new PropertyMetadata(Colors.SteelBlue, OnShadowPropertyChanged));

    public static readonly DependencyProperty ShadowBlurRadiusProperty =
        DependencyProperty.Register(nameof(ShadowBlurRadius), typeof(double), typeof(ShadowBorder),
            new PropertyMetadata(15.0, OnShadowPropertyChanged));

    public static readonly DependencyProperty ShadowDepthProperty =
        DependencyProperty.Register(nameof(ShadowDepth), typeof(double), typeof(ShadowBorder),
            new PropertyMetadata(3.0, OnShadowPropertyChanged));

    public static readonly DependencyProperty ShadowOpacityProperty =
        DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(ShadowBorder),
            new PropertyMetadata(0.5, OnShadowPropertyChanged));

    public Color ShadowColor
    {
        get => (Color)GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    public double ShadowBlurRadius
    {
        get => (double)GetValue(ShadowBlurRadiusProperty);
        set => SetValue(ShadowBlurRadiusProperty, value);
    }

    public double ShadowDepth
    {
        get => (double)GetValue(ShadowDepthProperty);
        set => SetValue(ShadowDepthProperty, value);
    }

    public double ShadowOpacity
    {
        get => (double)GetValue(ShadowOpacityProperty);
        set => SetValue(ShadowOpacityProperty, value);
    }

    private static void OnShadowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var shadowBorder = (ShadowBorder)d;
        if (shadowBorder.Effect is DropShadowEffect dropShadowEffect)
        {
            dropShadowEffect.Color = shadowBorder.ShadowColor;
            dropShadowEffect.BlurRadius = shadowBorder.ShadowBlurRadius;
            dropShadowEffect.ShadowDepth = shadowBorder.ShadowDepth;
            dropShadowEffect.Opacity = shadowBorder.ShadowOpacity;
        }
    }
}