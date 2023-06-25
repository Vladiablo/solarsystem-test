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
using SolarSystem.Math;
using System.Runtime.Intrinsics;
using GLFW;
using System.Runtime.CompilerServices;

namespace SolarSystem
{
    internal class SimulationUI
    {
        private SimulationSystem simulation;
        private SimulationSystemRenderer simulationRenderer;
        private IReadOnlyList<BodyBase> bodies;

        public float simulationTime;
        public float renderingTime;
        private Vector3 cameraPositionEntry;
        private Vector3 cameraRotationEntry;
        private bool showDemoWindow;
        private bool vsync;

        private bool drawBodyNames;
        private string simulationTimeEntry;
        private double simulationSpeedEntry;
        private bool simulatePhysicsEntry;
        private int simulationIterationsEntry;
        private bool realScaleEntry;
        private bool drawOrbitsEntry;
        private float sunScaleEntry;
        private float bodiesScaleEntry;
        private float cameraFovEntry;

        private BodyBase targetBody;
        private bool drawBodyInfo;

        public SimulationUI(SimulationSystem simulation, SimulationSystemRenderer simulationRenderer)
        {
            this.simulation = simulation;
            this.simulationRenderer = simulationRenderer;
            this.bodies = simulation.Bodies;

            this.vsync = true;
        }

        private void DrawDebugWindow()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.SetNextWindowPos(new Vector2(0.0f, 0.0f), ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(new Vector2(320.0f, 256.0f), ImGuiCond.Appearing);
            ImGui.SetNextWindowCollapsed(true, ImGuiCond.Appearing);
            if (ImGui.Begin("Debug"))
            {
                if (ImGui.CollapsingHeader("Performance"))
                {
                    float frameTime = io.DeltaTime;

                    ImGui.Text($"Simulation Time: {this.simulationTime:f3} ms");
                    ImGui.Text($"Rendering Time: {this.renderingTime:f3} ms");
                    ImGui.Text($"Frame Time: {frameTime * 1000.0f:f3} ms");
                    ImGui.Text($"FPS: {1.0f / frameTime:f1}");
                }

                if (ImGui.CollapsingHeader("Camera"))
                {
                    this.cameraPositionEntry = this.simulationRenderer.Camera.Position;
                    if (ImGui.DragFloat3("Position", ref this.cameraPositionEntry, 10.0f))
                    {
                        this.simulationRenderer.Camera.Position = this.cameraPositionEntry;
                    }

                    this.cameraRotationEntry = this.simulationRenderer.Camera.Rotation;
                    if (ImGui.DragFloat3("Rotation", ref this.cameraRotationEntry, 1.0f))
                    {
                        this.simulationRenderer.Camera.Rotation = this.cameraRotationEntry;
                    }
                }

                ImGui.Checkbox("Show ImGui Demo", ref this.showDemoWindow);
                if (this.showDemoWindow)
                    ImGui.ShowDemoWindow(ref this.showDemoWindow);

                if (ImGui.Checkbox("VSync", ref this.vsync))
                    Glfw.SwapInterval(this.vsync ? 1 : 0);

                ImGui.End();
            }

        }

        private void SetTargetBody(BodyBase targetBody)
        {
            this.targetBody = targetBody;
            this.drawBodyInfo = targetBody != null;
            this.simulationRenderer.TargetBody = targetBody;
        }

        private void DrawSimulationWindow()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            Vector2 displaySize = io.DisplaySize;

