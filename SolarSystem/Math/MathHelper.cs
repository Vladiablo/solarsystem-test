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
    public static class MathHelper
    {
        public const float PI = 3.14159265358979323846264338327950288f;
        public const float TAU = PI * 2.0f;
        public const float DEG_TO_RAD = PI / 180.0f;
        public const float RAD_TO_DEG = 180.0f / PI;

        public const double DEG_TO_RAD64 = (double)PI / 180.0;
        public const double RAD_TO_DEG64 = 180.0 / (double)PI;

        public const double NEAR_ZERO = 1.0e-3;
        public const double G = 6.67430e-11;
        /// <summary>
        /// Astronomical Unit value in meters.
        /// </summary>
        public const double AU = 149_597_870_700.0;

        public const double J2000 = 2451545.0;

        /// <summary>
        /// Simulation values may be too large to fit into regular floats.<br/>
        /// The values must be scaled by this constant before rendering.
        /// </summary>
        public const double RENDER_TO_SIMULATION_SCALE = 1e-9;

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
        public static double DegToRad(double degrees)
        {
            return degrees * DEG_TO_RAD64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RadToDeg(double radians)
        {
            return radians * RAD_TO_DEG64;
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
        public static float ClampRotation(float angle)
        {
            angle %= 360.0f;

            if (angle < 0.0f)
                angle += 360.0f;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeRotation(float angle)
        {
            angle = ClampRotation(angle);

            if (angle > 180.0f)
                angle -= 360.0f;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ClampRotation(double angle)
        {
            angle %= 360.0;

            if (angle < 0.0)
                angle += 360.0;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NormalizeRotation(double angle)
        {
            angle = ClampRotation(angle);

            if (angle > 180.0)
                angle -= 360.0;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampRotation(in Vector3 rotation)
        {
            return new Vector3(
                ClampRotation(rotation.X),
                ClampRotation(rotation.Y),
                ClampRotation(rotation.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeRotation(in Vector3 rotation)
        {
            return new Vector3(
                NormalizeRotation(rotation.X),
                NormalizeRotation(rotation.Y),
                NormalizeRotation(rotation.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateRotationMatrix(in Vector3 rotation)
        {
            float sx = MathF.Sin(rotation.X);
            float sy = MathF.Sin(rotation.Y);
            float sz = MathF.Sin(rotation.Z);
            float cx = MathF.Cos(rotation.X);
            float cy = MathF.Cos(rotation.Y);
            float cz = MathF.Cos(rotation.Z);

            Matrix4x4 mat = new Matrix4x4()
            {
                M11 = cy * cz,
                M12 = cy * sz,
                M13 = -sy,
                M14 = 0.0f,
                M21 = sx * sy * cz - cx * sz,
                M22 = sx * sy * sz + cx * cz,
                M23 = sx * cy,
                M24 = 0.0f,
                M31 = cx * sy * cz + sx * sz,
                M32 = cx * sy * sz - sx * cz,
                M33 = cx * cy,
                M34 = 0.0f,
                M41 = 0.0f,
                M42 = 0.0f,
                M43 = 0.0f,
                M44 = 1.0f,
            };

            return mat;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToJulianDate(in DateTime gregorianDate)
        {
            int a = (14 - gregorianDate.Month) / 12;
            int y = gregorianDate.Year + 4800 - a;
            int m = gregorianDate.Month + 12 * a - 3;

            int jdn = gregorianDate.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
            return (double)jdn
                + ((double)gregorianDate.Hour - 12.0) / 24.0
                + (double)gregorianDate.Minute / 1440.0
                + (double)gregorianDate.Second / 86400.0;
        }

    }
}
