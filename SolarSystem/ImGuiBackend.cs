using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using OpenGL;
using GLFW;
using SolarSystem.Rendering;

namespace SolarSystem
{

    internal static class ImGuiBackend
    {
        // OpenGL
        private static Texture fontTexture;
        private static Rendering.Program program;
        private static int locationTex;       // Uniforms location
        private static int locationProjMtx;
        private static VertexArray vao;
        private static Rendering.Buffer vbo;
        private static Rendering.Buffer indices;

        // GLFW
        private static NativeWindow window;
        private static bool callbacksInstalled = false;
        private static Vector2 lastMousePos;

        public static void Init(NativeWindow glfwWindow)
        {
            window = glfwWindow;

            InstallCallbacks();

            CreateDeviceObjects();
        }

        public static void Shutdown()
        {
            RestoreCallbacks();
            DestroyDeviceObjects();
        }

        private static void WindowFocusCallback(object sender, EventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddFocusEvent((sender as NativeWindow).IsFocused);
        }

        private static void WindowCursorEnterCallback(NativeWindow win, bool entering)
        {
            if (win.CursorMode == CursorMode.Disabled)
                return;

            ImGuiIOPtr io = ImGui.GetIO();
            
            if (entering)
            {
                io.AddMousePosEvent(lastMousePos.X, lastMousePos.Y);
                return;
            }
            io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
        }

        private static void MouseEnterCallback(object sender, EventArgs e) => WindowCursorEnterCallback(sender as NativeWindow, true);
        private static void MouseLeaveCallback(object sender, EventArgs e) => WindowCursorEnterCallback(sender as NativeWindow, false);

        private static void WindowCursorPosCallback(object sender, MouseMoveEventArgs e)
        {
            NativeWindow win = sender as NativeWindow;
            if (win.CursorMode == CursorMode.Disabled)
                return;

            ImGuiIOPtr io = ImGui.GetIO();

            Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);
            io.AddMousePosEvent(mousePos.X, mousePos.Y);
            lastMousePos = mousePos;
        }

