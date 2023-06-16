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
            NativeWindow window = new NativeWindow(1280, 720, "Solar System", GLFW.Monitor.None, Window.None);

            window.MakeCurrent();
            Glfw.SwapInterval(1);

            nint ctx = ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(1280.0f, 720.0f);

            unsafe
            {
                ImFontPtr fontPtr = io.Fonts.AddFontFromFileTTF("Fonts/Roboto-Medium.ttf", 14.0f);

                ImFontConfig* nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
                nativeConfig->DstFont = fontPtr;
                ImFontConfigPtr config = new ImFontConfigPtr(nativeConfig);

                config.MergeMode = true;
                config.SizePixels = 14.0f;

                io.Fonts.AddFontFromFileTTF("Fonts/Roboto-Medium.ttf", 14.0f, config, io.Fonts.GetGlyphRangesCyrillic());
                io.Fonts.Build();

                config.Destroy();
            }

            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

            ImGui.StyleColorsDark();

            ImGuiBackend.Init(window);

            SimulationSystem simulation = new SimulationSystem();

            simulation.TimeScale = 31_557_600.0 / 16.0;
            simulation.SolverIterations = 8;

            simulation.AddBody(new Sun(), new Vector<double>(new double[] { 0.0, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Mercury(), new Vector<double>(new double[] { 57_910_000_000, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Venus(), new Vector<double>(new double[] { 108_000_000_000.0, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Earth(), new Vector<double>(new double[] { MathHelper.AU, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Moon(), new Vector<double>(new double[] { MathHelper.AU + 384_400_000.0, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Mars(), new Vector<double>(new double[] { MathHelper.AU * 1.52, 0.0, 0.0, 1.0 }));
            simulation.AddBody(new Jupiter(), new Vector<double>(new double[] { MathHelper.AU * 5.2, 0.0, 0.0, 1.0 }));

            simulation.Bodies[1].Velocity = new Vector<double>(new double[] { 0.0, 48_000.0, 0.0, 0.0 });
            simulation.Bodies[2].Velocity = new Vector<double>(new double[] { 0.0, 35_020.0, 0.0, 0.0 });
            simulation.Bodies[3].Velocity = new Vector<double>(new double[] { 0.0, 29_765.0, 0.0, 0.0 });
            simulation.Bodies[4].Velocity = new Vector<double>(new double[] { 0.0, 29_765.0 + 1_023.0, 0.0, 0.0 });
            simulation.Bodies[5].Velocity = new Vector<double>(new double[] { 0.0, 24_130.0, 0.0, 0.0 });
            simulation.Bodies[6].Velocity = new Vector<double>(new double[] { 0.0, 13_070.0, 0.0, 0.0 });

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            SimulationSystemRenderer renderer = new SimulationSystemRenderer(simulation, window);
            SimulationUI ui = new SimulationUI(simulation, renderer);

            //Glfw.SetInputMode(window, InputMode.RawMouseMotion, 1);
            //Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);

            Glfw.GetCursorPosition(window, out double cursorXold, out double cursorYold);

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
                ui.simulationTime = (float)timer.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;

                timer.Restart();
                ImGui.NewFrame();
                Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Glfw.GetCursorPosition(window, out double cursorX, out double cursorY);

                float dx = (float)(cursorXold - cursorX);
                float dy = (float)(cursorYold - cursorY);

                cursorXold = cursorX;
                cursorYold = cursorY;

                //renderer.Camera.Rotate(new Vector3(0.0f, -dy / 8.0f, dx / 8.0f));

                if (Glfw.GetKey(window, Keys.W) == InputState.Press) 
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 10.0f, 0.0f));                 
                }

                if (Glfw.GetKey(window, Keys.S) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, -10.0f, 0.0f));   
                }

                if (Glfw.GetKey(window, Keys.A) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(-10.0f, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.D) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(10.0f, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.I) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(-1.0f, 0.0f, 0.0f));            
                }

                if (Glfw.GetKey(window, Keys.K) == InputState.Press)
                {                 
                    renderer.Camera.Rotate(new Vector3(1.0f, 0.0f, 0.0f));
                }

                if (Glfw.GetKey(window, Keys.J) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(0.0f, 0.0f, -1.0f));
                }

                if (Glfw.GetKey(window, Keys.L) == InputState.Press)
                {
                    renderer.Camera.Rotate(new Vector3(0.0f, 0.0f, 1.0f));
                }

                //if (Glfw.GetKey(window, Keys.Q) == InputState.Press)
                //{
                //    renderer.Camera.Rotate(new Vector3(0.0f, 1.0f, 0.0f));
                //}

                //if (Glfw.GetKey(window, Keys.E) == InputState.Press)
                //{
                //    renderer.Camera.Rotate(new Vector3(0.0f, -1.0f, 0.0f));
                //}

                if (Glfw.GetKey(window, Keys.Space) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 0.0f, 10.0f));
                }

                if (Glfw.GetKey(window, Keys.LeftControl) == InputState.Press)
                {
                    renderer.Camera.MoveRelative(new Vector3(0.0f, 0.0f, -10.0f));
                }

                renderer.Render(deltaTime);

                ui.SubmitUI();

                ImGui.Render();
                ImGuiBackend.RenderDrawData(ImGui.GetDrawData(), deltaTime);

                window.SwapBuffers();
                timer.Stop();
                ui.renderingTime = (float)timer.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;

                Glfw.PollEvents();
            }

            ImGuiBackend.Shutdown();
            Glfw.Terminate();
        }

    }
}