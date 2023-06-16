using OpenGL;
using SolarSystem.Rendering;
using SolarSystem.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal class Moon : BodyBase
    {
        public Moon() : base()
        {
            this._name = "Moon";
            this._mass = 7.3477e22;
            this._radius = 1_737_100.0;
            this._distanceFromSun = MathHelper.AU + 384_400_000.0;

        }

        public override void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseTexture");

            this._textures = new Texture[1];
            this._textures[0] = AssetManager.LoadTexture2D("moon_2k.jpg");
            this._textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
