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

        // Physics
        protected Vector<double> _position;
        protected Vector<double> _velocity;
        protected Vector<double> _acceleration;
        protected Vector<double> _force;

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

        #endregion

        public BodyBase()
        {
            this._name = string.Empty;

            this._position = new Vector<double>(0.0);
            this._velocity = new Vector<double>(0.0);
            this._acceleration = new Vector<double>(0.0);
            this._force = new Vector<double>(0.0);
        }

        public void AddForce(Vector<double> force)
        {
            this._force += force;
        }

        
    }
}
