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
        private uint _id;
        private TextureTarget _target;
        private InternalFormat _internalFormat;
        private PixelFormat _pixelFormat;
        private PixelType _pixelType;

        private byte[] _pixels;
        private int _width;
        private int _height;

        private bool _isDeleted;
        private bool _renderDataLoaded;

        public uint Id { get { return _id; } }

        public string AssetId { get; init; }

        public Texture(TextureTarget target)
        {
            this._id = Gl.GenTexture();
            _target = target;
            this._isDeleted = false;
            this._renderDataLoaded = false;
        }

        ~Texture() 
        {
            this.Delete();
        }

        public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
        {
            Gl.ActiveTexture(textureUnit);
            Gl.BindTexture(this._target, this._id);
        }

        public void SetPixels(PixelFormat pixelFormat, int width, int height, byte[] pixels)
        {
            this._pixelFormat = pixelFormat;
            this._width = width;
            this._height = height;
            this._pixels = pixels;
            this._pixelType = PixelType.UnsignedByte;
        }

        public void SetPixels(PixelFormat pixelFormat, int width, int height, float[] pixels)
        {
            this._pixelFormat = pixelFormat;
            this._width = width;
            this._height = height;
            this._pixels = new byte[pixels.Length * sizeof(float)];
            System.Buffer.BlockCopy(pixels, 0, this._pixels, 0, this._pixels.Length);
            this._pixelType = PixelType.Float;
        }

        public void Allocate(InternalFormat internalFormat, PixelFormat pixelFormat, int width, int height)
        {
            this._internalFormat = internalFormat;
            this._pixelFormat = pixelFormat;
            this._width = width;
            this._height = height;

            Gl.BindTexture(this._target, this._id);
            Gl.TexImage2D(this._target, 0, internalFormat, width, height, 0, pixelFormat, PixelType.UnsignedByte, 0);
        }

        public static string GetAssetBaseDir()
        {
            return "Textures/";
        }

        public void LoadRenderData(
            InternalFormat internalFormat,
            bool generateMipmaps,
            TextureMagFilter magFilter = TextureMagFilter.Nearest,
            TextureMinFilter minFilter = TextureMinFilter.Nearest)
        {
            if(this._renderDataLoaded) return;

            this._internalFormat = internalFormat;

            Gl.BindTexture(this._target, this._id);
            Gl.TexImage2D(this._target, 0, internalFormat, this._width, this._height, 0, this._pixelFormat, this._pixelType, this._pixels);

            if (generateMipmaps)
            {
                Gl.GenerateMipmap(this._target);

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

            Gl.TexParameteri(this._target, TextureParameterName.TextureMagFilter, magFilter);
            Gl.TexParameteri(this._target, TextureParameterName.TextureMinFilter, minFilter);

            this._renderDataLoaded = true;
        }

        public void Delete()
        {
            if (this._isDeleted) return;
            Gl.DeleteTextures(this._id);
            this._isDeleted = true;
        }

        public void Unload()
        {
            this.Delete();
        }
    }
}
