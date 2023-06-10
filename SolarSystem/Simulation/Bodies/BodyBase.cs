using SolarSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal abstract class BodyBase
    {
        #region Fields

        protected string _name;
        protected double _mass;
        protected double _radius;
        protected double _distanceFromSun;

        // Physics
        protected Vector<double> _position;
        protected Vector<double> _velocity;
        protected Vector<double> _acceleration;
        protected Vector<double> _force;

        // Rendering
        protected Mesh _mesh;
        protected Rendering.Program _material;
        protected Texture[] _textures;

        #endregion

        #region Properties

        /// <summary>
        /// Displays name of the body.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Represents body mass in kg.
        /// </summary>
        public double Mass { get { return _mass; } }

        /// <summary>
        /// Represents the radius of the body in meters.
        /// </summary>
        public double Radius { get { return _radius; } }

        /// <summary>
        /// Represents the body's distance from the Sun in m.
        /// </summary>
        public double DistanceFromSun { get { return _distanceFromSun; } }

        /// <summary>
        /// Represents body position in world space.
        /// </summary>
        public Vector<double> Position { get { return _position; } set { _position = value; } }

        /// <summary>
        /// Represents body velocity in m/s.
        /// </summary>
        public Vector<double> Velocity { get { return _velocity; } set { _velocity = value; } }

        /// <summary>
        /// Represents body acceleration in m/s^2.
        /// </summary>
        public Vector<double> Acceleration { get { return _acceleration; } set { _acceleration = value; } }

        /// <summary>
        /// Represents the force applied to the body in N.
        /// </summary>
        public Vector<double> Force { get { return _force; } set { _force = value; } }


        public Mesh Mesh { get { return _mesh; } }

        public Rendering.Program Material { get { return _material; } }

        public Texture[] Textures { get { return _textures; } }

        #endregion

        public BodyBase()
        {
            this._name = string.Empty;

            this._position = new Vector<double>(0.0);
            this._velocity = new Vector<double>(0.0);
            this._acceleration = new Vector<double>(0.0);
            this._force = new Vector<double>(0.0);

            this._textures = new Texture[0];
        }

        public void AddForce(Vector<double> force)
        {
            this._force += force;
        }

        public virtual void LoadRenderData()
        {
            this._mesh = AssetManager.LoadMesh("sphere.obj");
            this._mesh.LoadRenderData();

            this._material = AssetManager.LoadProgram("baseColorAttrib");
            this._material.Link();
        }

    }
}
