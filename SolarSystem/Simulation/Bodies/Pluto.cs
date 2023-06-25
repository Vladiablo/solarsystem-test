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
    internal class Pluto : BodyBase
    {
        private const string PLUTO_DESCRIPTION =
            "Плутон - крупнейшая известная карликовая планета Солнечной системы, транснептуновый объект и десятое по массе (без учёта спутников) небесное тело, " +
            "обращающееся вокруг Солнца - после восьми планет и Эриды. " +
            "Первоначально Плутон считали девятой классической планетой, но с 2006 года он считается карликовой планетой и крупнейшим объектом пояса Койпера. " +
            "Как и большинство тел пояса Койпера, Плутон состоит в основном из камня и льда, и он относительно мал: его масса меньше массы Луны примерно в шесть раз, " +
            "а объём - примерно в три раза. Площадь Плутона (17,7 млн км²) немного больше площади России (17,1 млн км²). " +
            "У орбиты Плутона большой эксцентриситет и большой наклон к плоскости эклиптики.";

        public Pluto() : base()
        {
            this.name = "Плутон";
            this.mass = 1.303e22;
            this.equatorialRadius = 1_188_000.0;
            this.polarRadius = 1_188_000.0;
            this.radius = 1_188_000.0;
            this.flattening = 0.0;
            this.surfaceArea = 17.7e6;
            this.volume = 7.0e9;
            this.density = 1.860;
            this.surfaceAcceleration = 0.617;
            this.escapeVelocity = 1_210.0;
            this.equatorialVelocity = 47.18 / 3.6;

            this.orbitData = new OrbitData()
            {
                semiMajorAxis = 5_869.656e9,
                siderialOrbitPeriod = 90_560.0,
                perihelion = 4_434.987e9,
                aphelion = 7_304.326e9,
                meanVelocity = 4_640.0,
                maxVelocity = 6_100.0,
                minVelocity = 3_710.0,
                orbitInclination = 17.160,
                eccentricity = 0.2444,
                siderialRotationPeriod = 153.292,
                equatorInclination = 122.53,

                ascendingNodeLongitude = 110.30347,
                periapsisArgument = 113.76329,
                meanAnomaly = 14.53
            };

            this.description = PLUTO_DESCRIPTION;

            this.orbitSamples = 1440;
            this.CreateOrbitBuffer();
        }

        public override OrbitData CalculateOrbitData(double timeDJ)
        {
            // Barycentric time (TDB) in centuries of Julian years since J2000.0
            double T = (timeDJ - MathHelper.J2000) / 36525.0;
            // Semi-major axis [m]
            double a = MathHelper.AU * (39.48686035 + 0.00449751 * T);
            // Mean longitude [degrees] (lambda, L0) = omega (longitude of the periapsis) + Omega (longitude of ascending node) + M0 (mean anomaly)
            double l = MathHelper.NormalizeRotation(238.96535011 + 145.18042903 * T);
            // Eccentricity
            double e = 0.24885238 + 0.00006016 * T;
            // Longitude of the periapsis [degrees]
            double omega = 224.09702598 - 0.00968827 * T;
            // Inclination [degrees]
            double i = 17.14104260 + 0.00000501 * T;
            // Longitude of the ascending node [degrees]
            double Omega = 110.30167986 - 0.00809981 * T;

            double b = -0.01262724;

            this.currentOrbitData = new OrbitData
            {
                semiMajorAxis = a,
                eccentricity = e,
                periapsisArgument = omega - Omega,
                orbitInclination = i,
                ascendingNodeLongitude = Omega,
                meanAnomaly = MathHelper.NormalizeRotation(l - omega + b * T * T)
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
            this.textures[0] = AssetManager.LoadTexture2D("pluto.png");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