        private static void UpdateKeyModifiers(NativeWindow win)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddKeyEvent(ImGuiKey.ModCtrl, (Glfw.GetKey(win, Keys.LeftControl) == InputState.Press) || (Glfw.GetKey(win, Keys.RightControl) == InputState.Press));
            io.AddKeyEvent(ImGuiKey.ModShift, (Glfw.GetKey(win, Keys.LeftShift) == InputState.Press) || (Glfw.GetKey(win, Keys.RightShift) == InputState.Press));
            io.AddKeyEvent(ImGuiKey.ModAlt, (Glfw.GetKey(win, Keys.LeftAlt) == InputState.Press) || (Glfw.GetKey(win, Keys.RightAlt) == InputState.Press));
            io.AddKeyEvent(ImGuiKey.ModSuper, (Glfw.GetKey(win, Keys.LeftSuper) == InputState.Press) || (Glfw.GetKey(win, Keys.RightSuper) == InputState.Press));
        }

        private static void WindowMouseButtonCallback(object sender, MouseButtonEventArgs e)
        {
            UpdateKeyModifiers(sender as NativeWindow);

            ImGuiIOPtr io = ImGui.GetIO();
            if ((int)e.Button >= 0 && (int)e.Button < (int)ImGuiMouseButton.COUNT)
                io.AddMouseButtonEvent((int)e.Button, e.Action == InputState.Press);
        }

        private static void WindowScrollCallback(object sender, MouseMoveEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddMouseWheelEvent((float)e.X, (float)e.Y);
        }

        private static ImGuiKey MapGlfwKeyToImGui(Keys key)
        {
            return key switch
            {
                Keys.Tab => ImGuiKey.Tab,
                Keys.Left => ImGuiKey.LeftArrow,
                Keys.Right => ImGuiKey.RightArrow,
                Keys.Up => ImGuiKey.UpArrow,
                Keys.Down => ImGuiKey.DownArrow,
                Keys.PageUp => ImGuiKey.PageUp,
                Keys.PageDown => ImGuiKey.PageDown,
                Keys.Home => ImGuiKey.Home,
                Keys.End => ImGuiKey.End,
                Keys.Insert => ImGuiKey.Insert,
                Keys.Delete => ImGuiKey.Delete,
                Keys.Backspace => ImGuiKey.Backspace,
                Keys.Space => ImGuiKey.Space,
                Keys.Enter => ImGuiKey.Enter,
                Keys.Escape => ImGuiKey.Escape,
                Keys.Apostrophe => ImGuiKey.Apostrophe,
                Keys.Comma => ImGuiKey.Comma,
                Keys.Minus => ImGuiKey.Minus,
                Keys.Period => ImGuiKey.Period,
                Keys.Slash => ImGuiKey.Slash,
                Keys.SemiColon => ImGuiKey.Semicolon,
                Keys.Equal => ImGuiKey.Equal,
                Keys.LeftBracket => ImGuiKey.LeftBracket,
                Keys.Backslash => ImGuiKey.Backslash,
                Keys.RightBracket => ImGuiKey.RightBracket,
                Keys.GraveAccent => ImGuiKey.GraveAccent,
                Keys.CapsLock => ImGuiKey.CapsLock,
                Keys.ScrollLock => ImGuiKey.ScrollLock,
                Keys.NumLock => ImGuiKey.NumLock,
                Keys.PrintScreen => ImGuiKey.PrintScreen,
                Keys.Pause => ImGuiKey.Pause,
                Keys.Numpad0 => ImGuiKey.Keypad0,
                Keys.Numpad1 => ImGuiKey.Keypad1,
                Keys.Numpad2 => ImGuiKey.Keypad2,
                Keys.Numpad3 => ImGuiKey.Keypad3,
                Keys.Numpad4 => ImGuiKey.Keypad4,
                Keys.Numpad5 => ImGuiKey.Keypad5,
                Keys.Numpad6 => ImGuiKey.Keypad6,
                Keys.Numpad7 => ImGuiKey.Keypad7,
                Keys.Numpad8 => ImGuiKey.Keypad8,
                Keys.Numpad9 => ImGuiKey.Keypad9,
                Keys.NumpadDecimal => ImGuiKey.KeypadDecimal,
                Keys.NumpadDivide => ImGuiKey.KeypadDivide,
                Keys.NumpadMultiply => ImGuiKey.KeypadMultiply,
                Keys.NumpadSubtract => ImGuiKey.KeypadSubtract,
                Keys.NumpadAdd => ImGuiKey.KeypadAdd,
                Keys.NumpadEnter => ImGuiKey.KeypadEnter,
                Keys.NumpadEqual => ImGuiKey.KeypadEqual,
                Keys.LeftShift => ImGuiKey.LeftShift,
                Keys.LeftControl => ImGuiKey.LeftCtrl,
                Keys.LeftAlt => ImGuiKey.LeftAlt,
                Keys.LeftSuper => ImGuiKey.LeftSuper,
                Keys.RightShift => ImGuiKey.RightShift,
                Keys.RightControl => ImGuiKey.RightCtrl,
                Keys.RightAlt => ImGuiKey.RightAlt,
                Keys.RightSuper => ImGuiKey.RightSuper,
                Keys.Menu => ImGuiKey.Menu,
                Keys.Alpha0 => ImGuiKey._0,
                Keys.Alpha1 => ImGuiKey._1,
                Keys.Alpha2 => ImGuiKey._2,
                Keys.Alpha3 => ImGuiKey._3,
                Keys.Alpha4 => ImGuiKey._4,
                Keys.Alpha5 => ImGuiKey._5,
                Keys.Alpha6 => ImGuiKey._6,
                Keys.Alpha7 => ImGuiKey._7,
                Keys.Alpha8 => ImGuiKey._8,
                Keys.Alpha9 => ImGuiKey._9,
                Keys.A => ImGuiKey.A,
                Keys.B => ImGuiKey.B,
                Keys.C => ImGuiKey.C,
                Keys.D => ImGuiKey.D,
                Keys.E => ImGuiKey.E,
                Keys.F => ImGuiKey.F,
                Keys.G => ImGuiKey.G,
                Keys.H => ImGuiKey.H,
                Keys.I => ImGuiKey.I,
                Keys.J => ImGuiKey.J,
                Keys.K => ImGuiKey.K,
                Keys.L => ImGuiKey.L,
                Keys.M => ImGuiKey.M,
                Keys.N => ImGuiKey.N,
                Keys.O => ImGuiKey.O,
                Keys.P => ImGuiKey.P,
                Keys.Q => ImGuiKey.Q,
                Keys.R => ImGuiKey.R,
                Keys.S => ImGuiKey.S,
                Keys.T => ImGuiKey.T,
                Keys.U => ImGuiKey.U,
                Keys.V => ImGuiKey.V,
                Keys.W => ImGuiKey.W,
                Keys.X => ImGuiKey.X,
                Keys.Y => ImGuiKey.Y,
                Keys.Z => ImGuiKey.Z,
                Keys.F1 => ImGuiKey.F1,
                Keys.F2 => ImGuiKey.F2,
                Keys.F3 => ImGuiKey.F3,
                Keys.F4 => ImGuiKey.F4,
                Keys.F5 => ImGuiKey.F5,
                Keys.F6 => ImGuiKey.F6,
                Keys.F7 => ImGuiKey.F7,
                Keys.F8 => ImGuiKey.F8,
                Keys.F9 => ImGuiKey.F9,
                Keys.F10 => ImGuiKey.F10,
                Keys.F11 => ImGuiKey.F11,
                Keys.F12 => ImGuiKey.F12,
                _ => ImGuiKey.None
            };

        }

        private static void WindowKeyCallback(object sender, KeyEventArgs e)
        {
            if (e.State != InputState.Press && e.State != InputState.Release)
                return;

            UpdateKeyModifiers(sender as NativeWindow);

            ImGuiIOPtr io = ImGui.GetIO();
            ImGuiKey key = MapGlfwKeyToImGui(e.Key);

            io.AddKeyEvent(key, e.State == InputState.Press);
            io.SetKeyEventNativeData(key, (int)e.Key, e.ScanCode);
        }

        private static void WindowCharCallback(object sender, CharEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddInputCharacter(e.CodePoint);
        }

        private static void WindowSizeChanged(object sender, SizeChangeEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2((float)e.Size.Width, (float)e.Size.Height);
        }

        private static void WindowFramebufferSizeChanged(object sender, SizeChangeEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            Glfw.GetWindowSize(sender as NativeWindow, out int w, out int h);

            io.DisplayFramebufferScale = new Vector2((float)e.Size.Width / (float)w, (float)e.Size.Height / (float)h);
        }


        private static void InstallCallbacks()
        {
            if (callbacksInstalled) return;

            window.FocusChanged += WindowFocusCallback;
            window.MouseEnter += MouseEnterCallback;
            window.MouseLeave += MouseLeaveCallback;
            window.MouseMoved += WindowCursorPosCallback;
            window.MouseButton += WindowMouseButtonCallback;
            window.MouseScroll += WindowScrollCallback;
            window.KeyAction += WindowKeyCallback;
            window.CharacterInput += WindowCharCallback;
            window.SizeChanged += WindowSizeChanged;
            window.FramebufferSizeChanged += WindowFramebufferSizeChanged;

            callbacksInstalled = true;
        }


        private static void RestoreCallbacks()
        {
            if (!callbacksInstalled) return;

            window.FocusChanged -= WindowFocusCallback;
            window.MouseEnter -= MouseEnterCallback;
            window.MouseLeave -= MouseLeaveCallback;
            window.MouseMoved -= WindowCursorPosCallback;
            window.MouseButton -= WindowMouseButtonCallback;
            window.MouseScroll -= WindowScrollCallback;
            window.KeyAction -= WindowKeyCallback;
            window.CharacterInput -= WindowCharCallback;
            window.SizeChanged -= WindowSizeChanged;
            window.FramebufferSizeChanged -= WindowFramebufferSizeChanged;

            callbacksInstalled = false;
        }

        private static void CreateDeviceObjects()
        {
            program = AssetManager.LoadProgram("imgui");
            locationTex = program.GetUniformLocation("Texture");
            locationProjMtx = program.GetUniformLocation("ProjMtx");

            vbo = new Rendering.Buffer(BufferTarget.ArrayBuffer, BufferUsage.StreamDraw);
            indices = new Rendering.Buffer(BufferTarget.ElementArrayBuffer, BufferUsage.StreamDraw);

            vao = new VertexArray();
            vao.Bind();

            vbo.Bind();
            indices.Bind();

            int stride = Marshal.SizeOf<ImDrawVert>();
            nint offset0 = Marshal.OffsetOf<ImDrawVert>("pos");
            nint offset1 = Marshal.OffsetOf<ImDrawVert>("uv");
            nint offset2 = Marshal.OffsetOf<ImDrawVert>("col");

            vao.BindAttribute(0, 2, VertexAttribType.Float, stride, offset0);
            vao.BindAttribute(1, 2, VertexAttribType.Float, stride, offset1);
            vao.BindAttribute(2, 4, VertexAttribType.UnsignedByte, stride, offset2, true);

            vao.Unbind();
            vbo.Unbind();
            indices.Unbind();

            CreateFontsTexture();
        }

        private static void DestroyDeviceObjects()
        {
            vao.Delete();
            vbo.Delete();
            indices.Delete();

            vao = null;
            vbo = null;
            indices = null;

            AssetManager.UnloadAsset(program);
            program = null;

            DestroyFontsTexture();
        }

        private static void CreateFontsTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.Fonts.GetTexDataAsRGBA32(out nint pixelsPtr, out int width, out int height);

            byte[] pixels = new byte[width * height * 4];
            Marshal.Copy(pixelsPtr, pixels, 0, pixels.Length);

            fontTexture = new Texture(TextureTarget.Texture2d);
            fontTexture.SetPixels(PixelFormat.Rgba, width, height, pixels);
            fontTexture.LoadRenderData(InternalFormat.Rgba, false, TextureMagFilter.Linear, TextureMinFilter.Linear);

            io.Fonts.SetTexID((nint)fontTexture.Id);
        }

        private static void DestroyFontsTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            fontTexture.Delete();
            fontTexture = null;

            io.Fonts.SetTexID(0);
        }

        public static void RenderDrawData(ImDrawDataPtr data, double deltaTime)
        {
            if (program == null)
                throw new System.Exception("Program is null! Did you call Init()?");

            ImGuiIOPtr io = ImGui.GetIO();
            io.DeltaTime = (float)deltaTime;

            Vector2 displaySize = data.DisplaySize;
            Vector2 fbScale = data.FramebufferScale;
            Vector2 displayPos = data.DisplayPos;

            int fbWidth = (int)(displaySize.X * fbScale.X);
            int fbHeight = (int)(displaySize.Y * fbScale.Y);

            if (fbWidth <= 0 || fbHeight <= 0)
                return;

            Gl.Enable(EnableCap.Blend);
            Gl.BlendEquation(BlendEquationMode.FuncAdd);
            Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            Gl.Disable(EnableCap.CullFace);
            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.StencilTest);
            Gl.Enable(EnableCap.ScissorTest);

            Gl.Viewport(0, 0, fbWidth, fbHeight);
            float l = displayPos.X;
            float r = displayPos.X + displaySize.X;
            float t = displayPos.Y;
            float b = displayPos.Y + displaySize.Y;

            Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(l, r, b, t, 0.0f, 1.0f);

            program.Use();
            Gl.Uniform1i(locationTex, 1, 0);
            Gl.UniformMatrix4f(locationProjMtx, 1, false, ortho);

            nint vertSize = Marshal.SizeOf<ImDrawVert>();

            int drawListCnt = data.CmdListsCount;
            for (int i = 0; i < drawListCnt; ++i)
            {
                ImDrawListPtr cmdList = data.CmdListsRange[i];

                nint vtxBufferSize = cmdList.VtxBuffer.Size * vertSize;
                nint idxBufferSize = cmdList.IdxBuffer.Size * sizeof(ushort);

                vbo.SetData(cmdList.VtxBuffer.Data, (uint)vtxBufferSize);
                indices.SetData(cmdList.IdxBuffer.Data, (uint)idxBufferSize);

                int cmdCnt = cmdList.CmdBuffer.Size;
                for (int cmd_i = 0; cmd_i < cmdCnt; ++cmd_i)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmd_i];

                    if (pcmd.UserCallback != 0)
                    {
                        continue;
                    }

                    Vector4 clipRect = pcmd.ClipRect;
                    Vector2 clipMin = new Vector2((clipRect.X - displayPos.X) * fbScale.X, (clipRect.Y - displayPos.Y) * fbScale.Y);
                    Vector2 clipMax = new Vector2((clipRect.Z + displayPos.X) * fbScale.X, (clipRect.W - displayPos.Y) * fbScale.Y);

                    if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                        continue;

                    Gl.Scissor((int)clipMin.X, (int)((float)fbHeight - clipMax.Y), (int)(clipMax.X - clipMin.X), (int)(clipMax.Y - clipMin.Y));

                    fontTexture.Bind(TextureUnit.Texture0);
                    vao.Bind();

                    Gl.DrawElements(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (nint)(pcmd.IdxOffset * sizeof(ushort)));
                }

            }

            vao.Unbind();
            Gl.Disable(EnableCap.ScissorTest);
            Gl.Disable(EnableCap.Blend);
        }

    }

  
}
