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
            simulation.AddBody(new Mercury(), new Vector<double>(new double[] { 57_910_000_000, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Venus(), new Vector<double>(new double[] { 108_000_000_000.0, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Earth(), new Vector<double>(new double[] { Math.Math.AU, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Moon(), new Vector<double>(new double[] { Math.Math.AU + 384_400_000.0, 0.0, 0.0, 1.0 }));

            simulation.Bodies[1].Velocity = new Vector<double>(new double[] { 0.0, 48_000.0, 0.0, 0.0 });
            simulation.Bodies[2].Velocity = new Vector<double>(new double[] { 0.0, 35_020.0, 0.0, 0.0 });
            simulation.Bodies[3].Velocity = new Vector<double>(new double[] { 0.0, 29_765.0, 0.0, 0.0 });
            simulation.Bodies[4].Velocity = new Vector<double>(new double[] { 0.0, 29_765.0 + 1_023.0, 0.0, 0.0 });

            window.MakeCurrent();
            Glfw.SwapInterval(1);

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            SimulationSystemRenderer renderer = new SimulationSystemRenderer(simulation);

            Stopwatch timer = new Stopwatch();
            double oldTime = Glfw.Time;
            while (!window.IsClosing)
            {
                double newTime = Glfw.Time;
                double deltaTime = newTime - oldTime;
                oldTime = newTime;

                timer.Restart();
                simulation.Update(deltaTime);
                timer.Stop();
                double simulationTime = (double)timer.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;

                Console.WriteLine($"Simulation Time: {simulationTime} ms");

                timer.Restart();
                Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderer.Render(deltaTime);

                window.SwapBuffers();
                timer.Stop();
                double renderingTime = (double)timer.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
                Console.WriteLine($"Rendering Time: {renderingTime} ms");

                timer.Restart();
                Glfw.PollEvents();
                timer.Stop();

                double processingEventsTime = (double)timer.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
                Console.WriteLine($"Events Time: {processingEventsTime} ms");

                double frameTime = simulationTime + renderingTime + processingEventsTime;
                Console.WriteLine($"Total: {frameTime} ms; FPS: {1000.0 / frameTime}");
            }

            Glfw.Terminate();
        }

    }
}