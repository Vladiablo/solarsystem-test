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
using System.Reflection;

namespace SolarSystem.Rendering
{
    internal class SimulationSystemRenderer
    {
        private static readonly float[] scaleOverrides = 
        { 
            15.0f, // Sun
            800.0f, 800.0f, 800.0f, 800.0f, // Terrestrial planets
            200.0f, 200.0f, 200.0f, 200.0f, // Gas giants
            3000.0f // Pluto
        };

        private SimulationSystem simulation;
        private IReadOnlyList<BodyBase> bodies;
        private BodyBase targetBody;

        private float bodiesScale;
        private float sunScale;
        private bool realScale;
        private bool drawOrbits;

        private Buffer skyboxVbo;
        private VertexArray skyboxVao;
        private Texture skyboxTex;
        private Rendering.Program skyboxProg;
        private Rendering.Program orbitProg;

        private float prevFov;
        private Camera camera;

        public float SunScale
        {
            get { return sunScale; }
            set
            {
                if (value < 0.001f) 
                    throw new ArgumentOutOfRangeException("Sun scale cannot be less that 0.001");
                this.sunScale = value;
                this.bodies[0].RenderScale = value;
            } 
        }

        public float BodiesScale
        {
            get { return bodiesScale; }
            set
            {
                if (value < 0.001f) 
                    throw new ArgumentOutOfRangeException("Bodies scale cannot be less that 0.001");
                this.bodiesScale = value;
                for (int i = 1; i < this.bodies.Count; ++i)
                {
                    this.bodies[i].RenderScale = value;
                }
            }
        }

        public bool RealScale
        {
            get { return realScale; }
            set 
            { 
                realScale = value;

                if (value)
                {
                    this.bodies[0].RenderScale = this.sunScale;
                    for (int i = 1; i < this.bodies.Count; ++i)
                    {
                        this.bodies[i].RenderScale = this.bodiesScale;
                    }
                    return;
                }

                int count = System.Math.Min(this.bodies.Count, scaleOverrides.Length);
                for (int i = 0; i < count; ++i)
                {
                    this.bodies[i].RenderScale = scaleOverrides[i];
                }

            }
        }

        public bool DrawOrbits { get { return drawOrbits; } set { drawOrbits = value; } }

        public BodyBase TargetBody 
        { 
            get { return this.targetBody; }
            set 
            {
                bool prevBodySet = this.targetBody != null;
                this.targetBody = value;

                if (this.targetBody != null)
                {
                    if(!prevBodySet)
                        this.prevFov = this.camera.FovY;
                    this.camera.FovY = 65.0f;
                    return;
                }

                this.camera.FovY = this.prevFov;
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

            for (int i = 0; i < this.bodies.Count; ++i)
            {
                this.bodies[i].LoadRenderData();
            }

            this.SunScale = 1.0f;
            this.BodiesScale = 10.0f;
            this.drawOrbits = true;

            this.orbitProg = AssetManager.LoadProgram("baseColor");
            this.LoadSkybox();

            this.camera = new Camera(15.0f, 16.0f / 9.0f, 1.0e-2f, 1.0e12f);
            this.prevFov = 15.0f;

            window.SizeChanged += WindowSizeChangedCallback;
        }

        private void LoadSkybox()
        {
            float[] skyboxVertices = new float[] {      
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };

            this.skyboxTex = AssetManager.LoadTextureCubemap("skyboxTex", new string[] {
                "right.png", "left.png", "top.png", "bottom.png", "front.png", "back.png" });
            this.skyboxProg = AssetManager.LoadProgram("skybox");

            this.skyboxVao = new VertexArray();
            this.skyboxVao.Bind();

            this.skyboxVbo = new Buffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw);
            this.skyboxVbo.SetData(skyboxVertices);
            this.skyboxVao.BindAttribute(0, 3, VertexAttribType.Float, 0, 0);

            this.skyboxVao.Unbind();
            this.skyboxVbo.Unbind();
        }

        private void WindowSizeChangedCallback(object? sender, SizeChangeEventArgs e)
        {
            this.camera.AspectRatio = (float)e.Size.Width / (float)e.Size.Height;
        }

        private void RenderSkybox()
        {
            Matrix4x4 proj = this.camera.Projection;
            Matrix4x4 view = (this.targetBody == null)
                ? this.Camera.GetViewMatrix() 
                : this.camera.GetLookAtMatrix(
                    MathHelper.ConvertDoubleVectorToFloat(this.targetBody.Position * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3());

            Matrix4x4 skyboxView = new Matrix4x4(
                view.M11, view.M12, view.M13, 0.0f,
                view.M21, view.M22, view.M23, 0.0f,
                view.M31, view.M32, view.M33, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);

            Gl.DepthMask(false);
            this.skyboxProg.Use();
            this.skyboxTex.Bind();

            this.skyboxProg.SetInteger("skybox", 0);
            this.skyboxProg.SetMatrix4("view", skyboxView);
            this.skyboxProg.SetMatrix4("projection", proj);

            this.skyboxVao.Bind();
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            this.skyboxVao.Unbind();

            Gl.DepthMask(true);
        }

        static readonly string[] samplerNames = { "baseColor", "nightColor", "specularMap", "normalMap" };

        public void Render(double deltaTime)
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);

