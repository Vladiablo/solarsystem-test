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
using SolarSystem.Math;
using GLFW;

namespace SolarSystem.Rendering
{
    internal class SimulationSystemRenderer
    {
        private SimulationSystem simulation;
        private IReadOnlyList<BodyBase> bodies;

        private float bodiesScale;
        private float sunScale;

        private Camera camera;

        public float SunScale
        {
            get { return sunScale; }
            set
            {
                if (value < 0.001f) 
                    throw new ArgumentOutOfRangeException("Sun scale cannot be less that 0.001");
                sunScale = value;
            } 
        }

        public float BodiesScale
        {
            get { return bodiesScale; }
            set
            {
                if (value < 0.001f) 
                    throw new ArgumentOutOfRangeException("Bodies scale cannot be less that 0.001");
                bodiesScale = value;
            }
        }

        public Camera Camera 
        { 
            get { return camera; } 
            set { camera = value; }
        }

        public SimulationSystemRenderer(SimulationSystem simulation, NativeWindow window)
        {
            this.simulation = simulation;
            this.bodies = simulation.Bodies;

            this.sunScale = 1.0f;
            this.bodiesScale = 10.0f;

            for (int i = 0; i < this.bodies.Count; ++i)
            {
                this.bodies[i].LoadRenderData();
            }

            this.camera = new Camera(15.0f, 16.0f / 9.0f, 0.1f, 1.0e12f);

            window.SizeChanged += WindowSizeChangedCallback;
        }

        private void WindowSizeChangedCallback(object? sender, SizeChangeEventArgs e)
        {
            this.camera.AspectRatio = (float)e.Size.Width / (float)e.Size.Height;
        }

        public void Render(double deltaTime)
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);

            Matrix4x4 proj = this.camera.Projection;

            //this._camera.Position = MathHelper.ConvertDoubleVectorToFloat((this._bodies[0].Position * MathHelper.RENDER_TO_SIMULATION_SCALE)).AsVector128().AsVector3();

            Matrix4x4 view = this.Camera.GetViewMatrix();
            this.bodies[0].Position = new Vector<double>(0.0);
            for (int i = 0; i < this.bodies.Count; ++i)
            {
                Matrix4x4 translation = Matrix4x4.CreateTranslation(
                    MathHelper.ConvertDoubleVectorToFloat(this.bodies[i].Position * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3());

                Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(0.0f, 0.0f, 0.0f);
                Matrix4x4 scale = Matrix4x4.CreateScale(
                    (float)(this.bodies[i].Radius * MathHelper.RENDER_TO_SIMULATION_SCALE) *
                    (i == 0 ? this.sunScale : this.bodiesScale));

                Matrix4x4 model = scale * rotation * translation;
                Matrix4x4 mvp = model * view * proj;

                this.bodies[i].Material.Use();
                this.bodies[i].Material.SetMatrix("mvp", mvp);

                IReadOnlyList<string> samplers = this.bodies[i].Material.Samplers;
                int textureCount = System.Math.Min(this.bodies[i].Textures.Length, samplers.Count);
                for(int tex = 0; tex < textureCount; ++tex)
                {
                    this.bodies[i].Textures[tex].Bind((TextureUnit)(Gl.TEXTURE0 + tex));
                    this.bodies[i].Material.SetInteger(samplers[tex], tex);
                }

                this.bodies[i].Mesh.Vao.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, this.bodies[i].Mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
                this.bodies[i].Mesh.Vao.Unbind();
            }
       
        }

    }
}
