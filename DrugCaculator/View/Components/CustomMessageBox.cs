using DrugCalculator.Utilities.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Brushes = System.Windows.Media.Brushes;

namespace DrugCalculator.View.Components;

public enum MsgBoxButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum MsgBoxIcon
{
    None,
    Information,
    Warning,
    Error,
    Success
}

public class CustomMessageBox : CustomDialog
{
    #region 依赖属性的定义与封装

    // 依赖属性定义
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message),
        typeof(string), typeof(CustomMessageBox), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OkButtonTextProperty = DependencyProperty.Register(nameof(OkButtonText),
        typeof(string), typeof(CustomMessageBox), new PropertyMetadata("确定"));

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(CustomMessageBox),
            new PropertyMetadata("取消"));

    public static readonly DependencyProperty NoButtonTextProperty = DependencyProperty.Register(nameof(NoButtonText),
        typeof(string), typeof(CustomMessageBox), new PropertyMetadata("否"));

    public static readonly DependencyProperty IconFontProperty = DependencyProperty.Register(nameof(IconFont),
        typeof(string), typeof(CustomMessageBox), new PropertyMetadata(null));

    // 封装属性
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string OkButtonText
    {
        get => (string)GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public string NoButtonText
    {
        get => (string)GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    public string IconFont
    {
        get => (string)GetValue(IconFontProperty);
        set => SetValue(IconFontProperty, value);
    }
    #endregion

    private CustomMessageBox()
    {
        DialogWidth = 300;
        DialogHeight = 180;
        InitializeTemplate();
    }

    // 初始化内容模板
    private void InitializeTemplate()
    {
        var contentStack = new StackPanel { Margin = new Thickness(15, 20, 15, 10) };

        // 创建图标和消息文本的横向排列
        var iconAndMessagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        // 图标文本块
        var iconText = new TextBlock
        {
            Text = IconFont,
            FontSize = 40,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0), // 右边距使文本与图标分开
            Style = (Style)Application.Current.FindResource("Icon")
        };

        iconText.SetBinding(TextBlock.TextProperty, new Binding(nameof(IconFont)) { Source = this });
        iconText.SetBinding(VisibilityProperty, new Binding(nameof(IconFont))
        {
            Source = this,
            Converter = StringIsNullToVisibilityConverter.Instance
        });
        iconText.SetBinding(ForegroundProperty, new Binding(nameof(IconFont))
        {
            Source = this,
            Converter = new MsgIconToBrushConverter()
        });

        // 消息文本框（不可编辑，允许选择）
        var messageTextBox = new TextBox
        {
            Text = Message,
            MaxWidth = 220,
            MaxHeight = 50,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent // 背景透明，使其看起来类似于文本
        };
        messageTextBox.SetBinding(TextBox.TextProperty, new Binding(nameof(Message)) { Source = this });


        // 添加图标和消息文本框到横向排列的面板中
        iconAndMessagePanel.Children.Add(iconText);
        iconAndMessagePanel.Children.Add(messageTextBox);
        contentStack.Children.Add(iconAndMessagePanel);

        // 按钮区域
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ClipToBounds = false,
            Margin = new Thickness(0, 0, 0, 20)
        };

        // OK按钮
        var okButton = new Button
        {
            Content = OkButtonText,
            Width = 60,
            Height = 26,
            Margin = new Thickness(10, 10, 10, 30),
            Padding = new Thickness(5),
            Style = (Style)Application.Current.FindResource("DarkButton")
        };

        okButton.Click += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
        okButton.SetBinding(ContentProperty, new Binding(nameof(OkButtonText)) { Source = this });
        buttonPanel.Children.Add(okButton);

        // No按钮
        if (!string.IsNullOrEmpty(NoButtonText))
        {
            var noButton = new Button
            {
                Content = NoButtonText,
                Width = 60,
                Height = 26,
                Margin = new Thickness(10, 10, 10, 30),
                Padding = new Thickness(5)
            };
            noButton.Click += (_, _) =>
            {
                DialogResult = false;
                Close();
            };
            noButton.SetBinding(ContentProperty, new Binding(nameof(NoButtonText)) { Source = this });
            noButton.Style = (Style)Application.Current.FindResource("LightButton");
            noButton.SetBinding(VisibilityProperty, new Binding(nameof(NoButtonText))
            {
                Source = this,
                Converter = StringIsNullToVisibilityConverter.Instance
            });
            buttonPanel.Children.Add(noButton);
        }

        // Cancel按钮
        if (!string.IsNullOrEmpty(CancelButtonText))
        {
            var cancelButton = new Button
            {
                Content = CancelButtonText,
                Width = 60,
                Height = 26,
                Margin = new Thickness(10, 10, 10, 30),
                Padding = new Thickness(5)

            };
            cancelButton.Click += (_, _) =>
            {
                DialogResult = null;
                Close();
            };
            cancelButton.SetBinding(ContentProperty, new Binding(nameof(CancelButtonText)) { Source = this });
            cancelButton.Style = (Style)Application.Current.FindResource("LightButton");
            cancelButton.SetBinding(VisibilityProperty, new Binding(nameof(CancelButtonText))
            {
                Source = this,
                Converter = StringIsNullToVisibilityConverter.Instance
            });
            buttonPanel.Children.Add(cancelButton);
        }

        contentStack.Children.Add(buttonPanel);
        Content = contentStack;
    }


    // 显示对话框并返回结果
    private MessageBoxResult ShowDialogWithResult()
    {
        var result = ShowDialog();
        return result switch
        {
            true => MessageBoxResult.Yes,
            false => MessageBoxResult.No,
            _ => MessageBoxResult.Cancel
        };
    }

    #region 11 个 Show 方法重载

    // 1. Show(string message)
    public static MessageBoxResult Show(string message)
    {
        return Show(message, "提示");
    }

    // 2. Show(string message, string title)
    public static MessageBoxResult Show(string message, string title)
    {
        return Show(message, title, MsgBoxButtons.OkCancel);
    }

    // 3. Show(string message, string title, MsgBoxButtons buttons)
    public static MessageBoxResult Show(string message, string title, MsgBoxButtons buttons)
    {
        return Show(message, title, buttons, MsgBoxIcon.None);
    }

    // 4. Show(string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon)
    public static MessageBoxResult Show(string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon)
    {
        return Show(message, title, buttons, icon, MessageBoxResult.None);
    }

    // 5. Show(string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon, MessageBoxResult defaultResult)
    public static MessageBoxResult Show(string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon,
        MessageBoxResult defaultResult)
    {
        var messageBox = new CustomMessageBox
        {
            Message = message,
            Title = title,
            IconFont = GetIconFont(icon),
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        ConfigureButtons(messageBox, buttons);
        var result = messageBox.ShowDialogWithResult();
        return result == MessageBoxResult.None ? defaultResult : result;
    }

    // 6. Show(Window owner, string message)
    public static MessageBoxResult Show(Window owner, string message)
    {
        return Show(owner, message, "提示");
    }

    // 7. Show(Window owner, string message, string title)
    public static MessageBoxResult Show(Window owner, string message, string title)
    {
        return Show(owner, message, title, MsgBoxButtons.OkCancel);
    }

    // 8. Show(Window owner, string message, string title, MsgBoxButtons buttons)
    public static MessageBoxResult Show(Window owner, string message, string title, MsgBoxButtons buttons)
    {
        return Show(owner, message, title, buttons, MsgBoxIcon.None);
    }

    // 9. Show(Window owner, string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon)
    public static MessageBoxResult Show(Window owner, string message, string title, MsgBoxButtons buttons,
        MsgBoxIcon icon)
    {
        return Show(owner, message, title, buttons, icon, MessageBoxResult.None);
    }

    // 10. Show(Window owner, string message, string title, MsgBoxButtons buttons, MsgBoxIcon icon, MessageBoxResult defaultResult)
    public static MessageBoxResult Show(Window owner, string message, string title, MsgBoxButtons buttons,
        MsgBoxIcon icon, MessageBoxResult defaultResult)
    {
        var messageBox = new CustomMessageBox
        {
            Message = message,
            Title = title,
            IconFont = GetIconFont(icon),
            Owner = owner
        };
        ConfigureButtons(messageBox, buttons);
        var result = messageBox.ShowDialogWithResult();
        return result == MessageBoxResult.None ? defaultResult : result;
    }

    // 11. Show(string message, string title, string okButtonText, string cancelButtonText)
    public static MessageBoxResult Show(string message, string title, string okButtonText, string cancelButtonText)
    {
        var messageBox = new CustomMessageBox
        {
            Message = message,
            Title = title,
            OkButtonText = okButtonText,
            CancelButtonText = cancelButtonText
        };
        return messageBox.ShowDialogWithResult();
    }

    #endregion

    // 配置按钮文本和功能
    private static void ConfigureButtons(CustomMessageBox messageBox, MsgBoxButtons buttons)
    {
        switch (buttons)
        {
            case MsgBoxButtons.Ok:
                messageBox.OkButtonText = "确定";
                messageBox.CancelButtonText = null;
                messageBox.NoButtonText = null;
                break;

            case MsgBoxButtons.OkCancel:
                messageBox.OkButtonText = "确定";
                messageBox.CancelButtonText = "取消";
                messageBox.NoButtonText = null;
                break;

            case MsgBoxButtons.YesNo:
                messageBox.OkButtonText = "是";
                messageBox.NoButtonText = "否";
                messageBox.CancelButtonText = null;
                break;

            case MsgBoxButtons.YesNoCancel:
                messageBox.OkButtonText = "是";
                messageBox.NoButtonText = "否";
                messageBox.CancelButtonText = "取消";
                break;
            default:
                messageBox.OkButtonText = null;
                messageBox.NoButtonText = null;
                messageBox.CancelButtonText = null;
                break;
        }
    }

    // 根据 MsgBoxIcon 返回对应的图标字符
    private static string GetIconFont(MsgBoxIcon icon)
    {
        return icon switch
        {
            MsgBoxIcon.Information => "\ue615", // 提示
            MsgBoxIcon.Warning => "\ue629", // 警告
            MsgBoxIcon.Error => "\ue60b", // 错误
            MsgBoxIcon.Success => "\ue665", // 正确
            _ => null // 无图标
        };
    }
}