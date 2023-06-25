using OpenGL;
using SolarSystem.Math;
using SolarSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal class Mercury : BodyBase
    {
        private const string MERCURY_DESCRIPTION =
            "Меркурий - наименьшая планета Солнечной системы и самая близкая к Солнцу. " +
            "Названа в честь древнеримского бога торговли - быстрого Меркурия, поскольку она движется по небу быстрее других планет. " +
            "Её период обращения вокруг Солнца составляет всего 87,97 земных суток - самый короткий среди всех планет Солнечной системы.";

        public Mercury() : base()
        {
            this.name = "Меркурий";
            this.mass = 3.33022e23;
            this.equatorialRadius = 2_439_700.0;
            this.polarRadius = 2_439_700.0;
            this.radius = 2_439_700.0;
            this.flattening = 0.0;
            this.surfaceArea = 7.48e7;
            this.volume = 6.083e10;
            this.density = 5.427;
            this.surfaceAcceleration = 3.7;
            this.escapeVelocity = 4_250.0;
            this.equatorialVelocity = 3.026;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 57.909e9,
                siderialOrbitPeriod = 87.969,
                perihelion = 46.0e9,
                aphelion = 69.818e9,
                meanVelocity = 47_360.0,
                maxVelocity = 58_970.0,
                minVelocity = 38_860.0,
                orbitInclination = 7.004,
                eccentricity = 0.2056,
                siderialRotationPeriod = 1407.6,
                equatorInclination = 0.034,

                ascendingNodeLongitude = 48.33167,
                periapsisArgument = 29.124279,
                meanAnomaly = 174.795884
            };

            this.description = MERCURY_DESCRIPTION;

            this.orbitSamples = 360;
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
            // Semi-major axis [m]
            double a = MathHelper.AU * 0.3870983098;
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                252.25090552
                + (5381016286.88982 / 3600.0) * t
                - (1.92789 / 3600.0) * t2
                + (0.00639 / 3600.0) * t3);
            // Eccentricity
            double e = 0.2056317526
                + 0.0002040653 * t
                - 28349.0e-10 * t2
                - 1805.0e-10 * t3
                + 23.0e-10 * t4
                - 2.0e-10 * t5;
            // Longitude of the periapsis [degrees]
            double omega = 77.45611904
                + (5719.11590 / 3600.0) * t
                - (4.83016 / 3600.0) * t2
                - (0.02464 / 3600.0) * t3
                - (0.00016 / 3600.0) * t4
                + (0.00004 / 3600.0) * t5;
            // Inclination [degrees]
            double i = 7.00498625
                - (214.25629 / 3600.0) * t
                + (0.28977 / 3600.0) * t2
                + (0.15421 / 3600.0) * t3
                - (0.00169 / 3600.0) * t4
                - (0.00002 / 3600.0) * t5;
            // Longitude of the ascending node [degrees]
            double Omega = 48.33089304
                - (4515.21727 / 3600.0) * t
                - (31.79892 / 3600.0) * t2
                - (0.71933 / 3600.0) * t3
                + (0.01242 / 3600.0) * t4;

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
            this.textures[0] = AssetManager.LoadTexture2D("mercury_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
