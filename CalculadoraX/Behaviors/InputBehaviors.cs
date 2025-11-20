using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CalculadoraX.Behaviors;

public static class InputBehaviors
{
    public static readonly DependencyProperty FocusOnVisibleProperty =
        DependencyProperty.RegisterAttached(
            "FocusOnVisible",
            typeof(bool),
            typeof(InputBehaviors),
            new PropertyMetadata(false, OnFocusOnVisibleChanged));

    public static readonly DependencyProperty SelectAllOnFocusProperty =
        DependencyProperty.RegisterAttached(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(InputBehaviors),
            new PropertyMetadata(false, OnSelectAllOnFocusChanged));

    public static bool GetFocusOnVisible(DependencyObject obj) => (bool)obj.GetValue(FocusOnVisibleProperty);

    public static void SetFocusOnVisible(DependencyObject obj, bool value) => obj.SetValue(FocusOnVisibleProperty, value);

    public static bool GetSelectAllOnFocus(DependencyObject obj) => (bool)obj.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value) => obj.SetValue(SelectAllOnFocusProperty, value);

    private static void OnFocusOnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if ((bool)e.NewValue)
            {
                element.Loaded += ElementOnLoaded;
                element.IsVisibleChanged += ElementOnIsVisibleChanged;
            }
            else
            {
                element.Loaded -= ElementOnLoaded;
                element.IsVisibleChanged -= ElementOnIsVisibleChanged;
            }
        }
    }

    private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.GotKeyboardFocus += TextBoxOnGotKeyboardFocus;
                textBox.PreviewMouseLeftButtonDown += TextBoxOnPreviewMouseLeftButtonDown;
            }
            else
            {
                textBox.GotKeyboardFocus -= TextBoxOnGotKeyboardFocus;
                textBox.PreviewMouseLeftButtonDown -= TextBoxOnPreviewMouseLeftButtonDown;
            }
        }
    }

    private static void ElementOnLoaded(object sender, RoutedEventArgs e)
    {
        FocusElement(sender as UIElement);
    }

    private static void ElementOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is UIElement element && element.IsVisible)
        {
            FocusElement(element);
        }
    }

    private static void FocusElement(UIElement? element)
    {
        if (element is null) return;
        element.Dispatcher.BeginInvoke(() =>
        {
            element.Focus();
        }, DispatcherPriority.ApplicationIdle);
    }

    private static void TextBoxOnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private static void TextBoxOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin)
        {
            e.Handled = true;
            textBox.Focus();
        }
    }
}
