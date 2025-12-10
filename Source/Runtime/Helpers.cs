using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    internal class Helpers
    {
        public static string ToString<T>(T value)
        {
            return value.ToString();
        }

        public static Vector3 FromMathematics(double3 vector)
        {
            return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        }

        public static Vector4 FromMathematics(double4 vector)
        {
            return new Vector4((float)vector.x, (float)vector.y, (float)vector.z, (float)vector.w);
        }

        public static double4x4 ToMathematics(Matrix4x4 matrix)
        {
            return new double4x4(
                matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                matrix.m30, matrix.m31, matrix.m32, matrix.m33);
        }

        public static Matrix4x4 FromMathematics(double4x4 matrix)
        {
            return new Matrix4x4(FromMathematics(matrix.c0), FromMathematics(matrix.c1), FromMathematics(matrix.c2), FromMathematics(matrix.c3));
        }

        public static double3x3 ToMathematicsDouble3x3(Matrix4x4 matrix)
        {
            return new double3x3(
                matrix.m00, matrix.m01, matrix.m02,
                matrix.m10, matrix.m11, matrix.m12,
                matrix.m20, matrix.m21, matrix.m22);
        }

        public static float3x3 ToMathematicsFloat3x3(Matrix4x4 matrix)
        {
            return new float3x3(
                matrix.m00, matrix.m01, matrix.m02,
                matrix.m10, matrix.m11, matrix.m12,
                matrix.m20, matrix.m21, matrix.m22);
        }

        internal static void MatrixToRotationAndScale(double3x3 matrix, out quaternion rotation, out double3 scale)
        {
            double lengthColumn0 = math.length(matrix.c0);
            double lengthColumn1 = math.length(matrix.c1);
            double lengthColumn2 = math.length(matrix.c2);

            float3x3 rotationMatrix = new float3x3(
                (float3)(matrix.c0 / lengthColumn0),
                (float3)(matrix.c1 / lengthColumn1),
                (float3)(matrix.c2 / lengthColumn2));

            scale = new double3(lengthColumn0, lengthColumn1, lengthColumn2);

            double3 cross = math.cross(matrix.c0, matrix.c1);
            if (math.dot(cross, matrix.c2) < 0.0)
            {
                rotationMatrix *= -1.0f;
                scale *= -1.0f;
            }

            rotation = math.quaternion(rotationMatrix);
        }

        internal static void MatrixToTranslationRotationAndScale(double4x4 matrix, out double3 translation, out quaternion rotation, out double3 scale)
        {
            translation = matrix.c3.xyz;

            Helpers.MatrixToRotationAndScale(
                new double3x3(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz),
                out rotation,
                out scale);
        }

        internal static void MatrixToInaccurateRotationAndScale(double3x3 matrix, out Quaternion rotation, out Vector3 scale)
        {
            quaternion rotationTemp;
            double3 scaleTemp;
            MatrixToRotationAndScale(matrix, out rotationTemp, out scaleTemp);

            rotation = rotationTemp;
            scale = (float3)scaleTemp;
        }

        internal static void MatrixToInaccurateTranslationRotationAndScale(double4x4 matrix, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            translation = Helpers.FromMathematics(matrix.c3.xyz);

            Helpers.MatrixToInaccurateRotationAndScale(
                new double3x3(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz),
                out rotation,
                out scale);
        }

        internal static double4x4 TranslationRotationAndScaleToMatrix(double3 translation, quaternion rotation, double3 scale)
        {
            double3x3 scaleMatrix = new double3x3(
                new double3(scale.x, 0.0, 0.0),
                new double3(0.0, scale.y, 0.0),
                new double3(0.0, 0.0, scale.z));
            double3x3 rotationMatrix = new float3x3(rotation);
            double3x3 scaleAndRotate = math.mul(rotationMatrix, scaleMatrix);
            return new double4x4(
                new double4(scaleAndRotate.c0, 0.0),
                new double4(scaleAndRotate.c1, 0.0),
                new double4(scaleAndRotate.c2, 0.0),
                new double4(translation, 1.0));
        }

        /// <summary>
        /// Produces an angle in the range -Pi <= angle <= Pi which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The angle in the range [`-Math.PI`, `Math.PI`].</returns>
        public static double NegativePiToPi(double angle)
        {
            if (angle >= -Math.PI && angle <= Math.PI)
            { 
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            return Helpers.ZeroToTwoPi(angle + Math.PI) - Math.PI;
        }

        /// <summary>
        /// Produces an angle in the range -180 <= angle <= 180 which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The angle in the range [-180, 180].</returns>
        public static double Negative180To180(double angle)
        {
            if (angle >= -180.0 && angle <= 180.0)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            return Helpers.ZeroTo360(angle + 180.0) - 180.0;
        }

        /// <summary>
        /// Produces an angle in the range 0 <= angle <= 2Pi which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The angle in the range [0, `Math::TwoPi`]</returns>
        public static double ZeroToTwoPi(double angle)
        {
            if (angle >= 0 && angle <= 2.0 * Math.PI)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            double mod = Helpers.Mod(angle, 2.0 * Math.PI);
            if (Math.Abs(mod) < 1.0e-14 && Math.Abs(angle) > 1.0e-14)
            {
                return 2.0 * Math.PI;
            }
            return mod;
        }

        /// <summary>
        /// Produces an angle in the range 0 <= angle <= 360 which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The angle in the range [0, 360]</returns>
        public static double ZeroTo360(double angle)
        {
            if (angle >= 0 && angle <= 360.0)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            double mod = Helpers.Mod(angle, 360.0);
            if (Math.Abs(mod) < 1.0e-14 && Math.Abs(angle) > 1.0e-14)
            {
                return 360.0;
            }
            return mod;
        }

        /// <summary>
        /// The modulo operation that also works for negative dividends.
        /// </summary>
        /// <param name="m">The dividend.</param>
        /// <param name="n">The divisor.</param>
        /// <returns>The remainder.</returns>
        public static double Mod(double m, double n)
        {
            if (Math.Sign(m) == Math.Sign(n) && Math.Abs(m) < Math.Abs(n)) {
              // Early exit if the input does not need to be modded. This avoids
              // unnecessary math which could introduce floating point error.
              return m;
            }
            return ((m % n) + n) % n;
        }
    }
}
