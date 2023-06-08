using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Math
{
    public static class Math
    {
        public const double G = 6.67430e-11;
        public const double AU = 149_597_870_700.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<double> NormalizeVector(in Vector<double> vector)
        {
            double lengthSquared = Vector.Dot(vector, vector);
            return vector / new Vector<double>(lengthSquared);
        }
    }
}
