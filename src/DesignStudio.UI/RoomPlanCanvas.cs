using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesignStudio.Domain.Geometry;
using DesignStudio.Domain.Identity;
using DesignStudio.Geometry.Derived;

namespace DesignStudio.UI;

public sealed class RoomPlanCanvas : FrameworkElement
{
    public event EventHandler<int>? WallSelected;
    public event EventHandler<EndpointDragEventArgs>? EndpointDragged;
    public event EventHandler<EntityId>? OpeningSelected;
    public event EventHandler<OpeningDragEventArgs>? OpeningDragged;
    public event EventHandler<EntityId>? CabinetSelected;
    public event EventHandler<CabinetDragEventArgs>? CabinetDragged;

    private readonly Dictionary<int, (Point Start, Point End)> _screenWalls = new();
    private readonly Dictionary<EntityId, (Point Start, Point End)> _screenOpenings = new();
    private int _dragWallIndex = -1;
    private bool _dragStart;
    private EntityId? _dragOpeningId;
    private bool _draggingWallEndpoint;
    private bool _draggingOpening;
    private EntityId? _dragCabinetId;
    private bool _draggingCabinet;
    private readonly Dictionary<EntityId, (Point[] Corners, Point Center)> _screenCabinets = new();

    public static readonly DependencyProperty PlanProperty = DependencyProperty.Register(
        nameof(Plan), typeof(RoomPlan2D), typeof(RoomPlanCanvas),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectedWallIndexProperty = DependencyProperty.Register(
        nameof(SelectedWallIndex), typeof(int), typeof(RoomPlanCanvas),
        new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectedOpeningIdProperty = DependencyProperty.Register(
        nameof(SelectedOpeningId), typeof(EntityId?), typeof(RoomPlanCanvas),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public RoomPlan2D? Plan { get => (RoomPlan2D?)GetValue(PlanProperty); set => SetValue(PlanProperty, value); }
    public int SelectedWallIndex { get => (int)GetValue(SelectedWallIndexProperty); set => SetValue(SelectedWallIndexProperty, value); }
    public EntityId? SelectedOpeningId { get => (EntityId?)GetValue(SelectedOpeningIdProperty); set => SetValue(SelectedOpeningIdProperty, value); }

    public static readonly DependencyProperty SelectedCabinetIdProperty = DependencyProperty.Register(
        nameof(SelectedCabinetId), typeof(EntityId?), typeof(RoomPlanCanvas),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public EntityId? SelectedCabinetId { get => (EntityId?)GetValue(SelectedCabinetIdProperty); set => SetValue(SelectedCabinetIdProperty, value); }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        _screenWalls.Clear();
        _screenOpenings.Clear();
        _screenCabinets.Clear();

        if (Plan is null || Plan.Walls.Count == 0) return;

        var points = Plan.Walls.SelectMany(w => new[] { w.Start, w.End }).ToList();
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        var width = Math.Max(1, maxX - minX);
        var height = Math.Max(1, maxY - minY);
        var margin = 60.0;
        var scale = Math.Min(
            Math.Max(1, ActualWidth - margin * 2) / width,
            Math.Max(1, ActualHeight - margin * 2) / height);

        Point Map(Point2D p) =>
            new(margin + (p.X - minX) * scale, margin + (maxY - p.Y) * scale);

        for (var i = 0; i < Plan.Walls.Count; i++)
        {
            var wall = Plan.Walls[i];
            var a = Map(wall.Start);
            var b = Map(wall.End);
            _screenWalls[i] = (a, b);
            var selected = i == SelectedWallIndex;
            var pen = new Pen(selected ? Brushes.Black : Brushes.DimGray, selected ? 8 : 5);
            dc.DrawLine(pen, a, b);

            if (selected)
            {
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 2), a, 7, 7);
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 2), b, 7, 7);
            }

            DrawDimension(dc, a, b, wall.LengthMm);
        }

