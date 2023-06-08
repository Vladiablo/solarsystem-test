using SolarSystem.Simulation.Bodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation
{
    internal class SimulationSystem
    {
        #region Fields

        public const double TIME_SCALE_MIN = 1.0e-6;
        public const double TIME_SCALE_MAX = 1.0e+6;

        private DateTime _simulationTime;
        private double _timeScale;

        private ushort _solverIterations;
        private List<BodyBase> _bodies;

        #endregion

        #region Properties

        /// <summary>
        /// Represents current simulation time.
        /// </summary>
        public DateTime SimulationTime { get { return _simulationTime; } set { _simulationTime = value; } }

        /// <summary>
        /// Specifies the simulation time scale.
        /// </summary>
        public double TimeScale 
        {
            get { return _timeScale; }
            set
            {
                if (value < TIME_SCALE_MIN || value > TIME_SCALE_MAX)
                    throw new ArgumentOutOfRangeException($"TimeScale value is out range! " +
                        $"Valid value should be in range [{TIME_SCALE_MIN}; {TIME_SCALE_MAX}]");
                _timeScale = value;
            } 
        }

        /// <summary>
        /// Specifies the number of iterations in the simulation loop.<br/>
        /// Higher values result in more accurate results, but take more time to simulate.
        /// </summary>
        public ushort SolverIterations 
        {
            get { return _solverIterations; } 
            set 
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("SolverIterations value is out of range! Value should be >= 1");
                _solverIterations = value;
            } 
        }

        /// <summary>
        /// Returns current list of bodies in the simulation;
        /// </summary>
        public IList<BodyBase> Bodies { get { return _bodies.AsReadOnly(); } }

        #endregion

        public SimulationSystem() 
        {
            this._simulationTime = DateTime.Now;
            this._timeScale = 1.0;
            this._solverIterations = 1;
            this._bodies = new List<BodyBase>();
        }

        #region Private Methods

        private void UpdatePositions(double deltaTime)
        {
            for (int i = 0; i < this._bodies.Count; ++i)
            {
                Vector<double> newPosition = 
                    this._bodies[i].Position +
                    this._bodies[i].Velocity * deltaTime +
                    this._bodies[i].Acceleration * (0.5 * deltaTime * deltaTime);

                this._bodies[i].Position = newPosition;
            }
        }

        private void CalculateForces()
        {
            for (int i = 0; i < this._bodies.Count; ++i)
                this._bodies[i].Force = new Vector<double>(0.0);

            for (int i = 0; i <= this._bodies.Count; ++i)
            {
                for (int j = i + 1; j < this._bodies.Count; ++j)
                {
                    Vector<double> distance = this._bodies[j].Position - this._bodies[i].Position;
                    Vector<double> forceDirection = Math.Math.NormalizeVector(distance);
                    double forceMagnitude = Math.Math.G * this._bodies[i].Mass * this._bodies[j].Mass / Vector.Dot(distance, distance);

                    this._bodies[i].AddForce(forceDirection *  forceMagnitude);
                    this._bodies[j].AddForce(forceDirection * -forceMagnitude);
                }
            }
        }

        private void UpdateVelocities(double deltaTime)
        {
            for (int i = 0; i < this._bodies.Count; ++i)
            {
                Vector<double> newAcceleration = this._bodies[i].Force / new Vector<double>(this._bodies[i].Mass);
                Vector<double> newVelocity = this._bodies[i].Velocity + (this._bodies[i].Acceleration + newAcceleration) * (0.5 * deltaTime);

                this._bodies[i].Acceleration = newAcceleration;
                this._bodies[i].Velocity = newVelocity;
            }
        }

        #endregion

        public void Update(double deltaTime)
        {
            deltaTime *= this._timeScale;
            double dt = deltaTime / (double)this._solverIterations;
            for (ushort i = 0; i < this._solverIterations; ++i)
            {
                this.UpdatePositions(dt);
                this.CalculateForces();
                this.UpdateVelocities(dt);
            }

            this._simulationTime = this._simulationTime.AddSeconds(deltaTime);
        }

        public void AddBody(BodyBase body)
        {
            this._bodies.Add(body);
        }

        public void AddBody(BodyBase body, Vector<double> position)
        {
            body.Position = position;
            this._bodies.Add(body);
        }
    }
}
