using OpenGL;
using SolarSystem.Math;
using SolarSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal abstract class BodyBase
    {
        #region Fields

        protected string name;
        protected double mass;
        protected double equatorialRadius;
        protected double polarRadius;
        protected double radius;
        protected double flattening;
        protected double surfaceArea;
        protected double volume;
        protected double density;
        protected double surfaceAcceleration;
        protected double escapeVelocity;
        protected double equatorialVelocity;

        protected OrbitData orbitData;
        protected OrbitData currentOrbitData;

        protected string description;

        // Physics
        protected Vector<double> position;
        protected Vector<double> velocity;
        protected Vector<double> acceleration;
        protected Vector<double> force;

        // Rendering
        protected Mesh mesh;
        protected Rendering.Program material;
        protected Texture[] textures;
        protected float renderScale;

        protected int orbitSamples;
        protected Rendering.Buffer orbitVbo;
        protected VertexArray orbitVao;

        protected Mesh ringMesh;
        protected Rendering.Program ringMaterial;
        protected Texture[] ringTextures;

        #endregion

        #region Properties

        /// <summary>
        /// Displays name of the body.
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// Represents body mass in kg.
        /// </summary>
        public double Mass { get { return mass; } }
        public double EquatorialRadius { get { return equatorialRadius; } }
        public double PolarRadius { get { return polarRadius; } }
        /// <summary>
        /// Represents the average radius of the body in m.
        /// </summary>
        public double Radius { get { return radius; } }
        public double Flattening { get { return flattening; } }
        public double SurfaceArea { get { return surfaceArea; } }
        public double Volume { get { return volume; } }
        public double Density { get { return density; } }
        public double SurfaceAcceleration { get { return surfaceAcceleration; } }
        public double EscapeVelocity { get { return escapeVelocity; } }
        public double EquatorialVelocity { get { return equatorialVelocity; } }

        /// <summary>
        /// Represents the body's average orbital speed in m/s.
        /// </summary>
        public double OrbitalSpeed { get { return this.orbitData.meanVelocity; } }

        /// <summary>
        /// Represents the keplerian elements of the body's orbit at the J2000.0 epoch.
        /// </summary>
        public OrbitData OrbitData { get { return orbitData; } }

        /// <summary>
        /// Represents the keplerian elements of the body's orbit at the moment of last calculation.
        /// </summary>
        public OrbitData CurrentOrbitData
        {
            get { return currentOrbitData; }
        }

        public string Description { get { return description; } }

        /// <summary>
        /// Represents body position in world space.
        /// </summary>
        public Vector<double> Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Represents body velocity in m/s.
        /// </summary>
        public Vector<double> Velocity { get { return velocity; } set { velocity = value; } }

        /// <summary>
        /// Represents body acceleration in m/s^2.
        /// </summary>
        public Vector<double> Acceleration { get { return acceleration; } set { acceleration = value; } }

        /// <summary>
        /// Represents the force applied to the body in N.
        /// </summary>
        public Vector<double> Force { get { return force; } set { force = value; } }


        public Mesh Mesh { get { return mesh; } }

        public Rendering.Program Material { get { return material; } }

        public Texture[] Textures { get { return textures; } }

        public int OrbitSamples { get { return orbitSamples; } }

        public VertexArray OrbitVao { get { return orbitVao; } }

        public Mesh RingMesh { get { return ringMesh; } }

        public Rendering.Program RingMaterial { get { return ringMaterial; } }

        public Texture[] RingTextures { get { return ringTextures; } }

        public float RenderScale 
        { 
            get { return renderScale; }
            set 
            {
                if (value < 0.001f) 
                    throw new ArgumentOutOfRangeException("Render Scale cannot be less than 0.001");
                renderScale = value; 
            } 
        }

        #endregion

        public BodyBase()
        {
            this.name = string.Empty;
            this.description = string.Empty;

            this.position = new Vector<double>(0.0);
            this.velocity = new Vector<double>(0.0);
            this.acceleration = new Vector<double>(0.0);
            this.force = new Vector<double>(0.0);

            this.textures = new Texture[0];
            this.renderScale = 1.0f;
            this.orbitSamples = 0;
        }

        ~BodyBase() 
        {
            if (this.orbitVbo != null)
            {
                this.orbitVao.Delete();
                this.orbitVbo.Delete();
            }
        }

        protected void CreateOrbitBuffer()
        {
            this.orbitVao = new VertexArray();
            this.orbitVao.Bind();

            this.orbitVbo = new Rendering.Buffer(BufferTarget.ArrayBuffer, BufferUsage.StreamDraw);
            this.orbitVbo.Alocate((uint)(this.orbitSamples * Marshal.SizeOf<Vector3>()));
            this.orbitVao.BindAttribute(0, 3, VertexAttribType.Float, 0, 0);

            this.orbitVao.Unbind();
            this.orbitVbo.Unbind();
        }

        protected void LoadOrbitVertices(Vector3[] vertices)
        {
            this.orbitVbo.UpdateData(0, vertices);
            this.orbitVbo.Unbind();
        }

        public void AddForce(Vector<double> force)
        {
            this.force += force;
        }

        protected static double SolveEccentricAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1.0e-6, int maxIterations = 8)
        {
            double eDegrees = MathHelper.RAD_TO_DEG64 * eccentricity;
            double E0 = meanAnomaly + eDegrees * System.Math.Sin(MathHelper.DegToRad(meanAnomaly));
            double En = E0;

            for (int n = 0; n < maxIterations; ++n)
            {
                double dM = meanAnomaly - (En - eDegrees * System.Math.Sin(MathHelper.DegToRad(En)));
                double dE = dM / (1.0 - eccentricity * System.Math.Cos(MathHelper.DegToRad(En)));
                En += dE;

                if (dE <= tolerance)
                    break;
            }

            return En;
        }

        public static Vector<double> CalculatePositionFromOrbitData(in OrbitData orbitData)
        {
            double E = SolveEccentricAnomaly(orbitData.meanAnomaly, orbitData.eccentricity);
            double Erad = MathHelper.DegToRad(E);
            double omegaRad = MathHelper.DegToRad(orbitData.periapsisArgument);
            double OmegaRad = MathHelper.DegToRad(orbitData.ascendingNodeLongitude);
            double iRad = MathHelper.DegToRad(orbitData.orbitInclination);

            double sinPerihelionArg = System.Math.Sin(omegaRad);
            double cosPerihelionArg = System.Math.Cos(omegaRad);
            double sinAscNodeLongitude = System.Math.Sin(OmegaRad);
            double cosAscNodeLongitude = System.Math.Cos(OmegaRad);
            double sinInclination = System.Math.Sin(iRad);
            double cosInclination = System.Math.Cos(iRad);

            double x = orbitData.semiMajorAxis * (System.Math.Cos(Erad) - orbitData.eccentricity);
            double y = orbitData.semiMajorAxis * System.Math.Sqrt(1.0 - orbitData.eccentricity * orbitData.eccentricity) * System.Math.Sin(Erad);

            double xEcl = (cosPerihelionArg * cosAscNodeLongitude - sinPerihelionArg * sinAscNodeLongitude * cosInclination) * x
                + (-sinPerihelionArg * cosAscNodeLongitude - cosPerihelionArg * sinAscNodeLongitude * cosInclination) * y;

            double yEcl = (cosPerihelionArg * sinAscNodeLongitude + sinPerihelionArg * cosAscNodeLongitude * cosInclination) * x
                + (-sinPerihelionArg * sinAscNodeLongitude + cosPerihelionArg * cosAscNodeLongitude * cosInclination) * y;

            double zEcl = (sinPerihelionArg * sinInclination) * x
                + (cosPerihelionArg * sinInclination) * y;

            return new Vector<double>(new double[] { xEcl, yEcl, zEcl, 1.0 });
        }

        public static StateVector CalculateStateVectorFromOrbitData(in OrbitData orbitData)
        {
            double E = SolveEccentricAnomaly(orbitData.meanAnomaly, orbitData.eccentricity);
            double Erad = MathHelper.DegToRad(E);
            double omegaRad = MathHelper.DegToRad(orbitData.periapsisArgument);
            double OmegaRad = MathHelper.DegToRad(orbitData.ascendingNodeLongitude);
            double iRad = MathHelper.DegToRad(orbitData.orbitInclination);

            //double trueAnomaly = 2.0 * System.Math.Atan(System.Math.Tan(Erad / 2.0)
            //    * System.Math.Sqrt((1.0 + orbitData.eccentricity) / (1.0 - orbitData.eccentricity)))
            //    % (MathHelper.PI * 2.0);

            double sinPerihelionArg = System.Math.Sin(omegaRad);
            double cosPerihelionArg = System.Math.Cos(omegaRad);
            double sinAscNodeLongitude = System.Math.Sin(OmegaRad);
            double cosAscNodeLongitude = System.Math.Cos(OmegaRad);
            double sinInclination = System.Math.Sin(iRad);
            double cosInclination = System.Math.Cos(iRad);

            double x = orbitData.semiMajorAxis * (System.Math.Cos(Erad) - orbitData.eccentricity);
            double y = orbitData.semiMajorAxis * System.Math.Sqrt(1.0 - orbitData.eccentricity * orbitData.eccentricity) * System.Math.Sin(Erad);

            double xEcl = (cosPerihelionArg * cosAscNodeLongitude - sinPerihelionArg * sinAscNodeLongitude * cosInclination) * x
                + (-sinPerihelionArg * cosAscNodeLongitude - cosPerihelionArg * sinAscNodeLongitude * cosInclination) * y;

            double yEcl = (cosPerihelionArg * sinAscNodeLongitude + sinPerihelionArg * cosAscNodeLongitude * cosInclination) * x
                + (-sinPerihelionArg * sinAscNodeLongitude + cosPerihelionArg * cosAscNodeLongitude * cosInclination) * y;

            double zEcl = (sinPerihelionArg * sinInclination) * x
                + (cosPerihelionArg * sinInclination) * y;

            // Distance to the central body
            double r = orbitData.semiMajorAxis * (1.0 - orbitData.eccentricity * System.Math.Cos(Erad));

            if (System.Math.Abs(r) < MathHelper.NEAR_ZERO)
            {
                return new StateVector
                {
                    position = new Vector<double>(new double[] { xEcl, yEcl, zEcl, 1.0 }),
                    velocity = new Vector<double>(new double[] { 0.0, 0.0, 0.0, 0.0 })
                };
            }

            // Gravitational parameter for The Sun (central body)
            double u = MathHelper.G * 1.9885e30;
            double vel = System.Math.Sqrt(u * orbitData.semiMajorAxis) / r;

            double velX = vel * -System.Math.Sin(Erad);
            double velY = vel * System.Math.Sqrt(1.0 - orbitData.eccentricity * orbitData.eccentricity) * System.Math.Cos(Erad);

            double velXecl = (cosPerihelionArg * cosAscNodeLongitude - sinPerihelionArg * sinAscNodeLongitude * cosInclination) * velX
                + (-sinPerihelionArg * cosAscNodeLongitude - cosPerihelionArg * sinAscNodeLongitude * cosInclination) * velY;

            double velYecl = (cosPerihelionArg * sinAscNodeLongitude + sinPerihelionArg * cosAscNodeLongitude * cosInclination) * velX
                + (-sinPerihelionArg * sinAscNodeLongitude + cosPerihelionArg * cosAscNodeLongitude * cosInclination) * velY;

            double velZecl = (sinPerihelionArg * sinInclination) * velX
                + (cosPerihelionArg * sinInclination) * velY;

            return new StateVector
            {
                position = new Vector<double>(new double[] { xEcl, yEcl, zEcl, 1.0 }),
                velocity = new Vector<double>(new double[] { velXecl, velYecl, velZecl, 0.0})
            };
        }

        public static Vector3[] CalculateOrbitVertices(in OrbitData orbitData, int orbitSegments)
        {         
            double omegaRad = MathHelper.DegToRad(orbitData.periapsisArgument);
            double OmegaRad = MathHelper.DegToRad(orbitData.ascendingNodeLongitude);
            double iRad = MathHelper.DegToRad(orbitData.orbitInclination);

            double sinPerihelionArg = System.Math.Sin(omegaRad);
            double cosPerihelionArg = System.Math.Cos(omegaRad);
            double sinAscNodeLongitude = System.Math.Sin(OmegaRad);
            double cosAscNodeLongitude = System.Math.Cos(OmegaRad);
            double sinInclination = System.Math.Sin(iRad);
            double cosInclination = System.Math.Cos(iRad);

            Vector3[] vertices = new Vector3[orbitSegments];

            double delta = 360.0 / (double)orbitSegments;
            double a = 0.0;
            for (int i = 0; i < orbitSegments; ++i)
            {

                double E = a;
                double Erad = MathHelper.DegToRad(E);

                double x = orbitData.semiMajorAxis * (System.Math.Cos(Erad) - orbitData.eccentricity);
                double y = orbitData.semiMajorAxis * System.Math.Sqrt(1.0 - orbitData.eccentricity * orbitData.eccentricity) * System.Math.Sin(Erad);

                double xEcl = ((cosPerihelionArg * cosAscNodeLongitude - sinPerihelionArg * sinAscNodeLongitude * cosInclination) * x
                    + (-sinPerihelionArg * cosAscNodeLongitude - cosPerihelionArg * sinAscNodeLongitude * cosInclination) * y) * MathHelper.RENDER_TO_SIMULATION_SCALE;

                double yEcl = ((cosPerihelionArg * sinAscNodeLongitude + sinPerihelionArg * cosAscNodeLongitude * cosInclination) * x
                    + (-sinPerihelionArg * sinAscNodeLongitude + cosPerihelionArg * cosAscNodeLongitude * cosInclination) * y) * MathHelper.RENDER_TO_SIMULATION_SCALE;

                double zEcl = ((sinPerihelionArg * sinInclination) * x
                    + (cosPerihelionArg * sinInclination) * y) * MathHelper.RENDER_TO_SIMULATION_SCALE;

                vertices[i] = new Vector3((float)xEcl, (float)yEcl, (float)zEcl);
                a += delta;
            }

            return vertices;
        }

        public Vector<double> CalculatePositionAtJD(double timeJD)
        {
            return CalculatePositionFromOrbitData(this.CalculateOrbitData(timeJD));
        }

        public StateVector CalculateStateVectorAtJD(double timeJD)
        {
            return CalculateStateVectorFromOrbitData(this.CalculateOrbitData(timeJD));
        }

        public virtual OrbitData CalculateOrbitData(double timeJD)
        {
            this.currentOrbitData = this.orbitData;

            return this.currentOrbitData;
        }

        public virtual void LoadRenderData()
        {
            this.mesh = AssetManager.LoadMesh("sphere.obj");
            this.mesh.LoadRenderData();

            this.material = AssetManager.LoadProgram("baseColorAttrib");
            this.material.Link();
        }

    }
}
