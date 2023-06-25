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
    internal class Saturn : BodyBase
    {
        private const string SATURN_DESCRIPTION =
            "Сатурн - шестая планета по удалённости от Солнца и вторая по размерам планета в Солнечной системе после Юпитера. " +
            "Сатурн классифицируется как газовая планета-гигант. Сатурн назван в честь римского бога земледелия. " +
            "В основном Сатурн состоит из водорода, с примесями гелия и следами воды, метана, аммиака и тяжёлых элементов. " +
            "Внутренняя область представляет собой относительно небольшое ядро из железа, никеля и льда, покрытое тонким слоем металлического водорода и газообразным внешним слоем.";

        public Saturn() : base()
        {
            this.name = "Сатурн";
            this.mass = 5.6846e26;
            this.equatorialRadius = 60_268_000.0;
            this.polarRadius = 54_364_000.0;
            this.radius = 58_232_000.0;
            this.flattening = 0.09796;
            this.surfaceArea = 4.272e10;
            this.volume = 8.2713e14;
            this.density = 0.687;
            this.surfaceAcceleration = 10.44;
            this.escapeVelocity = 35_500.0;
            this.equatorialVelocity = 9_870.0;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 1_432.041e9,
                siderialOrbitPeriod = 10_795.22,
                perihelion = 1_357.554e9,
                aphelion = 1_506.527e9,
                meanVelocity = 9_670.0,
                maxVelocity = 10_140.0,
                minVelocity = 9_120.0,
                orbitInclination = 2.486,
                eccentricity = 0.0520,
                siderialRotationPeriod = 10.656,
                equatorInclination = 26.73,

                ascendingNodeLongitude = 113.642,
                periapsisArgument = 336.013,
                meanAnomaly = 317.020
            };

            this.description = SATURN_DESCRIPTION;

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
                9.5549091915
                - 0.0000213896 * t
                + 444.0e-10 * t2
                + 670.0e-10 * t3
                + 110.0e-10 * t4
                - 7.0e-10 * t5
                - 1.0e-10 * t6);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                50.07744430
                + (43996098.55732 / 3600.0) * t
                + (75.61614 / 3600.0) * t2
                - (0.16618 / 3600.0) * t3
                - (0.11484 / 3600.0) * t4
                - (0.01452 / 3600.0) * t5
                + (0.00083 / 3600.0) * t6);
            // Eccentricity
            double e = 0.0555481426
                - 0.0034664062 * t
                - 0.0000643639 * t2
                + 33956.0e-10 * t3
                - 219.0e-10 * t4
                - 3.0e-10 * t5
                + 6.0e-10 * t6;
            // Longitude of the periapsis [degrees]
            double omega = 93.05723748
                + (20395.49439 / 3600.0) * t
                + (190.25952 / 3600.0) * t2
                + (17.68303 / 3600.0) * t3
                + (1.23148 / 3600.0) * t4
                + (0.10310 / 3600.0) * t5
                + (0.00702 / 3600.0) * t6;
            // Inclination [degrees]
            double i = 2.48887878
                + (91.85195 / 3600.0) * t
                - (17.66225 / 3600.0) * t2
                + (0.06105 / 3600.0) * t3
                + (0.02638 / 3600.0) * t4
                - (0.00152 / 3600.0) * t5
                - (0.00012 / 3600.0) * t6;
            // Longitude of the ascending node [degrees]
            double Omega = 113.66550252
                - (9240.19942 / 3600.0) * t
                - (66.23743 / 3600.0) * t2
                + (1.72778 / 3600.0) * t3
                + (0.26990 / 3600.0) * t4
                + (0.03610 / 3600.0) * t5
                - (0.00248 / 3600.0) * t6;

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
            this.textures[0] = AssetManager.LoadTexture2D("saturn_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);

            this.ringMesh = AssetManager.LoadMesh("saturnRings.obj");
            this.ringMesh.LoadRenderData();

            this.ringMaterial = AssetManager.LoadProgram("baseTexture");

            this.ringTextures = new Texture[1];
            this.ringTextures[0] = AssetManager.LoadTexture2D("saturn_ring_alpha_2k.png");
            this.ringTextures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear, 16.0f);
        }
    }
}
