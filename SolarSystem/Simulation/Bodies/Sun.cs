using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal class Sun : BodyBase
    {
        public Sun() 
        {
            this._name = "Sun";
            this._mass = 1.9885e30;
        }
    }
}
