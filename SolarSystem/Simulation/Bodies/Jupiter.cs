using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using SolarSystem.Rendering;
using SolarSystem.Math;

namespace SolarSystem.Simulation.Bodies
{
    internal class Jupiter : BodyBase
    {
        private const string JUPITER_DESCRIPTION =
            "Юпитер - крупнейшая планета Солнечной системы, пятая по удалённости от Солнца. Наряду с Сатурном Юпитер классифицируется как газовый гигант. " +
            "Планета была известна людям с глубокой древности, что нашло своё отражение в мифологии и религиозных верованиях различных культур: месопотамской, вавилонской, греческой и других. " +
            "Современное название Юпитера происходит от имени древнеримского верховного бога-громовержца. " +
            "Ряд атмосферных явлений на Юпитере - штормы, молнии, полярные сияния, - имеет масштабы, на порядки превосходящие земные. " +
            "Примечательным образованием в атмосфере является Большое красное пятно - гигантский шторм, известный с XVII века.";

        public Jupiter() : base()
        {
            this.name = "Юпитер";
            this.mass = 1.8986e27;
            this.equatorialRadius = 71_492_000.0;
            this.polarRadius = 66_854_000.0;
            this.radius = 69_911_000.0;
            this.flattening = 0.06487;
            this.surfaceArea = 6.21796e10;
            this.volume = 1.433128e15;
            this.density = 1.326;
            this.surfaceAcceleration = 24.79;
            this.escapeVelocity = 59_500.0;
            this.equatorialVelocity = 12_600.0;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 778.479e9,
                siderialOrbitPeriod = 4_332.589,
                perihelion = 740.595e9,
                aphelion = 816.363e9,
                meanVelocity = 13_060.0,
                maxVelocity = 13_720.0,
                minVelocity = 12_440.0,
                orbitInclination = 1.304,
                eccentricity = 0.0487,
                siderialRotationPeriod = 9.9250,
                equatorInclination = 3.13,

                ascendingNodeLongitude = 100.55615,
                periapsisArgument = 275.066,
                meanAnomaly = 20.020
            };

            this.description = JUPITER_DESCRIPTION;

            this.orbitSamples = 720;
            this.CreateOrbitBuffer();
        }

        public override OrbitData CalculateOrbitData(double timeDJ)
        {
            // Barycentric time (TDB) in thousands of Julian years since J2000.0
            double t = (timeDJ - MathHelper.J2000) / 365250.0;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;
            double t6 = t5 * t;
            // Semi-major axis [m]
            double a = MathHelper.AU * (
                5.2026032092 
                + 19132.0e-10 * t
                - 39.0e-10 * t2
                - 60.0e-10 * t3
                - 10.0e-10 * t4
                + 1.0e-10 * t5);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                34.35151874
                + (109256603.77991 / 3600.0) * t
                - (30.60378 / 3600.0) * t2
                + (0.05706 / 3600.0) * t3
                + (0.04667 / 3600.0) * t4
                - (0.00591 / 3600.0) * t5
                - (0.00034 / 3600.0) * t6);
            // Eccentricity
            double e = 0.0484979255
                + 0.0016322542 * t
                - 0.0000471366 * t2
                - 20063.0e-10 * t3
                + 1018.0e-10 * t4
                - 21.0e-10 * t5
                + 1.0e-10 * t6;
            // Longitude of the periapsis [degrees]
            double omega = 14.33120687
                + (7758.75163 / 3600.0) * t
                + (259.95938 / 3600.0) * t2
                - (16.14731 / 3600.0) * t3
                + (0.74704 / 3600.0) * t4
                - (0.02087 / 3600.0) * t5
                - (0.00016 / 3600.0) * t6;
            // Inclination [degrees]
            double i = 1.30326698
                - (71.55890 / 3600.0) * t
                + (11.95297 / 3600.0) * t2
                + (0.34909 / 3600.0) * t3
                - (0.02710 / 3600.0) * t4
                - (0.00124 / 3600.0) * t5
                + (0.00003 / 3600.0) * t6;
            // Longitude of the ascending node [degrees]
            double Omega = 100.46440702
                + (6362.03561 / 3600.0) * t
                + (326.52178 / 3600.0) * t2
                - (26.18091 / 3600.0) * t3
                - (2.10322 / 3600.0) * t4
                + (0.04459 / 3600.0) * t5
                + (0.01154 / 3600.0) * t6;

            this.currentOrbitData = new OrbitData
            {
                semiMajorAxis = a,
                eccentricity = e,
                periapsisArgument = omega - Omega,
                orbitInclination = i,
                ascendingNodeLongitude = Omega,
                meanAnomaly = MathHelper.NormalizeRotation(l - omega)
            };

            this.LoadOrbitVertices(CalculateOrbitVertices(this.currentOrbitData, this.orbitSamples));
            return this.currentOrbitData;
        }

        public override void LoadRenderData()
        {
            this.mesh = AssetManager.LoadMesh("sphere.obj");
            this.mesh.LoadRenderData();

            this.material = AssetManager.LoadProgram("phongNoSpecular");

            this.textures = new Texture[1];
            this.textures[0] = AssetManager.LoadTexture2D("jupiter_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
