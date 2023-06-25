using SolarSystem.Math;
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

        public const double TIME_SCALE_MIN = 1.0e-3;
        public const double TIME_SCALE_MAX = 1.0e+9;

        private DateTime simulationTime;
        private DateTime orbitCalculationDate;
        private double timeScale;
        private bool simulatePhysics;

        private ushort solverIterations;
        private List<BodyBase> bodies;

        #endregion

        #region Properties

        /// <summary>
        /// Represents current simulation time.
        /// </summary>
        public DateTime SimulationTime 
        { 
            get { return simulationTime; }
            set 
            { 
                simulationTime = value; 

                if (this.simulatePhysics) 
                {
                    this.RecalculatePositions();
                }
            } 
        }

        public DateTime OrbitCalculationDate { get { return orbitCalculationDate; } }


        /// <summary>
        /// Specifies the simulation time scale.
        /// </summary>
        public double TimeScale 
        {
            get { return timeScale; }
            set
            {
                if (value < TIME_SCALE_MIN || value > TIME_SCALE_MAX)
                    throw new ArgumentOutOfRangeException($"TimeScale value is out range! " +
                        $"Valid value should be in range [{TIME_SCALE_MIN}; {TIME_SCALE_MAX}]");
                timeScale = value;
            } 
        }

        /// <summary>
        /// Specifies the number of iterations in the simulation loop.<br/>
        /// Higher values result in more accurate results, but take more time to simulate.
        /// </summary>
        public ushort SolverIterations 
        {
            get { return solverIterations; } 
            set 
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("SolverIterations value is out of range! Value should be >= 1");
                solverIterations = value;
            } 
        }

        public bool SimulatePhysics 
        {
            get { return simulatePhysics; }
            set { simulatePhysics = value; }
        }

        /// <summary>
        /// Returns current list of bodies in the simulation;
        /// </summary>
        public IReadOnlyList<BodyBase> Bodies { get { return bodies.AsReadOnly(); } }

        #endregion

        public SimulationSystem() 
        {
            this.simulationTime = DateTime.Now;
            this.timeScale = 1.0;
            this.solverIterations = 8;
            this.simulatePhysics = true;
            this.bodies = new List<BodyBase>();
        }

        #region Private Methods

        private void UpdatePositions(double deltaTime)
        {
            for (int i = 1; i < this.bodies.Count; ++i)
            {
                Vector<double> newPosition = 
                    this.bodies[i].Position +
                    this.bodies[i].Velocity * deltaTime +
                    this.bodies[i].Acceleration * (0.5 * deltaTime * deltaTime);

                this.bodies[i].Position = newPosition;
            }
        }

        private void RecalculatePositions()
        {
            for (int i = 0; i < this.bodies.Count; ++i)
            {
                StateVector state = this.bodies[i].CalculateStateVectorAtJD(MathHelper.ToJulianDate(this.simulationTime));
                this.bodies[i].Position = state.position;
                if (this.bodies[i].Velocity != state.velocity)
                    this.bodies[i].Acceleration = this.bodies[i].Velocity - state.velocity;
                this.bodies[i].Velocity = state.velocity;
                
            }
            this.orbitCalculationDate = this.simulationTime;
        }

        private void CalculateForces()
        {
            for (int i = 0; i < this.bodies.Count; ++i)
                this.bodies[i].Force = new Vector<double>(0.0);

            for (int i = 0; i < this.bodies.Count; ++i)
            {
                for (int j = i + 1; j < this.bodies.Count; ++j)
                {
                    Vector<double> distance = this.bodies[j].Position - this.bodies[i].Position;
                    Vector<double> forceDirection = MathHelper.NormalizeVector(distance);

                    double distanceSquared = Vector.Dot(distance, distance);
                    if (distanceSquared < MathHelper.NEAR_ZERO)
                        distanceSquared = MathHelper.NEAR_ZERO;

                    double forceMagnitude = MathHelper.G * this.bodies[i].Mass * this.bodies[j].Mass / distanceSquared;

                    this.bodies[i].AddForce(forceDirection *  forceMagnitude);
                    this.bodies[j].AddForce(forceDirection * -forceMagnitude);
                }
            }
        }

        private void UpdateVelocities(double deltaTime)
        {
            for (int i = 0; i < this.bodies.Count; ++i)
            {
                Vector<double> newAcceleration = this.bodies[i].Force / new Vector<double>(this.bodies[i].Mass);
                Vector<double> newVelocity = this.bodies[i].Velocity + (this.bodies[i].Acceleration + newAcceleration) * (0.5 * deltaTime);

                this.bodies[i].Acceleration = newAcceleration;
                this.bodies[i].Velocity = newVelocity;
            }
        }

        #endregion

        public void Update(double deltaTime)
        {
            deltaTime *= this.timeScale;

            try
            {
                this.simulationTime = this.simulationTime.AddSeconds(deltaTime);
            }
            catch (Exception e)
            {
                this.simulationTime = new DateTime(0);
            }

            if (this.simulatePhysics)
            {
                double dt = deltaTime / (double)this.solverIterations;
                for (ushort i = 0; i < this.solverIterations; ++i)
                {
                    this.UpdatePositions(dt);
                    this.CalculateForces();
                    this.UpdateVelocities(dt);
                }
                return;
            }

            this.RecalculatePositions();
        }

        public void AddBody(BodyBase body)
        {
            this.bodies.Add(body);
        }

        public void AddBody(BodyBase body, Vector<double> position)
        {
            body.Position = position;
            this.bodies.Add(body);
        }

        public void AddBodyAtDefaultPosition(BodyBase body)
        {
            double pos = body.OrbitData.semiMajorAxis
                    * System.Math.Sqrt(1.0 - body.OrbitData.eccentricity * body.OrbitData.eccentricity);

            body.Position = new Vector<double>(new double[] { 0.0, -pos, 0.0, 0.0 });
            body.Velocity = new Vector<double>(new double[] { body.OrbitalSpeed, 0.0, 0.0, 0.0 });
            
            this.bodies.Add(body);
        }

        public void LoadSolarSystem()
        {
            this.bodies.Clear();

            Sun sun = new Sun();
            Mercury mercury = new Mercury();
            Venus venus = new Venus();
            Earth earth = new Earth();
            Mars mars = new Mars();
            Jupiter jupiter = new Jupiter();
            Saturn saturn = new Saturn();
            Uranus uranus = new Uranus();
            Neptune neptune = new Neptune();
            Pluto pluto = new Pluto();

            this.AddBody(sun);
            this.AddBody(mercury);
            this.AddBody(venus);
            this.AddBody(earth);
            this.AddBody(mars);
            this.AddBody(jupiter);
            this.AddBody(saturn);
            this.AddBody(uranus);
            this.AddBody(neptune);
            this.AddBody(pluto);

            this.RecalculatePositions();
        }
    }
}
