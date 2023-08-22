using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Texture : IAsset
    {
        private uint id;
        private TextureTarget target;
        private InternalFormat internalFormat;
        private PixelFormat pixelFormat;
        private PixelType pixelType;

        private byte[] pixels;
        private int width;
        private int height;

        private bool isDeleted;
        private bool renderDataLoaded;

        public uint Id { get { return id; } }

        public string AssetId { get; init; }

        public Texture(TextureTarget target)
        {
            this.id = Gl.GenTexture();
            this.target = target;
            this.isDeleted = false;
            this.renderDataLoaded = false;
        }

        ~Texture() 
        {
            this.Delete();
        }

        public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
        {
            Gl.ActiveTexture(textureUnit);
            Gl.BindTexture(this.target, this.id);
        }

        public void SetPixels(PixelFormat pixelFormat, int width, int height, byte[] pixels)
        {
            this.pixelFormat = pixelFormat;
            this.width = width;
            this.height = height;
            this.pixels = pixels;
            this.pixelType = PixelType.UnsignedByte;
        }

        public void SetPixels(PixelFormat pixelFormat, int width, int height, float[] pixels)
        {
            this.pixelFormat = pixelFormat;
            this.width = width;
            this.height = height;
            this.pixels = new byte[pixels.Length * sizeof(float)];
            System.Buffer.BlockCopy(pixels, 0, this.pixels, 0, this.pixels.Length);
            this.pixelType = PixelType.Float;
        }

        public void Allocate(InternalFormat internalFormat, PixelFormat pixelFormat, int width, int height)
        {
            this.internalFormat = internalFormat;
            this.pixelFormat = pixelFormat;
            this.width = width;
            this.height = height;

            Gl.BindTexture(this.target, this.id);
            Gl.TexImage2D(this.target, 0, internalFormat, width, height, 0, pixelFormat, PixelType.UnsignedByte, 0);
        }

        public void SetCubeMapFace(PixelFormat pixelFormat, InternalFormat internalFormat, int face, int width, int height, byte[] pixels)
        {
            if (this.target != TextureTarget.TextureCubeMap)
                throw new Exception($"Texture {this.AssetId} is not a cubemap");

            this.pixelFormat = pixelFormat;
            this.width = width;
            this.height = height;
            this.pixelType = PixelType.UnsignedByte;

            Gl.BindTexture(this.target, this.id);
            Gl.TexImage2D((TextureTarget)(Gl.TEXTURE_CUBE_MAP_POSITIVE_X + face), 0, internalFormat,
                width, height, 0, pixelFormat, PixelType.UnsignedByte, pixels);
        }

        public void SetCubemapParameters()
        {
            Gl.BindTexture(this.target, this.id);
            Gl.TexParameteri(this.target, TextureParameterName.TextureMinFilter, TextureMinFilter.Linear);
            Gl.TexParameteri(this.target, TextureParameterName.TextureMagFilter, TextureMagFilter.Linear);
            Gl.TexParameteri(this.target, TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(this.target, TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(this.target, TextureParameterName.TextureWrapR, TextureWrapMode.ClampToEdge);
        }

        public static string GetAssetBaseDir()
        {
            return "Textures/";
        }

        public void LoadRenderData(
            InternalFormat internalFormat,
            bool generateMipmaps,
            TextureMagFilter magFilter = TextureMagFilter.Nearest,
            TextureMinFilter minFilter = TextureMinFilter.Nearest,
            float anisotropy = 1.0f)
        {
            if(this.renderDataLoaded) return;

            this.internalFormat = internalFormat;

            Gl.BindTexture(this.target, this.id);
            Gl.TexImage2D(this.target, 0, internalFormat, this.width, this.height, 0, this.pixelFormat, this.pixelType, this.pixels);

            if (generateMipmaps)
            {
                Gl.GenerateMipmap(this.target);

                switch (magFilter)
                {
                    case TextureMagFilter.Nearest:
                    {
                        magFilter = (TextureMagFilter)Gl.NEAREST_MIPMAP_NEAREST;
                        break;
                    }
                    case TextureMagFilter.Linear: 
                    {
                        magFilter = (TextureMagFilter)Gl.LINEAR_MIPMAP_LINEAR;
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }

                switch (minFilter)
                {
                    case TextureMinFilter.Nearest:
                    {
                        minFilter = (TextureMinFilter)Gl.NEAREST_MIPMAP_NEAREST;
                        break;
                    }
                    case TextureMinFilter.Linear:
                    {
                        minFilter = (TextureMinFilter)Gl.LINEAR_MIPMAP_LINEAR;
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }

            }

            Gl.TexParameteri(this.target, TextureParameterName.TextureMagFilter, magFilter);
            Gl.TexParameteri(this.target, TextureParameterName.TextureMinFilter, minFilter);

            this.renderDataLoaded = true;
            this.pixels = null;

            if (anisotropy > 1.0f)
            {
                Gl.Extensions extensions = new Gl.Extensions();
                extensions.Query();

                if (!extensions.TextureFilterAnisotropic_EXT && !extensions.TextureFilterAnisotropic_ARB)
                {
                    Console.WriteLine("Warning! Anisotropic filter is not supported on this platform");
                    return;
                }

                Gl.Get((GetPName)Gl.MAX_TEXTURE_MAX_ANISOTROPY, out float maxAnisotropy);
                if (anisotropy > maxAnisotropy)
                    anisotropy = maxAnisotropy;

                Gl.TexParameterf(this.target, (TextureParameterName)Gl.TEXTURE_MAX_ANISOTROPY, anisotropy);

            }

        }

        public void Delete()
        {
            if (this.isDeleted) return;
            Gl.DeleteTextures(this.id);
            this.isDeleted = true;
        }

        public void Unload()
        {
            this.Delete();
        }
    }
}