            ImGui.SetNextWindowPos(new Vector2(displaySize.X - 480.0f, 0.0f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(480.0f, 320.0f), ImGuiCond.Appearing);
            if (ImGui.Begin("Симуляция"))
            {
                this.simulationTimeEntry = this.simulation.SimulationTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (ImGui.InputText("Время симуляции", ref this.simulationTimeEntry, 32,
                    ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (DateTime.TryParse(this.simulationTimeEntry, out DateTime result))
                        this.simulation.SimulationTime = result;
                }

                if (ImGui.Button("Установить текущее время"))
                {
                    this.simulation.SimulationTime = DateTime.Now;
                }

                this.simulationSpeedEntry = this.simulation.TimeScale;
                if (ImGui.InputDouble("Скорость симуляции", ref this.simulationSpeedEntry, 1.0f, 10.0f))
                {
                    this.simulationSpeedEntry = System.Math.Clamp(this.simulationSpeedEntry, SimulationSystem.TIME_SCALE_MIN, SimulationSystem.TIME_SCALE_MAX);
                    this.simulation.TimeScale = this.simulationSpeedEntry;
                }

                this.simulatePhysicsEntry = this.simulation.SimulatePhysics;
                if (ImGui.Checkbox("Симулировать физику", ref this.simulatePhysicsEntry))
                {
                    this.simulation.SimulatePhysics = this.simulatePhysicsEntry;
                }

                if (this.simulatePhysicsEntry)
                {
                    this.simulationIterationsEntry = this.simulation.SolverIterations;
                    if (ImGui.SliderInt("Количество итераций", ref this.simulationIterationsEntry, 1, 256))
                    {
                        this.simulation.SolverIterations = (ushort)this.simulationIterationsEntry;
                    }
                }

                ImGui.Checkbox("Отображать названия тел", ref this.drawBodyNames);

                if (ImGui.CollapsingHeader("Тела"))
                {
                    if (ImGui.BeginTable("bodiesButtons", 5))
                    {
                        for (int i = 0; i < this.bodies.Count; ++i)
                        {
                            ImGui.TableNextColumn();
                            if (ImGui.Button(this.bodies[i].Name))
                            {
                                this.SetTargetBody(this.bodies[i]);
                            }
                        }
                        ImGui.EndTable();
                    }

                }

                if (ImGui.Button("Сбросить цель"))
                {
                    this.SetTargetBody(null);
                }

                if (ImGui.CollapsingHeader("Отрисовка"))
                {
                    this.drawOrbitsEntry = this.simulationRenderer.DrawOrbits;
                    if (ImGui.Checkbox("Отображать орбиты", ref this.drawOrbitsEntry))
                    {
                        this.simulationRenderer.DrawOrbits = this.drawOrbitsEntry;
                    }

                    this.realScaleEntry = this.simulationRenderer.RealScale;
                    if (ImGui.Checkbox("Реальные размеры", ref this.realScaleEntry))
                    {
                        this.simulationRenderer.RealScale = this.realScaleEntry;
                    }

                    if (this.realScaleEntry)
                    {
                        this.sunScaleEntry = this.simulationRenderer.SunScale;
                        if (ImGui.SliderFloat("Размер Солнца", ref this.sunScaleEntry, 1.0f, 100.0f))
                        {
                            this.simulationRenderer.SunScale = this.sunScaleEntry;
                        }

                        this.bodiesScaleEntry = this.simulationRenderer.BodiesScale;
                        if (ImGui.SliderFloat("Размер тел", ref this.bodiesScaleEntry, 1.0f, 1000.0f))
                        {
                            this.simulationRenderer.BodiesScale = this.bodiesScaleEntry;
                        }

                    }

                    this.cameraFovEntry = this.simulationRenderer.Camera.FovY;
                    if (ImGui.SliderFloat("Поле зрения камеры", ref this.cameraFovEntry, 0.001f, 90.0f))
                    {
                        this.simulationRenderer.Camera.FovY = this.cameraFovEntry;
                    }

                }
                ImGui.End();
            }

        }

        private void HitTestBodies()
        {
            if (this.targetBody != null) return;

            ImGuiIOPtr io = ImGui.GetIO();
            Vector2 displaySize = io.DisplaySize;

            Matrix4x4 proj = this.simulationRenderer.Camera.Projection;
            Matrix4x4 view = this.simulationRenderer.Camera.GetViewMatrix();
            Matrix4x4 vp = view * proj;
            
            for (int i = 0; i < this.bodies.Count; ++i)
            {
                Vector3 worldPosition = 
                    MathHelper.ConvertDoubleVectorToFloat(this.bodies[i].Position * MathHelper.RENDER_TO_SIMULATION_SCALE)
                    .AsVector128()
                    .AsVector3();

                Vector4 position = Vector4.Transform(worldPosition, vp);
                position /= position.W;

                if (position.X > 1.0f || position.X < -1.0f ||
                    position.Y > 1.0f || position.Y < -1.0f ||
                    position.Z > 1.0f)
                    continue;

                Vector2 normalizedPos = new Vector2((position.X + 1.0f) / 2.0f, (position.Y + 1.0f) / 2.0f);

                float bodyScale = (float)(this.bodies[i].Radius * MathHelper.RENDER_TO_SIMULATION_SCALE) * this.bodies[i].RenderScale * 2.0f;
                float scale = bodyScale / (Vector3.Distance(worldPosition, this.simulationRenderer.Camera.Position) *
                    MathF.Sin(MathHelper.DegToRad(this.simulationRenderer.Camera.FovY)));

                Vector2 boxSize = new Vector2(displaySize.Y * scale, displaySize.Y * scale);
                if (boxSize.X < 64.0f)
                    boxSize.X = 64.0f;
                if (boxSize.Y < 64.0f)
                    boxSize.Y = 64.0f;
                 
                Vector2 boxPos = new Vector2(
                    normalizedPos.X * displaySize.X - boxSize.X / 2.0f,
                    displaySize.Y - normalizedPos.Y * displaySize.Y - boxSize.Y / 2.0f);

                //Vector4 bodyRect = new Vector4(boxPos.X, boxPos.Y, boxPos.X + boxSize.X, boxPos.Y + boxSize.Y);

                //if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                //{
                //    Vector2 mousePos = ImGui.GetMousePos();
                //    if (mousePos.X > bodyRect.X && mousePos.X < bodyRect.Z &&
                //        mousePos.Y > bodyRect.Y && mousePos.Y < bodyRect.W)
                //    {
                //        this.SetTargetBody(this.bodies[i]);
                //    }
                //}

                ImGui.SetNextWindowPos(boxPos);
                ImGui.SetNextWindowSize(boxSize);
                ImGui.SetNextWindowBgAlpha(0.0f);
                ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 0.0f);
                if (ImGui.Begin($"{this.bodies[i].Name} button", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoScrollbar))
                {
                    if (ImGui.InvisibleButton($"body{i}", boxSize))
                    {
                        this.SetTargetBody(this.bodies[i]);
                    }

                    ImGui.End();
                }
                ImGui.PopStyleVar();


                if (this.drawBodyNames)
                {
                    Vector2 windowPos = new Vector2(
                        normalizedPos.X * displaySize.X + displaySize.Y * scale / 2.0f,
                        displaySize.Y - normalizedPos.Y * displaySize.Y);

                    if (windowPos.X + 80.0f > displaySize.X)
                        windowPos.X -= 80.0f;

                    ImGui.SetNextWindowPos(windowPos);
                    ImGui.SetNextWindowSize(new Vector2(80.0f, 0.0f));
                    ImGui.SetNextWindowBgAlpha(0.5f);
                    if (ImGui.Begin($"{this.bodies[i].Name}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove |
                        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.NoNavFocus))
                    {
                        //ImGui.SetWindowFontScale(1.35f);
                        ImGui.Text($"{this.bodies[i].Name}");

                        ImGui.End();
                    }
                    
                }

            }

        }

