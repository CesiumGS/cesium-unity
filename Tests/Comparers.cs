using System;
using System.Collections.Generic;

internal class Comparers
{
    private class DoubleComparer : IEqualityComparer<double>
    {
        private readonly double _epsilon;

        public DoubleComparer(double epsilon)
        {
            this._epsilon = epsilon;
        }

        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) <= this._epsilon;
        }

        public int GetHashCode(double obj)
        {
            throw new NotImplementedException();
        }
    }

    public static IEqualityComparer<double> Double(double absoluteEpsilon)
    {
        return new DoubleComparer(absoluteEpsilon);
    }
}
