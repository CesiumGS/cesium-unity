using System;
using System.Collections.Generic;

internal class Comparers
{
    private class DoubleComparer : IEqualityComparer<double>
    {
        private readonly double _absoluteEpsilon;
        private readonly double _relativeEpsilon;

        public DoubleComparer(double absoluteEpsilon)
        {
            this._absoluteEpsilon = absoluteEpsilon;
            this._relativeEpsilon = 0.0;
        }

        public DoubleComparer(double absoluteEpsilon, double relativeEpsilon) : this(absoluteEpsilon)
        {
            this._relativeEpsilon = relativeEpsilon;
        }

        public bool Equals(double x, double y)
        {
            double diff = Math.Abs(x - y);
            return diff <= this._absoluteEpsilon ||
                diff <= RelativeToAbsoluteEpsilon(x, y, this._relativeEpsilon);
        }

        public int GetHashCode(double obj)
        {
            throw new NotImplementedException();
        }

        private double RelativeToAbsoluteEpsilon(double left, double right, double relativeEpsilon)
        {
            return relativeEpsilon * Math.Max(Math.Abs(left), Math.Abs(right));
        }
    }

    public static IEqualityComparer<double> Double(double absoluteEpsilon)
    {
        return new DoubleComparer(absoluteEpsilon);
    }

    public static IEqualityComparer<double> Double(double absoluteEpsilon, double relativeEpsilon)
    {
        return new DoubleComparer(absoluteEpsilon, relativeEpsilon);
    }
}