        private void DrawBodyInfo()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            Vector2 displaySize = io.DisplaySize;

            if (this.drawBodyInfo)
            {
                ImGui.SetNextWindowPos(new Vector2(displaySize.X - 480.0f, 320.0f), ImGuiCond.Appearing);
                if (ImGui.Begin("Информация о теле", ref this.drawBodyInfo))
                {
                    ImGui.SeparatorText(this.targetBody.Name);

                    if (ImGui.CollapsingHeader("Физические характеристики", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        if (ImGui.BeginTable("physicalData", 2))
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text("Масса");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.Mass} кг");

                            ImGui.TableNextColumn();
                            ImGui.Text("Экваториальный радиус");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.EquatorialRadius / 1000.0} км");

                            ImGui.TableNextColumn();
                            ImGui.Text("Полярный радиус");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.PolarRadius / 1000.0} км");

                            ImGui.TableNextColumn();
                            ImGui.Text("Средний радиус");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.Radius / 1000.0} км");

                            ImGui.TableNextColumn();
                            ImGui.Text("Полярное сжатие");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.Flattening}");

                            ImGui.TableNextColumn();
                            ImGui.Text("Площадь поверхности");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.SurfaceArea} км²");

                            ImGui.TableNextColumn();
                            ImGui.Text("Объём");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.Volume} км³");

                            ImGui.TableNextColumn();
                            ImGui.Text("Средняя плотность");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.Density} г/см³");

                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("Ускорение свободного падения на экваторе");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.SurfaceAcceleration} м/с²");

                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("Вторая космическая скорость");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{this.targetBody.EscapeVelocity / 1000.0} км/с");

                            ImGui.TableNextColumn();
                            ImGui.TextWrapped("Экваториальная скорость вращения");
                            ImGui.TableNextColumn();
                            ImGui.TextWrapped($"{this.targetBody.EquatorialVelocity} м/с ({this.targetBody.EquatorialVelocity * 3.6} км/ч)");

                            ImGui.EndTable();
                        }
                    }

                    if (this.targetBody is not Sun)
                    {
                        if (ImGui.CollapsingHeader("Орбитальные характеристики", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SeparatorText("Эпоха: J2000.0");
                            if (ImGui.BeginTable("orbitData", 2))
                            {
                                OrbitData data = this.targetBody.OrbitData;

                                ImGui.TableNextColumn();
                                ImGui.Text("Большая полуось");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.semiMajorAxis / 1000.0} км");

                                ImGui.TableNextColumn();
                                ImGui.Text("Сидерический период обращения");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.siderialOrbitPeriod} сут");

                                ImGui.TableNextColumn();
                                ImGui.Text("Перигелий");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.perihelion / 1000.0} км");

                                ImGui.TableNextColumn();
                                ImGui.Text("Афелий");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.aphelion / 1000.0} км");

                                ImGui.TableNextColumn();
                                ImGui.Text("Средняя орбитальная скорость");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.meanVelocity / 1000.0} км/с");

                                ImGui.TableNextColumn();
                                ImGui.Text("Максимальная орбитальная скорость");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.maxVelocity / 1000.0} км/с");

                                ImGui.TableNextColumn();
                                ImGui.Text("Минимальная орбитальная скорость");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.minVelocity / 1000.0} км/с");

                                ImGui.TableNextColumn();
                                ImGui.Text("Наклонение");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.orbitInclination}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Эксцентриситет орбиты");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.eccentricity}");

                                ImGui.TableNextColumn();
                                ImGui.Text("Период вращения");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.siderialRotationPeriod} ч");

                                ImGui.TableNextColumn();
                                ImGui.Text("Наклон оси");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.equatorInclination}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Долгота восходящего узла");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.ascendingNodeLongitude}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Аргумент перицентра");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.periapsisArgument}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Средняя аномалия");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.meanAnomaly}°");


                                ImGui.EndTable();
                            }
                        }

                        if (ImGui.CollapsingHeader("Текущие орбитальные данные"))
                        {
                            ImGui.SeparatorText($"На момент {this.simulation.OrbitCalculationDate:yyyy-MM-dd HH:mm:ss.fff}");
                            if (ImGui.BeginTable("currentOrbitData", 2))
                            {
                                OrbitData data = this.targetBody.CurrentOrbitData;

                                ImGui.TableNextColumn();
                                ImGui.Text("Большая полуось");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.semiMajorAxis / 1000.0} км");

                                ImGui.TableNextColumn();
                                ImGui.Text("Наклонение");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.orbitInclination}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Эксцентриситет орбиты");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.eccentricity}");

                                ImGui.TableNextColumn();
                                ImGui.Text("Долгота восходящего узла");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.ascendingNodeLongitude}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Аргумент перицентра");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.periapsisArgument}°");

                                ImGui.TableNextColumn();
                                ImGui.Text("Средняя аномалия");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{data.meanAnomaly}°");

                                ImGui.EndTable();
                            }
                        }
                    }

                    if (ImGui.CollapsingHeader("Описание", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.TextWrapped(this.targetBody.Description);
                    }

                    if (ImGui.CollapsingHeader("Данные симуляции"))
                    {
                        if (ImGui.BeginTable("simulationData", 4))
                        {
                            Vector<double> position = this.targetBody.Position;
                            Vector<double> velocity = this.targetBody.Velocity;
                            Vector<double> acceleration = this.targetBody.Acceleration;

                            ImGui.TableNextColumn();
                            ImGui.Text("Расстояние от Солнца (км)");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{System.Math.Sqrt(position[0] * position[0] + position[1] * position[1] + position[2] * position[2]) / 1000.0}");
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();

                            ImGui.TableNextColumn();
                            ImGui.Text("Положение (м)");
                            ImGui.TableNextColumn();
                            ImGui.Text(position[0].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(position[1].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(position[2].ToString());

                            ImGui.TableNextColumn();
                            ImGui.Text("Скорость (м/с)");
                            ImGui.TableNextColumn();
                            ImGui.Text(System.Math.Sqrt(velocity[0] * velocity[0] + velocity[1] * velocity[1] + velocity[2] * velocity[2]).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();

                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.Text(velocity[0].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(velocity[1].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(velocity[2].ToString());

                            ImGui.TableNextColumn();
                            ImGui.Text("Ускорение (м/с²)");
                            ImGui.TableNextColumn();
                            ImGui.Text(System.Math.Sqrt(acceleration[0] * acceleration[0] + acceleration[1] * acceleration[1] + acceleration[2] * acceleration[2]).ToString());
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();

                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                            ImGui.Text(acceleration[0].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(acceleration[1].ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(acceleration[2].ToString());

                            ImGui.EndTable();
                        }
                    }

                    ImGui.End();
                }

                if (this.drawBodyInfo == false)
                {
                    this.SetTargetBody(null);
                }
            }
        }

        public void SubmitUI()
        {
            this.DrawDebugWindow();
            this.DrawSimulationWindow();
            this.DrawBodyInfo();
            this.HitTestBodies();
        }

    }
}
