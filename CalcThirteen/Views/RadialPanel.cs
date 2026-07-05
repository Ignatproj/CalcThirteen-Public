using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace CalcThirteen.Views;

public class RadialPanel : Panel
{
    public RadialPanel()
    {
        Background = Brushes.Transparent;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = new Size(
            double.IsInfinity(availableSize.Width) ? 500 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 500 : availableSize.Height);

        foreach (var child in Children)
            child.Measure(size);

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is RadialSectorButton button)
                button.ZIndex = i;

            Children[i].Arrange(rect);
        }

        return finalSize;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Handled)
            return;

        var point = e.GetPosition(this);
        var button = FindButtonAt(point);

        if (button is null)
            return;

        button.RaiseClickEvent();
        e.Handled = true;
    }

    private RadialSectorButton? FindButtonAt(Point point)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is not RadialSectorButton button || !button.IsEnabled)
                continue;

            if (button.IsPointInsideSector(point))
                return button;
        }

        return null;
    }
}
