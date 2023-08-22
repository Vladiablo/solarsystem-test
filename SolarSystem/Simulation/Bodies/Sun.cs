using SolarSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Simulation.Bodies
{
    internal class Sun : BodyBase
    {
        private const string SUN_DESCRIPTION =
            "Солнце - одна из звёзд нашей Галактики (Млечный Путь) и единственная звезда Солнечной системы. " +
            "Вокруг Солнца обращаются другие объекты этой системы: планеты и их спутники, карликовые планеты и их спутники, астероиды, метеороиды, кометы и космическая пыль. " +
            "По спектральной классификации Солнце относится к типу G2V (жёлтый карлик). Средняя плотность Солнца составляет 1,4 г/см³ (в 1,4 раза больше, чем у воды). " +
            "Эффективная температура поверхности Солнца - 5780 кельвин. Поэтому Солнце светит почти белым светом, " +
            "но прямой свет Солнца у поверхности нашей планеты приобретает некоторый жёлтый оттенок из-за более сильного рассеяния и поглощения коротковолновой части спектра атмосферой Земли " +
            "(при ясном небе, вместе с голубым рассеянным светом от неба, солнечный свет вновь даёт белое освещение).";

        public Sun() : base()
        {
            this.name = "Солнце";
            this.mass = 1.9885e30;
            this.equatorialRadius = 6.9551e8;
            this.polarRadius = this.equatorialRadius - (this.equatorialRadius * 9.0e-6);
            this.radius = 1.392e9 / 2.0;
            this.flattening = 9.0e-6;
            this.surfaceArea = 6.07877e12;
            this.volume = 1.40927e18;
            this.density = 1.409;
            this.surfaceAcceleration = 274.0;
            this.escapeVelocity = 617_700.0;
            this.equatorialVelocity = 7284.0 / 3.6;

            this.orbitData = new OrbitData
            { 
                equatorInclination = 7.25,
                siderialRotationPeriod = 25.38
            };

            this.description = SUN_DESCRIPTION;
        }

        public override void LoadRenderData()
        {
            this.mesh = AssetManager.LoadMesh("sphere.obj");
            this.mesh.LoadRenderData();

            this.material = AssetManager.LoadProgram("baseTexture");

            this.textures = new Texture[1];
            this.textures[0] = AssetManager.LoadTexture2D("sun_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true, TextureMagFilter.Linear, TextureMinFilter.Linear);
        }
    }
}
