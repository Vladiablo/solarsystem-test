using System;
using System.Numerics;
using GLFW;
using OpenGL;
using ImGuiNET;
using SolarSystem.Platform.Windows;
using SolarSystem.Simulation;
using SolarSystem.Simulation.Bodies;
using System.Diagnostics;
using SolarSystem.Rendering;
using SolarSystem.Math;
using System.Runtime.InteropServices;
using System.Text;

namespace SolarSystem
{
    class Program
    {
        
        static void GlfwError(GLFW.ErrorCode error, nint message)
        {
            Console.WriteLine($"GLFW Error {error}: {Marshal.PtrToStringUTF8(message)}");
        }

        static void Main(string[] args)
        {
#if !DEBUG
            IntPtr console = Kernel32.GetConsoleWindow();
            User32.ShowWindow(console, User32.CmdShow.Hide);
#endif

            Glfw.SetErrorCallback(GlfwError);

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
            Glfw.WindowHint(Hint.Samples, 4);
            NativeWindow window = new NativeWindow(1280, 720, "Solar System", GLFW.Monitor.None, Window.None);

            window.MakeCurrent();
            Glfw.SwapInterval(1);
            Console.WriteLine($"OpenGL Version: {Gl.GetString(StringName.Version)}");
            Console.WriteLine($"OpenGL Renderer: {Gl.GetString(StringName.Renderer)}");

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            nint ctx = ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(1280.0f, 720.0f);

            unsafe
            {
                ImFontPtr fontPtr = io.Fonts.AddFontFromFileTTF("Fonts/Roboto-Medium.ttf", 16.0f);

                ImFontConfig* nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
                nativeConfig->DstFont = fontPtr;
                ImFontConfigPtr config = new ImFontConfigPtr(nativeConfig);

                config.MergeMode = true;
                config.SizePixels = 16.0f;

                io.Fonts.AddFontFromFileTTF("Fonts/Roboto-Medium.ttf", 16.0f, config, io.Fonts.GetGlyphRangesCyrillic());
                io.Fonts.AddFontFromFileTTF("Fonts/Roboto-Medium.ttf", 16.0f, config, io.Fonts.GetGlyphRangesGreek());
                io.Fonts.Build();

                config.Destroy();
            }

            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

            ImGui.StyleColorsDark();

            ImGuiBackend.Init(window);

            SimulationSystem simulation = new SimulationSystem();
            simulation.TimeScale = 31_557_600.0 / 16.0;
            simulation.SolverIterations = 16;
            simulation.LoadSolarSystem();

            SimulationSystemRenderer renderer = new SimulationSystemRenderer(simulation, window);
            renderer.RealScale = false;
            renderer.Camera.Position = new Vector3(0.0f, 0.0f, 10000.0f);
            renderer.Camera.Rotation = new Vector3(90.0f, 0.0f, 0.0f);

            SimulationUI ui = new SimulationUI(simulation, renderer);

            simulation.SimulationTime = DateTime.Now;
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
                float simulationTime = (float)timer.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;

                timer.Restart();
                ImGui.NewFrame();
                Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                if (Glfw.GetKey(window, Keys.W) == InputState.Press) 
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 100.0f * (float)deltaTime, 0.0f));                 
                }

                if (Glfw.GetKey(window, Keys.S) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, -100.0f * (float)deltaTime, 0.0f));   
                }

                if (Glfw.GetKey(window, Keys.A) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(-100.0f * (float)deltaTime, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.D) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(100.0f * (float)deltaTime, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.Space) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 0.0f, 100.0f * (float)deltaTime));
                }

                if (Glfw.GetKey(window, Keys.LeftControl) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 0.0f, -100.0f * (float)deltaTime));
                }

                if (Glfw.GetKey(window, Keys.I) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(-10.0f * (float)deltaTime, 0.0f, 0.0f));            
                }

                if (Glfw.GetKey(window, Keys.K) == InputState.Press)
                {                 
                    renderer.Camera.Rotate(new Vector3(10.0f * (float)deltaTime, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.J) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(0.0f, 0.0f, -10.0f * (float)deltaTime));
                }

                if (Glfw.GetKey(window, Keys.L) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(0.0f, 0.0f, 10.0f * (float)deltaTime));
                }

                //if (Glfw.GetKey(window, Keys.Q) == InputState.Press)
                //{
                //    renderer.Camera.Rotate(new Vector3(0.0f, 1.0f, 0.0f));
                //}

                //if (Glfw.GetKey(window, Keys.E) == InputState.Press)
                //{
                //    renderer.Camera.Rotate(new Vector3(0.0f, -1.0f, 0.0f));
                //}

                renderer.Render(deltaTime);

                ui.SubmitUI();

                ImGui.Render();
                ImGuiBackend.RenderDrawData(ImGui.GetDrawData(), deltaTime);

                window.SwapBuffers();
                timer.Stop();
                ui.simulationTime = simulationTime;
                ui.renderingTime = (float)timer.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;

                Glfw.PollEvents();
            }

            ImGuiBackend.Shutdown();
            Glfw.Terminate();
        }

    }
}