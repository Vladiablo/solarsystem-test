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
    internal class Jupiter : BodyBase
    {

        public Jupiter() : base()
        {
            this._name = "Jupiter";
            this._mass = 1.8986e27;
            this._radius = 69_911_000.0;
            this._distanceFromSun = MathHelper.AU * 5.2;
        }

        public override void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseTexture");

            this._textures = new Texture[1];
            this._textures[0] = AssetManager.LoadTexture2D("jupiter_2k.jpg");
            this._textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
