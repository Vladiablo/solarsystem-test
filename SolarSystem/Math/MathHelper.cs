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

    }
}
