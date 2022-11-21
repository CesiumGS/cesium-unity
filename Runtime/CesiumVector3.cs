using System;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumVector3
    {
        public double x;
        public double y;
        public double z;

        public CesiumVector3() : this(0.0, 0.0, 0.0)
        {}

        public CesiumVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static CesiumVector3 Lerp(CesiumVector3 a, CesiumVector3 b, double t)
        {
            return new CesiumVector3()
            {
                x = a.x + (b.x - a.x) * t,
                y = a.y + (b.y - a.y) * t,
                z = a.z + (b.z - a.z) * t
            };
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static double Length(CesiumVector3 input)
        {
            return Math.Sqrt(
                input.x * input.x 
                + input.y * input.y 
                + input.z * input.z);
        }

        public static CesiumVector3 Normalize(CesiumVector3 input)
        {
            double length = CesiumVector3.Length(input);

            return new CesiumVector3()
            {
                x = input.x / length,
                y = input.y / length,
                z = input.z / length
            };
        }

        public static CesiumVector3 operator +(CesiumVector3 a, CesiumVector3 b)
        {
            return new CesiumVector3()
            {
                x = a.x + b.x,
                y = a.y + b.y,
                z = a.z + b.z
            };
        }

        public static CesiumVector3 operator -(CesiumVector3 a, CesiumVector3 b)
        {
            return new CesiumVector3()
            {
                x = a.x - b.x,
                y = a.y - b.y,
                z = a.z - b.z
            };
        }
    }
}
