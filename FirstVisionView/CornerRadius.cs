using System.Windows;

namespace VisionView
{
    public static class ButtonHelp
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ButtonHelp),
                new PropertyMetadata(new CornerRadius(8))
                );

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }
    }
    public static class ComboboxHelp
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ComboboxHelp),
                new PropertyMetadata(new CornerRadius(15))
                );

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }
    }
}
