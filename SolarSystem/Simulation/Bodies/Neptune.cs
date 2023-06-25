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
    internal class Neptune : BodyBase
    {
        private const string NEPTUNE_DESCRIPTION =
            "Нептун - восьмая и самая дальняя от Солнца планета Солнечной системы. " +
            "Его масса превышает массу Земли в 17,2 раза и является третьей среди планет Солнечной системы, " +
            "а по экваториальному диаметру Нептун занимает четвёртое место, превосходя Землю в 3,9 раза. Планета названа в честь Нептуна - римского бога морей. " +
            "Обнаруженный 23 сентября 1846 года, Нептун стал первой планетой, открытой благодаря математическим расчётам. " +
            "Обнаружение непредсказуемых изменений орбиты Урана породило гипотезу о неизвестной планете, гравитационным возмущающим влиянием которой они и обусловлены.";

        public Neptune() : base()
        {
            this.name = "Нептун";
            this.mass = 1.0243e26;
            this.equatorialRadius = 24_764_000.0;
            this.polarRadius = 24_341_000.0;
            this.radius = 24_632_000.0;
            this.flattening = 0.0171;
            this.surfaceArea = 7.6408e9;
            this.volume = 6.254e13;
            this.density = 1.638;
            this.surfaceAcceleration = 11.15;
            this.escapeVelocity = 23_500.0;
            this.equatorialVelocity = 2_680.0;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 4_514.953e9,
                siderialOrbitPeriod = 60_189.0,
                perihelion = 4_471.050e9,
                aphelion = 4_558.857e9,
                meanVelocity = 5_450.0,
                maxVelocity = 5_470.0,
                minVelocity = 5_370.0,
                orbitInclination = 1.770,
                eccentricity = 0.0097,
                siderialRotationPeriod = 16.11,
                equatorInclination = 28.32,

                ascendingNodeLongitude = 131.7794310,
                periapsisArgument = 265.646853,
                meanAnomaly = 267.767281
            };

            this.description = NEPTUNE_DESCRIPTION;

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
            // Semi-major axis [m]
            double a = MathHelper.AU * (
                30.1103868694
                - 16635.0e-10 * t
                + 686.0e-10 * t2);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                304.34866548
                + (7865503.20744 / 3600.0) * t
                + (0.21103 / 3600.0) * t2
                - (0.00895 / 3600.0) * t3);
            // Eccentricity
            double e = 0.0094557470
                + 0.0000603263 * t
                + 0.0 * t2
                - 483.0e-10 * t3;
            // Longitude of the periapsis [degrees]
            double omega = 48.12027554
                + (1050.71912 / 3600.0) * t
                + (27.39717 / 3600.0) * t2;
            // Inclination [degrees]
            double i = 1.76995259
                + (8.12333 / 3600.0) * t
                + (0.08135 / 3600.0) * t2
                - (0.00046 / 3600.0) * t3;
            // Longitude of the ascending node [degrees]
            double Omega = 131.78405702
                - (221.94322 / 3600.0) * t
                - (0.78728 / 3600.0) * t2
                - (0.28070 / 3600.0) * t3
                + (0.00049 / 3600.0) * t4;

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
            this.textures[0] = AssetManager.LoadTexture2D("neptune_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
