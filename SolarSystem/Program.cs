using System;
using System.Numerics;
using GLFW;
using OpenGL;
using SolarSystem.Platform.Windows;
using SolarSystem.Simulation;
using SolarSystem.Simulation.Bodies;
using System.Diagnostics;
using SolarSystem.Rendering;

namespace SolarSystem
{
    class Program
    {

        static void Main(string[] args)
        {
#if !DEBUG
        IntPtr console = Kernel32.GetConsoleWindow();
        User32.ShowWindow(console, User32.CmdShow.Hide);
#endif

            if (!Glfw.Init())
            {
                throw new System.Exception("Failed to initialize GLFW");
            }
            Gl.Initialize();

            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.OpenglForwardCompatible, true);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            NativeWindow window = new NativeWindow(1280, 720, "Solar System", GLFW.Monitor.None, Window.None);

            SimulationSystem simulation = new SimulationSystem();

            simulation.AddBody(new Sun(), new Vector<double>(new double[] { 0.0, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Earth(), new Vector<double>(new double[] { Math.Math.AU, 0.0, 0.0, 1.0 }));
            IList<BodyBase> bodies = simulation.Bodies;

            window.MakeCurrent();
            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            SimulationSystemRenderer renderer = new SimulationSystemRenderer(simulation);

            Stopwatch simulationTime = new Stopwatch();
            double oldTime = Glfw.Time;
            while (!window.IsClosing)
            {
                double newTime = Glfw.Time;
                double deltaTime = newTime - oldTime;
                oldTime = newTime;

                simulationTime.Restart();
                simulation.Update(deltaTime);
                simulationTime.Stop();

                Console.WriteLine(bodies[1].Velocity);
                Console.WriteLine($"Simulation Time: { (double) simulationTime.ElapsedTicks / (double) Stopwatch.Frequency * 1000.0 } ms");

                Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderer.Render(deltaTime);

                window.SwapBuffers();
                Glfw.PollEvents();
            }

            Glfw.Terminate();
        }

    }
}