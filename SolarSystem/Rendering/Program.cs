using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Program : IAsset
    {
        #region Fields

        private uint _id;
        private bool _isLinked;
        private bool _isDeleted;
        private Shader _vertex;
        private Shader _fragment;
        private Dictionary<string, int> _uniformLocations;
        private List<string> _samplers;

        #endregion

        public string AssetId { get; init; }

        public IReadOnlyList<string> Samplers { get {  return _samplers.AsReadOnly(); } }

        public Program(Shader vertex, Shader fragment)
        {
            this._id = Gl.CreateProgram();
            this._isLinked = false;
            this._isDeleted = false;
            this._vertex = vertex;
            this._fragment = fragment;
            this._uniformLocations = new Dictionary<string, int>();
            this._samplers = new List<string>(4);

            Gl.AttachShader(this._id, vertex.Id);
            Gl.AttachShader(this._id, fragment.Id);
        }

        ~Program()
        {
            this.Delete();
        }

        public void CompileShaders()
        {
            this._vertex.Compile();
            this._fragment.Compile();
        }

        public void Link()
        {
            if (this._isLinked) return;

            if (!this._vertex.IsCompiled)
                this._vertex.Compile();
            if (!this._fragment.IsCompiled)
                this._fragment.Compile();

            Gl.LinkProgram(this._id);
            this.Validate();

            Gl.GetProgram(this._id, ProgramProperty.LinkStatus, out int linkStatus);
            if (linkStatus == Gl.FALSE)
            {
                Gl.GetProgram(this._id, ProgramProperty.InfoLogLength, out int infoLogLength);

                if (infoLogLength <= 0)
                    throw new Exception($"Failed to link program {this.AssetId}");

                StringBuilder infoLog = new StringBuilder();
                Gl.GetShaderInfoLog(this._id, infoLogLength, out int length, infoLog);

                throw new Exception($"Failed to link program {this.AssetId}. Info Log:\n{infoLog.ToString()}");
            }

            Gl.GetProgram(this._id, ProgramProperty.ActiveUniforms, out int activeUniforms);

            StringBuilder buff = new StringBuilder(64);
            for (uint i = 0; i < activeUniforms; ++i)
            {
                Gl.GetActiveUniform(this._id, i, 64, out int length, out int size, out int type, buff);
                string name = buff.ToString();
                int location = Gl.GetUniformLocation(this._id, name);

                this._uniformLocations.Add(name, location);

                if(type == Gl.SAMPLER_1D || type == Gl.SAMPLER_2D || type == Gl.SAMPLER_3D)
                {
                    this._samplers.Add(name);
                }
            }

            this._isLinked = true;
        }

        public void Validate()
        {
            Gl.ValidateProgram(this._id);

            Gl.GetProgram(this._id, ProgramProperty.ValidateStatus, out int validateStatus);
            Console.WriteLine($"Program validation status: {validateStatus}");

            Gl.GetProgram(this._id, ProgramProperty.InfoLogLength, out int infoLogLength);
            if (infoLogLength > 0)
            {
                StringBuilder infoLog = new StringBuilder(infoLogLength);
                Gl.GetProgramInfoLog(_id, infoLogLength, out int length, infoLog);
                Console.WriteLine($"Info Log:\n{infoLog.ToString()}");
            }
        }

        public void Use()
        {
            Gl.UseProgram(this._id);
        }

        public int GetUniformLocation(string uniformName)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                return -1;

            return location;
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="mat">Matrix to set uniform with</param>
        public void SetMatrix3(string uniformName, Matrix3x3f mat)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                throw new ArgumentException($"Uniform with name \"{uniformName}\" not found!");

            Gl.UniformMatrix3f(location, 1, false, mat);
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="mat">Matrix to set uniform with</param>
        public bool TrySetMatrix3(string uniformName, Matrix3x3f mat)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                return false;

            Gl.UniformMatrix3f(location, 1, false, mat);
            return true;
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="mat">Matrix to set uniform with</param>
        public void SetMatrix4(string uniformName, Matrix4x4 mat)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                throw new ArgumentException($"Uniform with name \"{uniformName}\" not found!");

            Gl.UniformMatrix4f(location, 1, false, mat);
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="mat">Matrix to set uniform with</param>
        public bool TrySetMatrix4(string uniformName, Matrix4x4 mat)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                return false;

            Gl.UniformMatrix4f(location, 1, false, mat);
            return true;
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="vec">Vector to set uniform with</param>
        public void SetVector3(string uniformName, Vector3 vec)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                throw new ArgumentException($"Uniform with name \"{uniformName}\" not found!");

            Gl.Uniform3f(location, 1, vec);
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="vec">Vector to set uniform with</param>
        public bool TrySetVector3(string uniformName, Vector3 vec)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                return false;

            Gl.Uniform3f(location, 1, vec);
            return true;
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="vec">Vector to set uniform with</param>
        public void SetVector4(string uniformName, Vector4 vec)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                throw new ArgumentException($"Uniform with name \"{uniformName}\" not found!");

            Gl.Uniform4f(location, 1, vec);
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="vec">Vector to set uniform with</param>
        public bool TrySetVector4(string uniformName, Vector4 vec)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                return false;

            Gl.Uniform4f(location, 1, vec);
            return true;
        }

        /// <summary>
        /// Call <c>Use()</c> before this!
        /// </summary>
        /// <param name="uniformName">Name of the uniform variable</param>
        /// <param name="value">Value to set the uinform with</param>
        public void SetInteger(string uniformName, int value)
        {
            if (!this._uniformLocations.TryGetValue(uniformName, out int location))
                throw new ArgumentException($"Uniform with name \"{uniformName}\" not found!");

            Gl.Uniform1i(location, 1, value);
        }

        public void Delete()
        {
            if (this._isDeleted) return;
            Gl.DeleteProgram(this._id);
            this._isDeleted = true;
        }

        public static string GetAssetBaseDir()
        {
            return "Shaders/";
        }

        public void Unload()
        {
            this.Delete();
        }

    }
}