        foreach (var cabinet in Plan.Cabinets)
        {
            var rad = cabinet.RotationDegrees * Math.PI / 180.0;
            var ux = Math.Cos(rad);
            var uy = Math.Sin(rad);
            var vx = -uy;
            var vy = ux;
            var halfW = cabinet.WidthMm / 2.0;
            var halfD = cabinet.DepthMm / 2.0;
            Point2D ModelPoint(double a, double b) => new(
                cabinet.Center.X + a * ux + b * vx,
                cabinet.Center.Y + a * uy + b * vy);
            var modelCorners = new[] { ModelPoint(-halfW, -halfD), ModelPoint(halfW, -halfD), ModelPoint(halfW, halfD), ModelPoint(-halfW, halfD) };
            var corners = modelCorners.Select(p => Map(p)).ToArray();
            var center = Map(cabinet.Center);
            _screenCabinets[cabinet.Id] = (corners, center);
            var selected = SelectedCabinetId is EntityId selectedId && selectedId == cabinet.Id;
            var pen = new Pen(selected ? Brushes.Black : Brushes.DarkOliveGreen, selected ? 4 : 3);
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(corners[0], true, true);
                context.LineTo(corners[1], true, false);
                context.LineTo(corners[2], true, false);
                context.LineTo(corners[3], true, false);
            }
            dc.DrawGeometry(selected ? Brushes.WhiteSmoke : Brushes.Beige, pen, geometry);
            var text = new FormattedText(
                cabinet.Name, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 11, Brushes.Black, 1.0);
            dc.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
        }

        foreach (var opening in Plan.Openings)
        {
            var a = Map(opening.Start);
            var b = Map(opening.End);
            _screenOpenings[opening.Id] = (a, b);
            var selected = SelectedOpeningId is EntityId selectedId && selectedId == opening.Id;
            var baseBrush = opening.Kind == OpeningKind.Door ? Brushes.DarkSlateGray : Brushes.SteelBlue;
            var pen = new Pen(selected ? Brushes.Black : baseBrush, selected ? 10 : 7);
            dc.DrawLine(pen, a, b);
            if (selected)
            {
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 2), a, 5, 5);
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 2), b, 5, 5);
            }
        }
    }

    private static void DrawDimension(DrawingContext dc, Point a, Point b, double lengthMm)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var len = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
        var nx = -dy / len;
        var ny = dx / len;
        var offset = 20.0;
        var p = new Point((a.X + b.X) / 2 + nx * offset, (a.Y + b.Y) / 2 + ny * offset);
        var text = new FormattedText(
            $"{lengthMm:0} mm", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 12, Brushes.Black, 1.0);
        dc.DrawText(text, new Point(p.X - text.Width / 2, p.Y - text.Height / 2));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var point = e.GetPosition(this);
        if (Plan is null) return;

        var cabinetHit = _screenCabinets
            .Select(x => (Id: x.Key, Center: x.Value.Center, Distance: (x.Value.Center - point).Length, Corners: x.Value.Corners))
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (cabinetHit.Id != default && PointInPolygon(point, cabinetHit.Corners))
        {
            SelectedOpeningId = null;
            SelectedWallIndex = -1;
            SelectedCabinetId = cabinetHit.Id;
            _dragCabinetId = cabinetHit.Id;
            _draggingCabinet = true;
            CaptureMouse();
            CabinetSelected?.Invoke(this, cabinetHit.Id);
            InvalidateVisual();
            return;
        }

        var openingHit = _screenOpenings
            .Select(x => (Id: x.Key, x.Value.Start, x.Value.End, Distance: DistanceToSegment(point, x.Value.Start, x.Value.End)))
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (openingHit.Distance <= 14)
        {
            SelectedOpeningId = openingHit.Id;
            _dragOpeningId = openingHit.Id;
            _draggingOpening = true;
            CaptureMouse();
            OpeningSelected?.Invoke(this, openingHit.Id);
            InvalidateVisual();
            return;
        }

        if (_screenWalls.Count == 0) return;

        var endpointHit = _screenWalls
            .SelectMany(x => new[]
            {
                (Index: x.Key, IsStart: true, Point: x.Value.Start),
                (Index: x.Key, IsStart: false, Point: x.Value.End)
            })
            .Select(x => (x.Index, x.IsStart, x.Point, Distance: (x.Point - point).Length))
            .OrderBy(x => x.Distance)
            .First();

        if (endpointHit.Distance <= 14)
        {
            SelectedOpeningId = null;
            SelectedCabinetId = null;
            SelectedWallIndex = endpointHit.Index;
            _dragWallIndex = endpointHit.Index;
            _dragStart = endpointHit.IsStart;
            _draggingWallEndpoint = true;
            CaptureMouse();
            WallSelected?.Invoke(this, _dragWallIndex);
            InvalidateVisual();
            return;
        }

        var hit = _screenWalls
            .OrderBy(x => DistanceToSegment(point, x.Value.Start, x.Value.End))
            .First();

        if (DistanceToSegment(point, hit.Value.Start, hit.Value.End) <= 14)
        {
            SelectedOpeningId = null;
            SelectedCabinetId = null;
            SelectedWallIndex = hit.Key;
            WallSelected?.Invoke(this, hit.Key);
            InvalidateVisual();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_draggingWallEndpoint || _draggingOpening || _draggingCabinet) InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!(_draggingWallEndpoint || _draggingOpening || _draggingCabinet))
        {
            base.OnMouseLeftButtonUp(e);
            return;
        }

        var modelPoint = Snap(ScreenToModel(e.GetPosition(this)), _draggingOpening ? 50 : 100);
        var draggingOpening = _draggingOpening;
        var openingId = _dragOpeningId;
        var draggingCabinet = _draggingCabinet;
        var cabinetId = _dragCabinetId;
        var wallIndex = _dragWallIndex;
        var moveStart = _dragStart;

        _draggingOpening = false;
        _draggingWallEndpoint = false;
        _draggingCabinet = false;
        _dragOpeningId = null;
        _dragCabinetId = null;
        _dragWallIndex = -1;
        ReleaseMouseCapture();

        if (draggingCabinet && cabinetId is EntityId cabinetIdValue)
            CabinetDragged?.Invoke(this, new CabinetDragEventArgs(cabinetIdValue, modelPoint));
        else if (draggingOpening && openingId is EntityId id)
            OpeningDragged?.Invoke(this, new OpeningDragEventArgs(id, modelPoint));
        else
            EndpointDragged?.Invoke(this, new EndpointDragEventArgs(wallIndex, moveStart, modelPoint));

        base.OnMouseLeftButtonUp(e);
    }

    private Point2D ScreenToModel(Point screen)
    {
        if (Plan is null || Plan.Walls.Count == 0) return new Point2D(0, 0);
        var points = Plan.Walls.SelectMany(x => new[] { x.Start, x.End }).ToList();
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        var margin = 60.0;
        var scale = Math.Min(
            Math.Max(1, ActualWidth - margin * 2) / Math.Max(1, maxX - minX),
            Math.Max(1, ActualHeight - margin * 2) / Math.Max(1, maxY - minY));
        return new Point2D(
            minX + (screen.X - margin) / scale,
            maxY - (screen.Y - margin) / scale);
    }

    private static Point2D Snap(Point2D point, double grid)
        => new(Math.Round(point.X / grid) * grid, Math.Round(point.Y / grid) * grid);

    private static bool PointInPolygon(Point point, IReadOnlyList<Point> polygon)
    {
        var inside = false;
        for (var i = 0; i < polygon.Count; i++)
        {
            var j = (i + polygon.Count - 1) % polygon.Count;
            var pi = polygon[i];
            var pj = polygon[j];
            if (((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                point.X < (pj.X - pi.X) * (point.Y - pi.Y) / Math.Max(0.000001, pj.Y - pi.Y) + pi.X)
                inside = !inside;
        }
        return inside;
    }

    private static double DistanceToSegment(Point p, Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var len2 = dx * dx + dy * dy;
        if (len2 <= 0) return (p - a).Length;
        var t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / len2, 0, 1);
        var q = new Point(a.X + t * dx, a.Y + t * dy);
        return (p - q).Length;
    }
}

public sealed class EndpointDragEventArgs : EventArgs
{
    public EndpointDragEventArgs(int wallIndex, bool moveStart, Point2D modelPoint)
    {
        WallIndex = wallIndex;
        MoveStart = moveStart;
        ModelPoint = modelPoint;
    }

    public int WallIndex { get; }
    public bool MoveStart { get; }
    public Point2D ModelPoint { get; }
}

public sealed class OpeningDragEventArgs : EventArgs
{
    public OpeningDragEventArgs(EntityId openingId, Point2D modelPoint)
    {
        OpeningId = openingId;
        ModelPoint = modelPoint;
    }

    public EntityId OpeningId { get; }
    public Point2D ModelPoint { get; }
}

public sealed class CabinetDragEventArgs : EventArgs
{
    public CabinetDragEventArgs(EntityId cabinetId, DesignStudio.Domain.Geometry.Point2D modelPoint)
    {
        CabinetId = cabinetId;
        ModelPoint = modelPoint;
    }

    public EntityId CabinetId { get; }
    public DesignStudio.Domain.Geometry.Point2D ModelPoint { get; }
}
