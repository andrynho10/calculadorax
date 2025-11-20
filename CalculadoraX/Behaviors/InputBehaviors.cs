using System.Linq;
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

    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(InputBehaviors),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    public static bool GetFocusOnVisible(DependencyObject obj) => (bool)obj.GetValue(FocusOnVisibleProperty);

    public static void SetFocusOnVisible(DependencyObject obj, bool value) => obj.SetValue(FocusOnVisibleProperty, value);

    public static bool GetSelectAllOnFocus(DependencyObject obj) => (bool)obj.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value) => obj.SetValue(SelectAllOnFocusProperty, value);

    public static bool GetNumericOnly(DependencyObject obj) => (bool)obj.GetValue(NumericOnlyProperty);

    public static void SetNumericOnly(DependencyObject obj, bool value) => obj.SetValue(NumericOnlyProperty, value);

    private static void OnFocusOnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement fe)
        {
            if ((bool)e.NewValue)
            {
                fe.Loaded += ElementOnLoaded;
                fe.IsVisibleChanged += ElementOnIsVisibleChanged;
            }
            else
            {
                fe.Loaded -= ElementOnLoaded;
                fe.IsVisibleChanged -= ElementOnIsVisibleChanged;
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

    private static void OnNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += TextBoxOnPreviewTextInput;
                DataObject.AddPastingHandler(textBox, OnPasteNumericOnly);
                textBox.PreviewKeyDown += TextBoxOnPreviewKeyDown;
            }
            else
            {
                textBox.PreviewTextInput -= TextBoxOnPreviewTextInput;
                DataObject.RemovePastingHandler(textBox, OnPasteNumericOnly);
                textBox.PreviewKeyDown -= TextBoxOnPreviewKeyDown;
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

    private static void TextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!IsTextNumeric(e.Text))
        {
            e.Handled = true;
        }
    }

    private static void OnPasteNumericOnly(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = e.DataObject.GetData(DataFormats.Text)?.ToString() ?? string.Empty;
            if (!IsTextNumeric(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private static void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    private static bool IsTextNumeric(string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        return text.All(ch => char.IsDigit(ch) || ch == '.' || ch == ',' );
    }
}
