using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation
{
    internal struct OrbitData
    {
        public double semiMajorAxis; // alpha
        public double siderialOrbitPeriod;
        public double perihelion;
        public double aphelion;

        public double meanVelocity;
        public double maxVelocity;
        public double minVelocity;

        public double orbitInclination; // i
        public double eccentricity; // e

        public double siderialRotationPeriod;

        public double ascendingNodeLongitude; // Omega
        public double equatorInclination;

        public double periapsisArgument; // omega
        public double meanAnomaly; // M0
    }
}
