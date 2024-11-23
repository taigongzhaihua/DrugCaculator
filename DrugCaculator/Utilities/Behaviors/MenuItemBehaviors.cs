using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCalculator.Utilities.Behaviors;

/// <summary>
/// 提供 `MenuItem` 控件的附加行为，支持鼠标事件绑定并触发 VisualState 切换。
/// </summary>
public static class MenuItemBehaviors
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 附加属性，用于控制是否启用鼠标事件绑定。
    /// </summary>
    public static readonly DependencyProperty AttachMouseEventsProperty =
        DependencyProperty.RegisterAttached(
            "AttachMouseEvents",
            typeof(bool),
            typeof(MenuItemBehaviors),
            new PropertyMetadata(false, OnAttachMouseEventsChanged));

    /// <summary>
    /// 获取是否附加鼠标事件。
    /// </summary>
    /// <param name="obj">目标依赖对象。</param>
    /// <returns>是否附加鼠标事件的布尔值。</returns>
    public static bool GetAttachMouseEvents(DependencyObject obj)
    {
        return (bool)obj.GetValue(AttachMouseEventsProperty);
    }

    /// <summary>
    /// 设置是否附加鼠标事件。
    /// </summary>
    /// <param name="obj">目标依赖对象。</param>
    /// <param name="value">是否附加鼠标事件的布尔值。</param>
    public static void SetAttachMouseEvents(DependencyObject obj, bool value)
    {
        obj.SetValue(AttachMouseEventsProperty, value);
    }

    /// <summary>
    /// 当附加属性的值发生更改时调用，负责添加或移除事件。
    /// </summary>
    /// <param name="d">附加属性的目标对象。</param>
    /// <param name="e">属性更改事件参数。</param>
    private static void OnAttachMouseEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuItem menuItem) return;

        // 根据新值决定是添加还是移除事件处理程序
        if ((bool)e.NewValue)
        {
            Logger.Info("为 MenuItem 附加鼠标事件处理程序。");
            AttachMouseEvents(menuItem);
        }
        else
        {
            Logger.Info("为 MenuItem 移除鼠标事件处理程序。");
            DetachMouseEvents(menuItem);
        }
    }

    /// <summary>
    /// 为指定的 `MenuItem` 附加鼠标事件处理程序。
    /// </summary>
    /// <param name="menuItem">目标 `MenuItem` 对象。</param>
    private static void AttachMouseEvents(MenuItem menuItem)
    {
        menuItem.MouseEnter += MenuItem_MouseEnter;
        menuItem.MouseLeave += MenuItem_MouseLeave;
        menuItem.PreviewMouseDown += MenuItem_PreviewMouseDown;
    }

    /// <summary>
    /// 为指定的 `MenuItem` 移除鼠标事件处理程序。
    /// </summary>
    /// <param name="menuItem">目标 `MenuItem` 对象。</param>
    private static void DetachMouseEvents(MenuItem menuItem)
    {
        menuItem.MouseEnter -= MenuItem_MouseEnter;
        menuItem.MouseLeave -= MenuItem_MouseLeave;
        menuItem.PreviewMouseDown -= MenuItem_PreviewMouseDown;
    }

    /// <summary>
    /// 当鼠标进入 `MenuItem` 时触发，将 VisualState 切换到 "MouseOver"。
    /// </summary>
    /// <param name="sender">事件发送者。</param>
    /// <param name="e">鼠标事件参数。</param>
    private static void MenuItem_MouseEnter(object sender, MouseEventArgs e)
    {
        Logger.Info("鼠标进入 MenuItem，尝试切换到 'MouseOver' 状态。");
        ChangeVisualState(sender, "MouseOver");
    }

    /// <summary>
    /// 当鼠标离开 `MenuItem` 时触发，将 VisualState 切换到 "Normal"。
    /// </summary>
    /// <param name="sender">事件发送者。</param>
    /// <param name="e">鼠标事件参数。</param>
    private static void MenuItem_MouseLeave(object sender, MouseEventArgs e)
    {
        Logger.Info("鼠标离开 MenuItem，尝试切换到 'Normal' 状态。");
        ChangeVisualState(sender, "Normal");
    }

    /// <summary>
    /// 当鼠标按下 `MenuItem` 时触发，将 VisualState 切换到 "Pressed"。
    /// </summary>
    /// <param name="sender">事件发送者。</param>
    /// <param name="e">鼠标按下事件参数。</param>
    private static void MenuItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Info("鼠标按下 MenuItem，尝试切换到 'Pressed' 状态。");
        ChangeVisualState(sender, "Pressed");
    }

    /// <summary>
    /// 切换指定对象的 VisualState。
    /// </summary>
    /// <param name="sender">事件发送者，通常为 `MenuItem`。</param>
    /// <param name="stateName">目标 VisualState 的名称。</param>
    private static void ChangeVisualState(object sender, string stateName)
    {
        if (sender is not MenuItem menuItem) return;

        try
        {
            // 切换到指定的 VisualState
            if (!VisualStateManager.GoToState(menuItem, stateName, true))
            {
                Logger.Warn($"未定义 VisualState: {stateName}");
            }
            else
            {
                Logger.Info($"成功切换到 VisualState: {stateName}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"切换 VisualState '{stateName}' 时发生异常。");
        }
    }
}