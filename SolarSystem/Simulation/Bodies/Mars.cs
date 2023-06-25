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
    internal class Mars : BodyBase
    {
        private const string MARS_DESCRIPTION =
            "Марс - четвёртая по удалённости от Солнца и седьмая по размеру планета Солнечной системы; " +
            "масса планеты составляет 10,7 % массы Земли. Названа в честь Марса - древнеримского бога войны, соответствующего древнегреческому Аресу. " +
            "Также Марс называют «красной планетой» из-за красноватого оттенка поверхности, придаваемого ей минералом маггемитом - γ-оксидом железа(III).";

        public Mars() : base()
        {
            this.name = "Марс";
            this.mass = 6.4171e23;
            this.equatorialRadius = 3_396_200.0;
            this.polarRadius = 3_376_200.0;
            this.radius = 3_389_500.0;
            this.flattening = 0.00589;
            this.surfaceArea = 1.4437e8;
            this.volume = 1.6318e11;
            this.density = 3.933;
            this.surfaceAcceleration = 3.711;
            this.escapeVelocity = 5_030.0;
            this.equatorialVelocity = 241.0;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 227.956e9,
                siderialOrbitPeriod = 686.980,
                perihelion = 206.650e9,
                aphelion = 249.261e9,
                meanVelocity = 24_080.0,
                maxVelocity = 26_500.0,
                minVelocity = 21_970.0,
                orbitInclination = 1.848,
                eccentricity = 0.0935,
                siderialRotationPeriod = 24.6229,
                equatorInclination = 25.19,

                ascendingNodeLongitude = 49.57854,
                periapsisArgument = 286.46230,
                meanAnomaly = 19.412
            };

            this.description = MARS_DESCRIPTION;

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
            double a = MathHelper.AU * (1.5236793419 + 3.0e-10 * t);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                355.43299958
                + (689050774.93988 / 3600.0) * t
                + (0.94264 / 3600.0) * t2
                - (0.01043 / 3600.0) * t3);
            // Eccentricity
            double e = 0.0934006477
                + 0.0009048438 * t
                - 80641.0e-10 * t2
                - 2519.0e-10 * t3
                + 124.0e-10 * t4
                - 10.0e-10 * t5;
            // Longitude of the periapsis [degrees]
            double omega = 336.06023395
                + (15980.45908 / 3600.0) * t
                - (62.32800 / 3600.0) * t2
                + (1.86464 / 3600.0) * t3
                - (0.04603 / 3600.0) * t4
                - (0.00164 / 3600.0) * t5;
            // Inclination [degrees]
            double i = 1.84972648
                - (293.31722 / 3600.0) * t
                - (8.11830 / 3600.0) * t2
                - (0.10326 / 3600.0) * t3
                - (0.00153 / 3600.0) * t4
                + (0.00048 / 3600.0) * t5;
            // Longitude of the ascending node [degrees]
            double Omega = 49.55809321
                - (10620.90088 / 3600.0) * t
                - (230.57416 / 3600.0) * t2
                - (7.06942 / 3600.0) * t3
                - (0.68920 / 3600.0) * t4
                - (0.05829 / 3600.0) * t5;

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
            this.textures[0] = AssetManager.LoadTexture2D("mars_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
