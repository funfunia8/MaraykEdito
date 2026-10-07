namespace DesignStudio.Domain.Geometry;

public enum SegmentRelation
{
    None,
    Touch,
    ProperIntersection,
    Overlap
}

public static class SegmentGeometry
{
    public static SegmentRelation Classify(
        Point2D firstStart,
        Point2D firstEnd,
        Point2D secondStart,
        Point2D secondEnd,
        double toleranceMm = 0.001)
    {
        if (!double.IsFinite(toleranceMm) ||
            toleranceMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toleranceMm));
        }

        var firstLength =
            firstStart.DistanceTo(firstEnd);

        var secondLength =
            secondStart.DistanceTo(secondEnd);

        if (firstLength <= toleranceMm ||
            secondLength <= toleranceMm)
        {
            if (firstLength <= toleranceMm &&
                secondLength <= toleranceMm)
            {
                return firstStart.DistanceTo(secondStart) <= toleranceMm
                    ? SegmentRelation.Touch
                    : SegmentRelation.None;
            }

            if (firstLength <= toleranceMm)
            {
                return IsPointOnSegment(
                    firstStart,
                    secondStart,
                    secondEnd,
                    toleranceMm)
                    ? SegmentRelation.Touch
                    : SegmentRelation.None;
            }

            return IsPointOnSegment(
                secondStart,
                firstStart,
                firstEnd,
                toleranceMm)
                ? SegmentRelation.Touch
                : SegmentRelation.None;
        }

        if (!BoundingBoxesOverlap(
                firstStart,
                firstEnd,
                secondStart,
                secondEnd,
                toleranceMm))
        {
            return SegmentRelation.None;
        }

        if (AreCollinear(
                firstStart,
                firstEnd,
                secondStart,
                secondEnd,
                toleranceMm))
        {
            return ClassifyCollinear(
                firstStart,
                firstEnd,
                secondStart,
                secondEnd,
                firstLength,
                toleranceMm);
        }

        var orientation1 =
            Orientation(
                firstStart,
                firstEnd,
                secondStart,
                toleranceMm);

        var orientation2 =
            Orientation(
                firstStart,
                firstEnd,
                secondEnd,
                toleranceMm);

        var orientation3 =
            Orientation(
                secondStart,
                secondEnd,
                firstStart,
                toleranceMm);

        var orientation4 =
            Orientation(
                secondStart,
                secondEnd,
                firstEnd,
                toleranceMm);

        if (orientation1 != 0 &&
            orientation2 != 0 &&
            orientation3 != 0 &&
            orientation4 != 0 &&
            orientation1 != orientation2 &&
            orientation3 != orientation4)
        {
            return SegmentRelation.ProperIntersection;
        }

        if (IsPointOnSegment(
                firstStart,
                secondStart,
                secondEnd,
                toleranceMm) ||
            IsPointOnSegment(
                firstEnd,
                secondStart,
                secondEnd,
                toleranceMm) ||
            IsPointOnSegment(
                secondStart,
                firstStart,
                firstEnd,
                toleranceMm) ||
            IsPointOnSegment(
                secondEnd,
                firstStart,
                firstEnd,
                toleranceMm))
        {
            return SegmentRelation.Touch;
        }

        return SegmentRelation.None;
    }

    private static SegmentRelation ClassifyCollinear(
        Point2D firstStart,
        Point2D firstEnd,
        Point2D secondStart,
        Point2D secondEnd,
        double firstLength,
        double toleranceMm)
    {
        var firstProjection =
            ProjectParameter(
                firstStart,
                firstEnd,
                secondStart);

        var secondProjection =
            ProjectParameter(
                firstStart,
                firstEnd,
                secondEnd);

        var overlapStart =
            Math.Max(
                0,
                Math.Min(
                    firstProjection,
                    secondProjection));

        var overlapEnd =
            Math.Min(
                1,
                Math.Max(
                    firstProjection,
                    secondProjection));

        var overlapLengthMm =
            (overlapEnd - overlapStart) *
            firstLength;

        if (overlapLengthMm > toleranceMm)
            return SegmentRelation.Overlap;

        if (Math.Abs(overlapLengthMm) <= toleranceMm)
            return SegmentRelation.Touch;

        return SegmentRelation.None;
    }

    private static bool AreCollinear(
        Point2D firstStart,
        Point2D firstEnd,
        Point2D secondStart,
        Point2D secondEnd,
        double toleranceMm)
    {
        var firstLength =
            firstStart.DistanceTo(firstEnd);

        if (firstLength <= toleranceMm)
        {
            return secondStart.DistanceTo(firstStart) <= toleranceMm &&
                   secondEnd.DistanceTo(firstStart) <= toleranceMm;
        }

        var secondStartDistance =
            Math.Abs(
                Cross(
                    firstStart,
                    firstEnd,
                    secondStart)) /
            firstLength;

        var secondEndDistance =
            Math.Abs(
                Cross(
                    firstStart,
                    firstEnd,
                    secondEnd)) /
            firstLength;

        return secondStartDistance <= toleranceMm &&
               secondEndDistance <= toleranceMm;
    }

    private static bool IsPointOnSegment(
        Point2D point,
        Point2D start,
        Point2D end,
        double toleranceMm)
    {
        var length =
            start.DistanceTo(end);

        if (length <= toleranceMm)
            return point.DistanceTo(start) <= toleranceMm;

        var distanceFromLine =
            Math.Abs(
                Cross(
                    start,
                    end,
                    point)) /
            length;

        if (distanceFromLine > toleranceMm)
            return false;

        var dx =
            end.X - start.X;

        var dy =
            end.Y - start.Y;

        var lengthSquared =
            dx * dx +
            dy * dy;

        var projection =
            ((point.X - start.X) * dx +
             (point.Y - start.Y) * dy) /
            lengthSquared;

        var parameterTolerance =
            toleranceMm / length;

        return projection >= -parameterTolerance &&
               projection <= 1 + parameterTolerance;
    }

    private static int Orientation(
        Point2D start,
        Point2D end,
        Point2D point,
        double toleranceMm)
    {
        var length =
            start.DistanceTo(end);

        if (length <= toleranceMm)
            return 0;

        var signedDistance =
            Cross(
                start,
                end,
                point) /
            length;

        if (Math.Abs(signedDistance) <= toleranceMm)
            return 0;

        return signedDistance > 0
            ? 1
            : -1;
    }

    private static double ProjectParameter(
        Point2D start,
        Point2D end,
        Point2D point)
    {
        var dx =
            end.X - start.X;

        var dy =
            end.Y - start.Y;

        var lengthSquared =
            dx * dx +
            dy * dy;

        if (lengthSquared <= 0)
            return 0;

        return
            ((point.X - start.X) * dx +
             (point.Y - start.Y) * dy) /
            lengthSquared;
    }

    private static bool BoundingBoxesOverlap(
        Point2D firstStart,
        Point2D firstEnd,
        Point2D secondStart,
        Point2D secondEnd,
        double toleranceMm)
    {
        var firstMinX =
            Math.Min(
                firstStart.X,
                firstEnd.X);

        var firstMaxX =
            Math.Max(
                firstStart.X,
                firstEnd.X);

        var firstMinY =
            Math.Min(
                firstStart.Y,
                firstEnd.Y);

        var firstMaxY =
            Math.Max(
                firstStart.Y,
                firstEnd.Y);

        var secondMinX =
            Math.Min(
                secondStart.X,
                secondEnd.X);

        var secondMaxX =
            Math.Max(
                secondStart.X,
                secondEnd.X);

        var secondMinY =
            Math.Min(
                secondStart.Y,
                secondEnd.Y);

        var secondMaxY =
            Math.Max(
                secondStart.Y,
                secondEnd.Y);

        return firstMinX <= secondMaxX + toleranceMm &&
               firstMaxX >= secondMinX - toleranceMm &&
               firstMinY <= secondMaxY + toleranceMm &&
               firstMaxY >= secondMinY - toleranceMm;
    }

    private static double Cross(
        Point2D firstStart,
        Point2D firstEnd,
        Point2D point)
    {
        return
            (firstEnd.X - firstStart.X) *
            (point.Y - firstStart.Y) -
            (firstEnd.Y - firstStart.Y) *
            (point.X - firstStart.X);
    }
}
