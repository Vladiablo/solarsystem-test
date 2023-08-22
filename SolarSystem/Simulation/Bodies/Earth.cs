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
    internal class Earth : BodyBase
    {
        private const string EARTH_DESCRIPTION =
            "Земля - третья по удалённости от Солнца планета Солнечной системы. " +
            "Самая плотная, пятая по диаметру и массе среди всех планет Солнечной системы и крупнейшая среди планет земной группы, " +
            "в которую входят также Меркурий, Венера и Марс. " +
            "Единственное известное человеку в настоящее время тело во Вселенной, населённое живыми организмами.";

        public Earth() : base()
        {
            this.name = "Земля";
            this.mass = 5.9726e24;
            this.equatorialRadius = 6_378_100.0;
            this.polarRadius = 6_356_800.0;
            this.radius = 6_371_000.0;
            this.flattening = 0.0033528;
            this.surfaceArea = 510_072_000.0;
            this.volume = 1.08321e12;
            this.density = 5.5153;
            this.surfaceAcceleration = 9.780327;
            this.escapeVelocity = 11_186.0;
            this.equatorialVelocity = 465.1;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 149.598e9,
                siderialOrbitPeriod = 365.256,
                perihelion = 147.095e9,
                aphelion = 152.100e9,
                meanVelocity = 29_780.0,
                maxVelocity = 30_290.0,
                minVelocity = 29_290.0,
                orbitInclination = 0.0,
                eccentricity = 0.0167,
                siderialRotationPeriod = 23.9345,
                equatorInclination = 23.44,

                ascendingNodeLongitude = 348.73936,
                periapsisArgument = 114.20783,
                meanAnomaly = 357.51716
            };

            this.description = EARTH_DESCRIPTION;

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
            double t6 = t5 * t;
            // Semi-major axis [m]
            double a = MathHelper.AU * 1.0000010178;
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                100.46645683
                + (1295977422.83429 / 3600.0) * t
                - (2.04411 / 3600.0) * t2
                - (0.00523 / 3600.0) * t3);
            // Eccentricity
            double e = 0.0167086342
                - 0.0004203654 * t
                - 0.0000126734 * t2
                + 1444.0e-10 * t3
                - 2.0e-10 * t4
                + 3.0e-10 * t5;
            // Longitude of the periapsis [degrees]
            double omega = 102.93734808
                + (11612.35290 / 3600.0) * t
                + (53.27577 / 3600.0) * t2
                - (0.14095 / 3600.0) * t3
                + (0.11440 / 3600.0) * t4
                + (0.00478 / 3600.0) * t5;
            // Inclination [degrees]
            double i = (469.97289 / 3600.0) * t
                - (3.35053 / 3600.0) * t2
                - (0.12374 / 3600.0) * t3
                + (0.00027 / 3600.0) * t4
                - (0.00001 / 3600.0) * t5
                + (0.00001 / 3600.0) * t6;
            // Longitude of the ascending node [degrees]
            double Omega = 174.87317577
                - (8679.27034 / 3600.0) * t
                + (15.34191 / 3600.0) * t2
                + (0.00532 / 3600.0) * t3
                - (0.03734 / 3600.0) * t4
                - (0.00073 / 3600.0) * t5
                + (0.00004 / 3600.0) * t6;

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

            this.material = AssetManager.LoadProgram("earth");

            this.textures = new Texture[3];
            this.textures[0] = AssetManager.LoadTexture2D("earth_day_8k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear, 16.0f);

            this.textures[1] = AssetManager.LoadTexture2D("earth_night_8k.jpg");
            this.textures[1].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear, 16.0f);

            this.textures[2] = AssetManager.LoadTexture2D("earth_specular_2k.tif");
            this.textures[2].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
