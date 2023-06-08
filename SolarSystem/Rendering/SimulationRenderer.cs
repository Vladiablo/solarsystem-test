using SolarSystem.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using SolarSystem.Simulation.Bodies;

namespace SolarSystem.Rendering
{
    internal class SimulationSystemRenderer
    {
        private SimulationSystem _simulation;
        private IList<BodyBase> _bodies;

        private Shader _baseColorVert;
        private Shader _baseColorFrag;
        private Program _baseColor;

        public SimulationSystemRenderer(SimulationSystem simulation)
        {
            this._simulation = simulation;
            this._bodies = simulation.Bodies;

            this._baseColorVert = Shader.ReadFromFile("Shaders/baseColor.vert", ShaderType.VertexShader);
            this._baseColorFrag = Shader.ReadFromFile("Shaders/baseColor.frag", ShaderType.FragmentShader);

            this._baseColor = new Program(this._baseColorVert, this._baseColorFrag);
            this._baseColor.CompileShaders();
            this._baseColor.Link();
        }

        public void Render(double deltaTime)
        {
            this._baseColor.Use();
            for (int i = 0; i < this._bodies.Count; ++i)
            {
                
            }
        }

    }
}
