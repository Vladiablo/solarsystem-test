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
    internal class Mars : BodyBase
    {

        public Mars() : base()
        {
            this._name = "Mars";
            this._mass = 6.4171e23;
            this._radius = 3_389_500.0;
            this._distanceFromSun = MathHelper.AU * 1.52;
        }

        public override void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseTexture");

            this._textures = new Texture[1];
            this._textures[0] = AssetManager.LoadTexture2D("mars_2k.jpg");
            this._textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
