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
            this.name = "Луна";
            this.mass = 7.3477e22;
            this.radius = 1_737_100.0;

        }

        public override void LoadRenderData()
        {
            this.mesh = AssetManager.LoadMesh("sphere.obj");
            this.mesh.LoadRenderData();

            this.material = AssetManager.LoadProgram("phongNoSpecular");

            this.textures = new Texture[1];
            this.textures[0] = AssetManager.LoadTexture2D("moon_2k.jpg");
            this.textures[0].LoadRenderData(InternalFormat.Rgba, true);
        }
    }
}
