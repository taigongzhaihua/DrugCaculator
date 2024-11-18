using DrugCalculator.Utilities.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DrugCalculator.View.Components;

public class CustomDialog : Window
{
    #region 定义和封装依赖属性
    // 使用一个通用方法来注册依赖属性，减少代码冗余
    private static DependencyProperty RegisterDependencyProperty<T>(string propertyName, T defaultValue)
    {
        return DependencyProperty.Register(propertyName, typeof(T), typeof(CustomDialog),
            new PropertyMetadata(defaultValue));
    }

    // 依赖属性定义
    public static readonly DependencyProperty DialogWidthProperty =
        RegisterDependencyProperty(nameof(DialogWidth), 200.00);

    public static readonly DependencyProperty DialogHeightProperty =
        RegisterDependencyProperty(nameof(DialogHeight), 100.00);

    public static readonly DependencyProperty DialogMinWidthProperty =
        RegisterDependencyProperty(nameof(DialogMinWidth), 60.0);

    public static readonly DependencyProperty DialogMinHeightProperty =
        RegisterDependencyProperty(nameof(DialogMinHeight), 60.0);

    public static readonly DependencyProperty DialogMaxWidthProperty =
        RegisterDependencyProperty(nameof(DialogMaxWidth), double.PositiveInfinity);

    public static readonly DependencyProperty DialogMaxHeightProperty =
        RegisterDependencyProperty(nameof(DialogMaxHeight), double.PositiveInfinity);

    public static readonly DependencyProperty DialogBackgroundColorProperty =
        RegisterDependencyProperty(nameof(DialogBackgroundColor), Brushes.White);

    public static readonly DependencyProperty TitleBarBackgroundProperty =
        RegisterDependencyProperty(nameof(TitleBarBackground), Brushes.GhostWhite);

    public static readonly DependencyProperty TitleColorProperty =
        RegisterDependencyProperty(nameof(TitleColor), Brushes.IndianRed);

    public static readonly DependencyProperty TitleAlignmentProperty =
        RegisterDependencyProperty(nameof(TitleAlignment), HorizontalAlignment.Center);

    public static readonly DependencyProperty TitleBarVisibilityProperty =
        RegisterDependencyProperty(nameof(TitleBarVisibility), Visibility.Visible);

