using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Math
{
    public static class Math
    {
        public const float PI = 3.14159265358979323846264338327950288f;
        public const float DEG_TO_RAD = PI / 180.0f;
        public const float RAD_TO_DEG = 180.0f / PI;

        public const double NEAR_ZERO = 1e-3;
        public const double G = 6.67430e-11;
        /// <summary>
        /// Astronomical Unit value in meters.
        /// </summary>
        public const double AU = 149_597_870_700.0;

        /// <summary>
        /// Simulation values may be too large to fit into regular floats.<br/>
        /// The values must be scaled by this constant before rendering.
        /// </summary>
        public const double RENDER_TO_SIMULATION_SCALE = 1e-6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<double> NormalizeVector(in Vector<double> vector)
        {
            double lengthSquared = Vector.Dot(vector, vector);
            return vector / new Vector<double>(System.Math.Sqrt(lengthSquared));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ConvertDoubleVectorToFloat(in Vector<double> vec)
        {
            if (Avx.IsSupported)
                return Avx.ConvertToVector128Single(vec.AsVector256()).AsVector4();

            return new Vector4((float)vec[0], (float)vec[1], (float)vec[2], (float)vec[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DegToRad(float degrees)
        {
            return degrees * DEG_TO_RAD;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadToDeg(float radians)
        {
            return radians * RAD_TO_DEG;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DegToRad(in Vector3 degrees)
        {
            return degrees * DEG_TO_RAD;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RadToDeg(in Vector3 radians)
        {
            return radians * RAD_TO_DEG;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreatePerspectiveMatrix(float fovY, float width, float height, float zNear, float zFar)
        {
            float ar = width / height;
            float zRange = zNear - zFar;
            float tanHalfFov = MathF.Tan(fovY / 2.0f);

            return new Matrix4x4
            {
                M11 = 1.0f / (tanHalfFov * ar),
                M12 = 0.0f,
                M13 = 0.0f,
                M14 = 0.0f,

                M21 = 0.0f,
                M22 = 1.0f / tanHalfFov,
                M23 = 0.0f,
                M24 = 0.0f,

                M31 = 0.0f,
                M32 = 0.0f,
                M33 = (-zNear - zFar) / zRange,
                M34 = 2.0f * zFar * zNear / zRange,

                M41 = 0.0f,
                M42 = 0.0f,
                M43 = 1.0f,
                M44 = 0.0f
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateLookAtMatrix(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 upVector)
        {
            return CreateLookFromMatrix(cameraPosition, cameraTarget - cameraPosition, upVector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateLookFromMatrix(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 upVector)
        {
            Vector3 zAxis = Vector3.Normalize(cameraDirection); // Forward
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(zAxis, upVector)); // Right
            Vector3 yAxis = Vector3.Cross(xAxis, zAxis); // Up

            return new Matrix4x4
            {
                M11 = xAxis.X,
                M12 = yAxis.X,
                M13 = -zAxis.X,
                M14 = 0.0f,

                M21 = xAxis.Y,
                M22 = yAxis.Y,
                M23 = -zAxis.Y,
                M24 = 0.0f,

                M31 = xAxis.Z,
                M32 = yAxis.Z,
                M33 = -zAxis.Z,
                M34 = 0.0f,

                M41 = -Vector3.Dot(cameraPosition, xAxis),
                M42 = -Vector3.Dot(cameraPosition, yAxis),
                M43 = Vector3.Dot(cameraPosition, zAxis),
                M44 = 1.0f
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 TranslateMatrix(Matrix4x4 mat, Vector3 translation)
        {
            return new Matrix4x4
            {
                M11 = mat.M11,
                M12 = mat.M12,
                M13 = mat.M13,
                M14 = mat.M14,

                M21 = mat.M21,
                M22 = mat.M22,
                M23 = mat.M23,
                M24 = mat.M24,

                M31 = mat.M31,
                M32 = mat.M32,
                M33 = mat.M33,
                M34 = mat.M34,

                M41 = mat.M11 * translation.X + mat.M21 * translation.Y + mat.M31 * translation.Z + mat.M41,
                M42 = mat.M12 * translation.X + mat.M22 * translation.Y + mat.M32 * translation.Z + mat.M42,
                M43 = mat.M13 * translation.X + mat.M23 * translation.Y + mat.M33 * translation.Z + mat.M43,
                M44 = mat.M44,
            };
        }

    }
}