            if (this.targetBody != null)
            {
                Vector<double> bodyPos = this.targetBody.Position;
                Vector3 camPos = 
                    MathHelper.ConvertDoubleVectorToFloat(bodyPos * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3();
                float doubleRadius = (float)(this.targetBody.Radius * MathHelper.RENDER_TO_SIMULATION_SCALE * 2.0) * this.targetBody.RenderScale;
                camPos.X += doubleRadius;
                camPos.Y += doubleRadius;

                this.camera.Position = camPos;
            }

            RenderSkybox();

            Matrix4x4 proj = this.camera.Projection;
            Matrix4x4 view = (this.targetBody == null)
                ? this.Camera.GetViewMatrix()
                : this.camera.GetLookAtMatrix(
                    MathHelper.ConvertDoubleVectorToFloat(this.targetBody.Position * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3());

            Vector3 sunPos = MathHelper.ConvertDoubleVectorToFloat((this.bodies[0].Position * MathHelper.RENDER_TO_SIMULATION_SCALE))
                .AsVector128()
                .AsVector3();

            double julianHrs = MathHelper.ToJulianDate(this.simulation.SimulationTime) * 24.0;

            for (int i = 0; i < this.bodies.Count; ++i)
            {
                BodyBase body = this.bodies[i];

                Matrix4x4 translation = Matrix4x4.CreateTranslation(
                    MathHelper.ConvertDoubleVectorToFloat(body.Position * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3());

                double rotSpeed = 1.0;
                if (this.simulation.TimeScale > body.OrbitData.siderialRotationPeriod * 900.0)
                    rotSpeed = body.OrbitData.siderialRotationPeriod * 900.0 / this.simulation.TimeScale;

                float rot = MathHelper.TAU * (float)((julianHrs * rotSpeed % body.OrbitData.siderialRotationPeriod) / body.OrbitData.siderialRotationPeriod);
                Matrix4x4 rotation = 
                    Matrix4x4.CreateRotationZ(rot) *
                    Matrix4x4.CreateRotationX(-MathHelper.DegToRad((float)body.OrbitData.equatorInclination));

                Matrix4x4 scale = Matrix4x4.CreateScale(
                    (float)(body.Radius * MathHelper.RENDER_TO_SIMULATION_SCALE) * body.RenderScale);

                Matrix4x4 model = scale * rotation * translation;
                Matrix4x4 mvp = model * view * proj;

                if (this.drawOrbits && body.OrbitVao != null)
                {
                    this.orbitProg.Use();
                    this.orbitProg.SetMatrix4("mvp", view * proj);
                    this.orbitProg.SetVector4("color", new Vector4(1.0f));

                    body.OrbitVao.Bind();
                    Gl.DrawArrays(PrimitiveType.LineLoop, 0, body.OrbitSamples);
                    body.OrbitVao.Unbind();
                }

                Matrix3x3f normal = new Matrix3x3f(
                    model.M11, model.M12, model.M13,
                    model.M21, model.M22, model.M23,
                    model.M31, model.M32, model.M33);
                normal.Invert();
                normal.Transpose();

                body.Material.Use();
                body.Material.SetMatrix4("mvp", mvp);
                body.Material.TrySetMatrix4("model", model);
                body.Material.TrySetMatrix3("normalMat", normal);

                body.Material.TrySetVector3("lightPos", sunPos);
                body.Material.TrySetVector3("viewPos", this.camera.Position);
                body.Material.TrySetVector3("lightColor", new Vector3(1.0f));
                body.Material.TrySetFloat("shininess", 16.0f);

                IReadOnlyList<string> samplers = body.Material.Samplers;
                int textureCount = System.Math.Min(body.Textures.Length, samplers.Count);

                for(int tex = 0; tex < textureCount; ++tex)
                {
                    body.Textures[tex].Bind((TextureUnit)(Gl.TEXTURE0 + tex));
                    body.Material.SetInteger(samplerNames[tex], tex);
                }

                body.Mesh.Vao.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, body.Mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
                body.Mesh.Vao.Unbind();

                if (body.RingMesh != null)
                {
                    body.RingMaterial.Use();
                    body.RingMaterial.SetMatrix4("mvp", mvp);
                    body.RingMaterial.TrySetMatrix4("model", model);
                    body.RingMaterial.TrySetMatrix3("normalMat", normal);
                    body.RingMaterial.TrySetVector3("lightPos", sunPos);
                    body.RingMaterial.TrySetVector3("lightColor", new Vector3(1.0f));

                    IReadOnlyList<string> ringSamplers = body.RingMaterial.Samplers;
                    int ringTextureCount = System.Math.Min(body.RingTextures.Length, samplers.Count);
                    for (int tex = 0; tex < ringTextureCount; ++tex)
                    {
                        body.RingTextures[tex].Bind((TextureUnit)(Gl.TEXTURE0 + tex));
                        body.RingMaterial.SetInteger(ringSamplers[tex], tex);
                    }

                    Gl.Enable(EnableCap.Blend);
                    Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    Gl.Disable(EnableCap.CullFace);
                    body.RingMesh.Vao.Bind();

                    Gl.DrawElements(PrimitiveType.Triangles, body.RingMesh.IndicesCount, DrawElementsType.UnsignedInt, 0);

                    body.RingMesh.Vao.Unbind();
                    Gl.Disable(EnableCap.Blend);
                    Gl.Enable(EnableCap.CullFace);
                }
            }
       
        }

    }
}
