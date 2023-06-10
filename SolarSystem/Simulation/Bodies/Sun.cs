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
        public Sun() : base()
        {
            this._name = "Sun";
            this._mass = 1.98847e30;
            this._radius = 6.9551e8;
            this._distanceFromSun = 0.0;
        }

        public override void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseTexture");

            this._textures = new Texture[1];
            this._textures[0] = AssetManager.LoadTexture2D("sun_2k.jpg");
            this._textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
