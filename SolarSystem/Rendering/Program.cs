using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Program
    {
        private uint _id;
        private bool _isDeleted;
        private Shader _vertex;
        private Shader _fragment;

        public Program(Shader vertex, Shader fragment)
        {
            this._id = Gl.CreateProgram();
            this._isDeleted = false;
            this._vertex = vertex;
            this._fragment = fragment;

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
                    throw new Exception("Failed to link program");

                StringBuilder infoLog = new StringBuilder();
                Gl.GetShaderInfoLog(this._id, infoLogLength, out int length, infoLog);

                throw new Exception($"Failed to link program. Info Log:\n{infoLog.ToString()}");
            }
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

        public void Delete()
        {
            if (this._isDeleted) return;
            Gl.DeleteProgram(this._id);
            this._isDeleted = true;
        }

    }
}
