using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace CalcThirteen.Views;

public class RadialSectorButton : Button
{
    private const double GapAngle = 1.5;

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<RadialSectorButton, double>(nameof(StartAngle));

    public static readonly StyledProperty<double> EndAngleProperty =
        AvaloniaProperty.Register<RadialSectorButton, double>(nameof(EndAngle));

    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<RadialSectorButton, double>(nameof(InnerRadius));

    public static readonly StyledProperty<double> OuterRadiusProperty =
        AvaloniaProperty.Register<RadialSectorButton, double>(nameof(OuterRadius));

    private StreamGeometry? _geometry;

    static RadialSectorButton()
    {
        BackgroundProperty.OverrideDefaultValue<RadialSectorButton>(new SolidColorBrush(Color.Parse("#2d2d30")));
        ForegroundProperty.OverrideDefaultValue<RadialSectorButton>(Brushes.White);
        FontSizeProperty.OverrideDefaultValue<RadialSectorButton>(20d);
        FocusableProperty.OverrideDefaultValue<RadialSectorButton>(false);
        IsHitTestVisibleProperty.OverrideDefaultValue<RadialSectorButton>(false);
    }

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double EndAngle
    {
        get => GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    public double OuterRadius
    {
        get => GetValue(OuterRadiusProperty);
        set => SetValue(OuterRadiusProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BackgroundProperty
            || change.Property == ForegroundProperty
            || change.Property == FontSizeProperty)
        {
            InvalidateVisual();
            return;
        }

        if (change.Property == BoundsProperty
            || change.Property == StartAngleProperty
            || change.Property == EndAngleProperty
            || change.Property == InnerRadiusProperty
            || change.Property == OuterRadiusProperty)
        {
            _geometry = null;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _geometry ??= CreateSectorGeometry(center, InnerRadius, OuterRadius, StartAngle, EndAngle);

        var fill = Background ?? Brushes.Gray;
        var border = new Pen(Brushes.Black, 1);
        context.DrawGeometry(fill, border, _geometry);

        var label = Content?.ToString();
        if (string.IsNullOrEmpty(label))
            return;

        Point textPosition;
        if (IsFullCircle())
            textPosition = center;
        else
        {
            var midAngle = (StartAngle + EndAngle) / 2.0 * Math.PI / 180.0;
            var midRadius = (InnerRadius + OuterRadius) / 2.0;
            textPosition = new Point(
                center.X + midRadius * Math.Cos(midAngle),
                center.Y + midRadius * Math.Sin(midAngle));
        }

        var formattedText = new FormattedText(
            label,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily ?? FontFamily.Default, FontStyle.Normal, FontWeight),
            FontSize,
            Foreground ?? Brushes.White);

        context.DrawText(
            formattedText,
            new Point(textPosition.X - formattedText.Width / 2, textPosition.Y - formattedText.Height / 2));
    }

    public bool IsPointInsideSector(Point localPoint)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return false;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _geometry ??= CreateSectorGeometry(center, InnerRadius, OuterRadius, StartAngle, EndAngle);
        return _geometry.FillContains(localPoint);
    }

    public void RaiseClickEvent()
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }

    private static StreamGeometry CreateSectorGeometry(
        Point center,
        double innerRadius,
        double outerRadius,
        double startAngle,
        double endAngle)
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();

        if (innerRadius < 1 && endAngle - startAngle >= 359.0)
        {
            context.BeginFigure(new Point(center.X + outerRadius, center.Y), true);
            context.ArcTo(
                new Point(center.X - outerRadius, center.Y),
                new Size(outerRadius, outerRadius),
                0,
                false,
                SweepDirection.Clockwise);
            context.ArcTo(
                new Point(center.X + outerRadius, center.Y),
                new Size(outerRadius, outerRadius),
                0,
                false,
                SweepDirection.Clockwise);
            return geometry;
        }

        var start = startAngle + GapAngle / 2.0;
        var end = endAngle - GapAngle / 2.0;

        if (end <= start)
            end = start + 0.01;

        var startOuter = Polar(center, outerRadius, start);
        var endOuter = Polar(center, outerRadius, end);
        var endInner = Polar(center, innerRadius, end);
        var startInner = Polar(center, innerRadius, start);
        var largeArc = end - start > 180;

        context.BeginFigure(startOuter, true);
        context.ArcTo(endOuter, new Size(outerRadius, outerRadius), 0, largeArc, SweepDirection.Clockwise);
        context.LineTo(endInner);
        context.ArcTo(startInner, new Size(innerRadius, innerRadius), 0, largeArc, SweepDirection.CounterClockwise);
        context.LineTo(startOuter);

        return geometry;
    }

    private bool IsFullCircle() => InnerRadius < 1 && EndAngle - StartAngle >= 359.0;

    private static Point Polar(Point center, double radius, double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }
}
