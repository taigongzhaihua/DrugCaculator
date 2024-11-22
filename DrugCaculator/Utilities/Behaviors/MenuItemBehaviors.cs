using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCalculator.Utilities.Behaviors;
/// <summary>
/// 
/// </summary>
public static class MenuItemBehaviors
{
    public static bool GetAttachMouseEvents(DependencyObject obj)
    {
        return (bool)obj.GetValue(AttachMouseEventsProperty);
    }

    public static void SetAttachMouseEvents(DependencyObject obj, bool value)
    {
        obj.SetValue(AttachMouseEventsProperty, value);
    }

    // Using a DependencyProperty as the backing store for AttachMouseEvents. This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AttachMouseEventsProperty =
        DependencyProperty.RegisterAttached("AttachMouseEvents", typeof(bool), typeof(MenuItemBehaviors), new PropertyMetadata(false, OnAttachMouseEventsChanged));

    private static void OnAttachMouseEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuItem menuItem || e.NewValue is not true) return;
        menuItem.MouseEnter += MenuItem_MouseEnter;
        menuItem.MouseLeave += MenuItem_MouseLeave;
        menuItem.PreviewMouseDown += MenuItem_PreviewMouseDown;
    }

    private static void MenuItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            VisualStateManager.GoToState(menuItem, "MouseOver", true);
        }
    }

    private static void MenuItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            VisualStateManager.GoToState(menuItem, "Normal", true);
        }
    }

    private static void MenuItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            VisualStateManager.GoToState(menuItem, "Pressed", true);
        }
    }
}