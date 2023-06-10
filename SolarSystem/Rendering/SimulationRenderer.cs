using SolarSystem.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using SolarSystem.Simulation.Bodies;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace SolarSystem.Rendering
{
    internal class SimulationSystemRenderer
    {
        private SimulationSystem _simulation;
        private IReadOnlyList<BodyBase> _bodies;

        private Camera _camera;

        public Camera Camera 
        { 
            get { return _camera; } 
            set { _camera = value; }
        }

        public SimulationSystemRenderer(SimulationSystem simulation)
        {
            this._simulation = simulation;
            this._bodies = simulation.Bodies;

            for (int i = 0; i < this._bodies.Count; ++i)
            {
                this._bodies[i].LoadRenderData();
            }

            this._camera = new Camera(90.0f, 16.0f / 9.0f, 0.001f, 1.0e12f);

            this._simulation.TimeScale = 31_557_600.0 / 16.0;
            //this._simulation.SolverIterations = 8;
        }

        public void Render(double deltaTime)
        {

            for (int i = 0; i < this._bodies.Count; ++i)
            {
                Matrix4x4 translation = Matrix4x4.CreateTranslation(
                    Math.Math.ConvertDoubleVectorToFloat(this._bodies[i].Position * (1.0 / Math.Math.AU))
                    .AsVector128()
                    .AsVector3());

                Matrix4x4 scale = Matrix4x4.CreateScale((float)(this._bodies[i].Radius * Math.Math.RENDER_TO_SIMULATION_SCALE * 1e-3));

                Matrix4x4 mvp = scale * translation;

                this._bodies[i].Material.Use();
                this._bodies[i].Material.SetMatrix("mvp", mvp);

                IReadOnlyList<string> samplers = this._bodies[i].Material.Samplers;
                int textureCount = System.Math.Min(this._bodies[i].Textures.Length, samplers.Count);
                for(int tex = 0; tex < textureCount; ++tex)
                {
                    this._bodies[i].Textures[tex].Bind((TextureUnit)(Gl.TEXTURE0 + tex));
                    this._bodies[i].Material.SetInteger(samplers[tex], tex);
                }

                this._bodies[i].Mesh.Vao.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, this._bodies[i].Mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            }
       
        }

    }
}
