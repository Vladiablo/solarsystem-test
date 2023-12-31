﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Shader : IAsset
    {
        #region Fields

        private ShaderType _type;
        private uint _id;
        private bool _isDeleted;
        private bool _isCompiled;

        #endregion

        #region Properties

        public string AssetId { get; init; }
        public uint Id { get { return _id; } }
        public bool IsCompiled { get { return _isCompiled; } }

        #endregion

        public Shader(ShaderType type)
        {
            this._type = type;
            this._id = Gl.CreateShader(type);
            this._isDeleted = false;
            this._isCompiled = false;
        }

        public Shader(string source, ShaderType type)
        {
            this._type = type;
            this._id = Gl.CreateShader(type);
            this._isDeleted = false;
            this._isCompiled = false;
            Gl.ShaderSource(this._id, new string[] { source });
        }

        public static Shader ReadFromFile(string path, ShaderType type)
        {
            string source = File.ReadAllText(path);

            return new Shader(source, type);
        }

        ~Shader() 
        {
            this.Delete();
        }

        public void SetSource(string source) 
        {
            Gl.ShaderSource(this._id, new string[] { source });
        }

        public void Compile()
        {
            Gl.CompileShader(this._id);

            Gl.GetShader(this._id, ShaderParameterName.CompileStatus, out int compileStatus);
            if (compileStatus == Gl.FALSE)
            {
                Gl.GetShader(this._id, ShaderParameterName.InfoLogLength, out int infoLogLength);

                if (infoLogLength <= 0)
                    throw new Exception($"Failed to compile shader {this.AssetId}");

                StringBuilder infoLog = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(this._id, infoLogLength, out int length, infoLog);

                throw new Exception($"Failed to compile shader {this.AssetId}. Info Log:\n{infoLog.ToString()}");
            }

            this._isCompiled = true;
        }

        public void Delete()
        {
            if (this._isDeleted) return;
            Gl.DeleteShader(this._id);
            this._isDeleted = true;
        }

        public static  string GetAssetBaseDir()
        {
            return "Shaders/";
        }

        public void Unload()
        {
            this.Delete();
        }
    }
}
