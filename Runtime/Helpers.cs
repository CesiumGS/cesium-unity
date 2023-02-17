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
    }
}
