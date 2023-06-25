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
    internal class Uranus : BodyBase
    {
        private const string URANUS_DESCRIPTION =
            "Уран - планета Солнечной системы, седьмая по удалённости от Солнца, третья по диаметру и четвёртая по массе. " +
            "Была открыта в 1781 году английским астрономом Уильямом Гершелем и названа в честь греческого бога неба Урана. " +
            "Уран стал первой планетой, обнаруженной в Новое время и при помощи телескопа. Его открыл Уильям Гершель 13 марта 1781 года, " +
            "тем самым впервые со времён античности расширив границы Солнечной системы в глазах человека. " +
            "Несмотря на то, что порой Уран различим невооружённым глазом, более ранние наблюдатели принимали его за тусклую звезду.";

        public Uranus() : base()
        {
            this.name = "Уран";
            this.mass = 8.6813e25;
            this.equatorialRadius = 25_559_000.0;
            this.polarRadius = 24_973_000.0;
            this.radius = 25_362_000.0;
            this.flattening = 0.02293;
            this.surfaceArea = 8.1156e9;
            this.volume = 6.833e13;
            this.density = 1.27;
            this.surfaceAcceleration = 8.87;
            this.escapeVelocity = 21_300.0;
            this.equatorialVelocity = 2_590.0;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 2_867.043e9,
                siderialOrbitPeriod = 30_685.4,
                perihelion = 2_737.696e9,
                aphelion = 3_001.390e9,
                meanVelocity = 6_790.0,
                maxVelocity = 7_130.0,
                minVelocity = 6_490.0,
                orbitInclination = 0.770,
                eccentricity = 0.0469,
                siderialRotationPeriod = 17.24,
                equatorInclination = 97.77,

                ascendingNodeLongitude = 73.989821,
                periapsisArgument = 96.541318,
                meanAnomaly = 142.955717
            };

            this.description = URANUS_DESCRIPTION;

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
                19.2184460618
                - 3716.0e-10 * t
                + 979.0e-10 * t2);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                314.05500511
                + (15424811.93933 / 3600.0) * t
                - (1.75083 / 3600.0) * t2
                + (0.02156 / 3600.0) * t3);
            // Eccentricity
            double e = 0.0463812221
                - 0.0002729293 * t
                + 0.0000078913 * t2
                + 2447.0e-10 * t3
                - 171.0e-10 * t4;
            // Longitude of the periapsis [degrees]
            double omega = 173.00529106
                + (3215.56238 / 3600.0) * t
                - (34.09288 / 3600.0) * t2
                + (1.48909 / 3600.0) * t3
                + (0.06600 / 3600.0) * t4;
            // Inclination [degrees]
            double i = 0.77319689
                - (60.72723 / 3600.0) * t
                + (1.25759 / 3600.0) * t2
                + (0.05808 / 3600.0) * t3
                + (0.00031 / 3600.0) * t4;
            // Longitude of the ascending node [degrees]
            double Omega = 74.00595701
                + (2669.15033 / 3600.0) * t
                + (145.93964 / 3600.0) * t2
                + (0.42917 / 3600.0) * t3
                - (0.09120 / 3600.0) * t4;

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
            this.textures[0] = AssetManager.LoadTexture2D("uranus_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);

            this.ringMesh = AssetManager.LoadMesh("uranusRings.obj");
            this.ringMesh.LoadRenderData();

            this.ringMaterial = AssetManager.LoadProgram("baseTexture");

            this.ringTextures = new Texture[1];
            this.ringTextures[0] = AssetManager.LoadTexture2D("uranus_ring.png");
            this.ringTextures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear, 16.0f);
        }
    }
}
