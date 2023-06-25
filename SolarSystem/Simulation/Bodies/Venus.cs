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
    internal class Venus : BodyBase
    {
        private const string VENUS_DESCRIPTION =
            "Венера - вторая по удалённости от Солнца и шестая по размеру планета Солнечной системы, " +
            "наряду с Меркурием, Землёй и Марсом принадлежащая к семейству планет земной группы. Названа в честь древнеримской богини любви Венеры. " +
            "По ряду характеристик - например, по массе и размерам - Венера считается «сестрой» Земли. Венерианский год составляет 224,7 земных суток. " +
            "Она имеет самый длинный период вращения вокруг своей оси (около 243 земных суток, в среднем 243,0212 ± 0,00006 сут) среди всех планет " +
            "Солнечной системы и вращается в направлении, противоположном направлению вращения большинства планет.";

        public Venus() : base()
        {
            this.name = "Венера";
            this.mass = 4.8675e24;
            this.equatorialRadius = 6_051_800.0;
            this.polarRadius = 6_051_800.0;
            this.radius = 6_051_800.0;
            this.flattening = 0.0;
            this.surfaceArea = 4.60e8;
            this.volume = 9.38e11;
            this.density = 5.24;
            this.surfaceAcceleration = 8.87;
            this.escapeVelocity = 10_363.0;
            this.equatorialVelocity = 1.81;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 108.210e9,
                siderialOrbitPeriod = 224.701,
                perihelion = 107.480e9,
                aphelion = 108.941e9,
                meanVelocity = 35_020.0,
                maxVelocity = 35_260.0,
                minVelocity = 34_780.0,
                orbitInclination = 3.395,
                eccentricity = 0.0068,
                siderialRotationPeriod = 5832.6,
                equatorInclination = 177.36,

                ascendingNodeLongitude = 76.67069,
                periapsisArgument = 54.85229,
                meanAnomaly = 50.115
            };

            this.description = VENUS_DESCRIPTION;

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
            double a = MathHelper.AU * 0.7233298200;
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(
                181.97980085
                + (2106641364.33548 / 3600.0) * t
                + (0.59381 / 3600.0) * t2
                - (0.00627 / 3600.0) * t3);
            // Eccentricity
            double e = 0.0067719164
                - 0.0004776521 * t
                + 98127.0e-10 * t2
                + 4639.0e-10 * t3
                + 123.0e-10 * t4
                - 3.0e-10 * t5;
            // Longitude of the periapsis [degrees]
            double omega = 131.56370300
                + (175.48640 / 3600.0) * t
                - (498.48184 / 3600.0) * t2
                - (20.50042 / 3600.0) * t3
                - (0.72432 / 3600.0) * t4
                + (0.00224 / 3600.0) * t5;
            // Inclination [degrees]
            double i = 3.39466189
                - (30.84437 / 3600.0) * t
                - (11.67836 / 3600.0) * t2
                + (0.03338 / 3600.0) * t3
                + (0.00269 / 3600.0) * t4
                + (0.00004 / 3600.0) * t5;
            // Longitude of the ascending node [degrees]
            double Omega = 76.67992019
                - (10008.48154 / 3600.0) * t
                - (51.32614 / 3600.0) * t2
                - (0.58910 / 3600.0) * t3
                - (0.04665 / 3600.0) * t4;

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
            this.textures[0] = AssetManager.LoadTexture2D("venus_atmosphere_4k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}

