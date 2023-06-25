using OpenGL;
using SolarSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace SolarSystem
{
    internal static class AssetManager
    {
        private static Dictionary<string, IAsset> _assets = new Dictionary<string, IAsset>();


        public static Shader LoadShader(string id, ShaderType type)
        {
            if (_assets.TryGetValue(id, out IAsset value))
                return value! as Shader;

            Shader shader = new Shader(File.ReadAllText(Path.Combine(Shader.GetAssetBaseDir(), id)), type) { AssetId = id };
            shader.Compile();
            _assets.Add(id, shader);

            Console.WriteLine($"Loaded Shader: {id}");

            return shader;
        }

        public static Rendering.Program LoadProgram(string id)
        {
            if (_assets.TryGetValue(id, out IAsset value))
                return value! as Rendering.Program;

            string[] shaders = File.ReadAllLines(Path.Combine(Rendering.Program.GetAssetBaseDir(), id) + ".prog");

            Shader vertex = LoadShader(shaders[0], ShaderType.VertexShader);
            Shader fragment = LoadShader(shaders[1], ShaderType.FragmentShader);

            Rendering.Program program = new Rendering.Program(vertex, fragment) { AssetId = id };
            program.Link();
            _assets.Add(id, program);

            Console.WriteLine($"Loaded Program: {id}");

            return program;
        }

        public static Mesh LoadMesh(string id)
        {
            if (_assets.TryGetValue(id, out IAsset value))
                return value! as Mesh;

            if (!id.EndsWith(".obj"))
                throw new NotImplementedException("Only OBJ files are currently supported");

            ObjImporter importer = new ObjImporter(Path.Combine(Mesh.GetAssetBaseDir(), id));
            Mesh mesh = importer.Import(id);
            _assets.Add(id, mesh);

            Console.WriteLine($"Loaded Mesh: {id}");

            return mesh;
        }

        public static Texture LoadTexture2D(string id)
        {
            if (_assets.TryGetValue(id, out IAsset value))
                return value! as Texture;

            string path = Path.Combine(Texture.GetAssetBaseDir(), id);
            Image<Rgba32> img = Image.Load<Rgba32>(path);

            int width = img.Width;
            int height = img.Height;

            Gl.Get(GetPName.MaxTextureSize, out int maxSize);
            while ((img.Width > maxSize) || (img.Height > maxSize))
            {
                Console.WriteLine($"Warning! GL Max Texture Size exceeded ({maxSize}). Resizing the texture to fit...");
                img.Mutate(x => x.Resize(img.Width / 2, img.Height / 2));
                width /= 2; 
                height /= 2;
            }

            byte[] pixels = new byte[img.Width * img.Height * 4];
            img.CopyPixelDataTo(pixels);
            img.Dispose();

            Texture tex = new Texture(TextureTarget.Texture2d);
            tex.SetPixels(PixelFormat.Rgba, width, height, pixels);
            _assets.Add(id, tex);

            Console.WriteLine($"Loaded Texture: {id}");

            return tex;
        }

        public static Texture LoadTextureCubemap(string id, string[] faces)
        {
            if (_assets.TryGetValue(id, out IAsset value))
                return value! as Texture;

            string dir = Path.Combine(Texture.GetAssetBaseDir(), id);

            Texture tex = new Texture(TextureTarget.TextureCubeMap);

            for (int i = 0; i < faces.Length; ++i)
            {
                Image<Rgba32> img = Image.Load<Rgba32>(Path.Combine(dir, faces[i]));
                int width = img.Width;
                int height = img.Height;

                byte[] pixels = new byte[img.Width * img.Height * 4];
                img.CopyPixelDataTo(pixels);
                img.Dispose();

                tex.SetCubeMapFace(PixelFormat.Rgba, InternalFormat.Rgba, i, width, height, pixels);
            }

            tex.SetCubemapParameters();
            _assets.Add(id, tex);

            Console.WriteLine($"Loaded CubeMap Texture: {id}");

            return tex;
        }

        public static void UnloadAsset(string id)
        {
            if (!_assets.TryGetValue(id, out IAsset value))
                return;

            value.Unload();
            _assets.Remove(id);
            Console.WriteLine($"Unloaded: {id}");
        }

        public static void UnloadAsset(IAsset asset)
        {
            UnloadAsset(asset.AssetId);
        }

    }
}
