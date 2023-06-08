using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Simulation.Bodies
{
    internal class Earth : BodyBase
    {

        public Earth() : base()
        {
            this._name = "Earth";
            this._mass = 5.9726e24;
        }
    }
}
