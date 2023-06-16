using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ImGuiNET;
using SolarSystem.Simulation;
using SolarSystem.Simulation.Bodies;
using SolarSystem.Rendering;

namespace SolarSystem
{
    internal class SimulationUI
    {
        private SimulationSystem simulation;
        private SimulationSystemRenderer simulationRenderer;
        private IReadOnlyList<BodyBase> bodies;

        public float simulationTime;
        public float renderingTime;

        public SimulationUI(SimulationSystem simulation, SimulationSystemRenderer simulationRenderer)
        {
            this.simulation = simulation;
            this.simulationRenderer = simulationRenderer;
            this.bodies = simulation.Bodies;
        }

        private void DrawDebugWindow()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.SetNextWindowPos(new Vector2(0.0f, 0.0f), ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(new Vector2(256.0f, 256.0f), ImGuiCond.Appearing);
            ImGui.SetNextWindowCollapsed(true, ImGuiCond.Appearing);
            ImGui.Begin("Debug");

            if (ImGui.BeginChild("Performance"))
            {
                float frameTime = io.DeltaTime;

                ImGui.Text($"Simulation Time: {this.simulationTime:f3} ms");
                ImGui.Text($"Rendering Time: {this.renderingTime:f3} ms");
                ImGui.Text($"Frame Time: {frameTime * 1000.0f:f3} ms");
                ImGui.Text($"FPS: {1.0f /frameTime:f1}");

                ImGui.EndChild();
            }

            ImGui.End();
        }

        public void SubmitUI()
        {
            DrawDebugWindow();

        }

    }
}
