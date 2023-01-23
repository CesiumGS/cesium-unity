using System;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    internal class Helpers
    {
        public static string ToString<T>(T value)
        {
            return value.ToString();
        }

        public static double4x4 ToMathematics(Matrix4x4 matrix)
        {
            return new double4x4(
                matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                matrix.m30, matrix.m31, matrix.m32, matrix.m33);
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

        internal static void MatrixToRotationAndScale(double3x3 matrix, out Quaternion rotation, out Vector3 scale)
        {
            double lengthColumn0 = math.length(matrix.c0);
            double lengthColumn1 = math.length(matrix.c1);
            double lengthColumn2 = math.length(matrix.c2);

            float3x3 rotationMatrix = new float3x3(
                (float3)(matrix.c0 / lengthColumn0),
                (float3)(matrix.c1 / lengthColumn1),
                (float3)(matrix.c2 / lengthColumn2));

            scale = new Vector3((float)lengthColumn0, (float)lengthColumn1, (float)lengthColumn2);

            double3 cross = math.cross(matrix.c0, matrix.c1);
            if (math.dot(cross, matrix.c2) < 0.0)
            {
                rotationMatrix *= -1.0f;
                scale *= -1.0f;
            }

            rotation = math.quaternion(rotationMatrix);
        }
    }
}
