using System;
using System.Collections.Generic;
using Unity.Mathematics;

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

    private class Double3Comparer : IEqualityComparer<double3>
    {
        private DoubleComparer _doubleComparer;

        public Double3Comparer(double absoluteEpsilon, double relativeEpsilon = 0.0)
        {
            this._doubleComparer = new DoubleComparer(absoluteEpsilon, relativeEpsilon);
        }

        public bool Equals(double3 a, double3 b)
        {
            return _doubleComparer.Equals(a.x, b.x) && _doubleComparer.Equals(a.y, b.y) && _doubleComparer.Equals(a.z, b.z);
        }

        public int GetHashCode(double3 obj)
        {
            throw new NotImplementedException();
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

    public static IEqualityComparer<double3> Double3(double absoluteEpsilon)
    {
        return new Double3Comparer(absoluteEpsilon);
    }

    public static IEqualityComparer<double3> Double3(double absoluteEpsilon, double relativeEpsilon)
    {
        return new Double3Comparer(absoluteEpsilon, relativeEpsilon);
    }
}
