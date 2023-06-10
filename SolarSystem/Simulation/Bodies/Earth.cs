using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using SolarSystem.Rendering;

namespace SolarSystem.Simulation.Bodies
{
    internal class Earth : BodyBase
    {

        public Earth() : base()
        {
            this._name = "Earth";
            this._mass = 5.9722e24;
            this._radius = 6_371_000.0;
            this._distanceFromSun = Math.Math.AU;
        }

        public override void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseTexture");

            this._textures = new Texture[1];
            this._textures[0] = AssetManager.LoadTexture2D("earth_day_2k.jpg");
            this._textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