    // 使用表达式属性简化包装器
    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    public double DialogHeight
    {
        get => (double)GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    public double DialogMinWidth
    {
        get => (double)GetValue(DialogMinWidthProperty);
        set => SetValue(DialogMinWidthProperty, value);
    }

    public double DialogMinHeight
    {
        get => (double)GetValue(DialogMinHeightProperty);
        set => SetValue(DialogMinHeightProperty, value);
    }

    public double DialogMaxWidth
    {
        get => (double)GetValue(DialogMaxWidthProperty);
        set => SetValue(DialogMaxWidthProperty, value);
    }

    public double DialogMaxHeight
    {
        get => (double)GetValue(DialogMaxHeightProperty);
        set => SetValue(DialogMaxHeightProperty, value);
    }

    public Brush DialogBackgroundColor
    {
        get => (Brush)GetValue(DialogBackgroundColorProperty);
        set => SetValue(DialogBackgroundColorProperty, value);
    }

    public Brush TitleBarBackground
    {
        get => (Brush)GetValue(TitleBarBackgroundProperty);
        set => SetValue(TitleBarBackgroundProperty, value);
    }

    public Brush TitleColor
    {
        get => (Brush)GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    public HorizontalAlignment TitleAlignment
    {
        get => (HorizontalAlignment)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public Visibility TitleBarVisibility
    {
        get => (Visibility)GetValue(TitleBarVisibilityProperty);
        set => SetValue(TitleBarVisibilityProperty, value);
    }
    #endregion

    // 构造函数，初始化窗口的样式和行为
    public CustomDialog()
    {
        DataContext = this;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        InitializeTemplate();
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed) TryDragMove();
        };
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
        InitializeBindings();
    }

    // 初始化绑定逻辑，将窗口的宽高等属性绑定到依赖属性
    private void InitializeBindings()
    {
        foreach (var (property, name) in new[]
                 {
                     (WidthProperty, nameof(DialogWidth)),
                     (HeightProperty, nameof(DialogHeight)),
                     (MinWidthProperty, nameof(DialogMinWidth)),
                     (MinHeightProperty, nameof(DialogMinHeight)),
                     (MaxWidthProperty, nameof(DialogMaxWidth)),
                     (MaxHeightProperty, nameof(DialogMaxHeight))
                 })
            SetBinding(property, new Binding(name) { Source = this, Converter = Add40Converter.Instance });
    }

    // 初始化控件模板，定义窗口的可视外观
    private void InitializeTemplate()
    {
        var template = new ControlTemplate(typeof(Window));

        // 主 Grid，用于容纳所有子元素，添加阴影效果
        var mainGridFactory = CreateMainGridFactory();

        // 边框，用于包裹窗口内容，设置圆角和边距
        var borderFactory = CreateBorderFactory();
        mainGridFactory.AppendChild(borderFactory);

        // 内部 Grid，用于放置标题栏和内容区域
        var innerGridFactory = CreateInnerGridFactory();

        // 创建标题栏
        var titleGridFactory = CreateTitleBar();
        titleGridFactory.SetBinding(VisibilityProperty, new Binding(nameof(TitleBarVisibility)) { Source = this });
        innerGridFactory.AppendChild(titleGridFactory);

        // 内容呈现器，用于显示用户定义的内容
        var contentPresenterFactory = CreateFrameworkElementFactory<ContentPresenter>((Grid.RowProperty, 1));
        innerGridFactory.AppendChild(contentPresenterFactory);

        // 将内部 Grid 添加到边框中
        borderFactory.AppendChild(innerGridFactory);

        // 设置模板的可视树
        template.VisualTree = mainGridFactory;
        Template = template;

        // 应用模板
        ApplyTemplate();
    }

    // 创建主 Grid，添加阴影效果
    private static FrameworkElementFactory CreateMainGridFactory()
    {
        return CreateFrameworkElementFactory<Grid>((EffectProperty, new DropShadowEffect
        {
            Color = Colors.SteelBlue,
            BlurRadius = 15,
            ShadowDepth = 5,
            Opacity = 0.5
        }));
    }

    // 创建边框，设置圆角和边距，并绑定大小属性
    private FrameworkElementFactory CreateBorderFactory()
    {
        var borderFactory = CreateFrameworkElementFactory<Border>(
            (Border.BackgroundProperty, new TemplateBindingExtension(DialogBackgroundColorProperty)),
            (Border.CornerRadiusProperty, new CornerRadius(15)),
            (Border.PaddingProperty, new Thickness(0, 0, 0, 5)),
            (ClipToBoundsProperty, true));
        borderFactory.AddHandler(SizeChangedEvent,
            new SizeChangedEventHandler((sender, _) => UpdateClipRegion((Border)sender)));

        foreach (var (property, name) in new[]
                 {
                     (WidthProperty, nameof(DialogWidth)),
                     (HeightProperty, nameof(DialogHeight)),
                     (MinWidthProperty, nameof(DialogMinWidth)),
                     (MinHeightProperty, nameof(DialogMinHeight)),
                     (MaxWidthProperty, nameof(DialogMaxWidth)),
                     (MaxHeightProperty, nameof(DialogMaxHeight))
                 })
            borderFactory.SetBinding(property, new Binding(name) { Source = this });

        return borderFactory;
    }

    // 创建内部 Grid 用于布局标题栏和内容区域
    private static FrameworkElementFactory CreateInnerGridFactory()
    {
        var innerGridFactory = new FrameworkElementFactory(typeof(Grid));
        innerGridFactory.AppendChild(CreateRowDefinition(GridLength.Auto)); // 标题栏高度
        innerGridFactory.AppendChild(CreateRowDefinition(new GridLength(1, GridUnitType.Star))); // 内容区域高度
        return innerGridFactory;
    }

    // 创建行定义，用于内部 Grid 的布局
    private static FrameworkElementFactory CreateRowDefinition(GridLength height)
    {
        return CreateFrameworkElementFactory<RowDefinition>((RowDefinition.HeightProperty, height)); // 设置行高
    }

    // 创建标题栏，包含标题文本和关闭按钮
    private FrameworkElementFactory CreateTitleBar()
    {
        var titleGridFactory = CreateFrameworkElementFactory<Grid>( // 创建 Grid 作为标题栏
            (Panel.BackgroundProperty, new TemplateBindingExtension(TitleBarBackgroundProperty)), // 绑定标题栏背景色属性
            (HeightProperty, 30.0), // 设置高度
            (VerticalAlignmentProperty, VerticalAlignment.Center)); // 垂直居中

        // 定义标题栏的两列
        titleGridFactory.AppendChild(CreateFrameworkElementFactory<ColumnDefinition>((ColumnDefinition.WidthProperty,
            new GridLength(1, GridUnitType.Star))));
        titleGridFactory.AppendChild(
            CreateFrameworkElementFactory<ColumnDefinition>((ColumnDefinition.WidthProperty, GridLength.Auto)));

        // 标题文本
        var titleTextFactory = CreateFrameworkElementFactory<TextBlock>(
            (TextBlock.TextProperty, new TemplateBindingExtension(TitleProperty)), // 绑定标题属性
            (MarginProperty, new Thickness(40, 0, 0, 0)), // 设置左边距
            (VerticalAlignmentProperty, VerticalAlignment.Center), // 垂直居中
            (HorizontalAlignmentProperty, new TemplateBindingExtension(TitleAlignmentProperty)), // 绑定标题对齐属性
            (TextBlock.FontSizeProperty, 14.0), // 设置字体大小
            (TextBlock.FontWeightProperty, FontWeights.Bold), // 设置字体粗细
            (TextBlock.ForegroundProperty, new TemplateBindingExtension(TitleColorProperty))); // 绑定标题颜色属性
        titleGridFactory.AppendChild(titleTextFactory); // 添加标题文本

        // 关闭按钮
        var closeButtonFactory = CreateFrameworkElementFactory<Button>(
            (ContentProperty, "\uE955"), // 使用 X 符号作为按钮内容
            (WidthProperty, 40.0), // 设置宽度
            (HorizontalAlignmentProperty, HorizontalAlignment.Right), // 右对齐
            (Grid.ColumnProperty, 1), // 放置在第二列
            (StyleProperty, new DynamicResourceExtension("CloseButton"))); // 使用样式 CloseButton
        closeButtonFactory.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((_, _) => Close())); // 点击时关闭窗口
        titleGridFactory.AppendChild(closeButtonFactory); // 添加关闭按钮

        return titleGridFactory; // 返回标题栏 Grid
    }

    // 试图拖动窗口
    private void TryDragMove()
    {
        try
        {
            if (WindowState is not WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }
            DragMove();
        }
        catch (InvalidOperationException)
        {
            /* 忽略异常，防止在某些情况下拖动失败导致的崩溃 */
        }
    }

    // 更新 Border 的裁剪区域，使其匹配当前大小和圆角
    private static void UpdateClipRegion(Border border)
    {
        border.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
            RadiusX = border.CornerRadius.TopLeft,
            RadiusY = border.CornerRadius.TopLeft
        };
    }

    // 通用 SetProperties 方法，用于简化属性设置
    private static void SetProperties(FrameworkElementFactory factory, params (DependencyProperty, object)[] properties)
    {
        foreach (var (property, value) in properties) factory.SetValue(property, value);
    }

    // 通用 CreateFrameworkElementFactory 方法，用于创建 FrameworkElementFactory 并设置属性
    private static FrameworkElementFactory CreateFrameworkElementFactory<T>(
        params (DependencyProperty, object)[] properties)
    {
        var factory = new FrameworkElementFactory(typeof(T));
        SetProperties(factory, properties);
        return factory;
    }
}