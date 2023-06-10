using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Buffer
    {
        private uint _id;
        private bool _isDeleted;
        private BufferTarget _target;
        private BufferUsage _usage;

        public Buffer(BufferTarget target, BufferUsage usage)
        {
            this._id = Gl.GenBuffer();
            this._isDeleted = false;
            this._target = target;
            this._usage = usage;
        }

        ~Buffer()
        {
            this.Delete();
        }

        public void Bind()
        {
            Gl.BindBuffer(this._target, this._id);
        }

        public void Unbind()
        {
            Gl.BindBuffer(this._target, 0);
        }

        public void Alocate(uint size)
        {
            Gl.BindBuffer(this._target, this._id);
            Gl.BufferData(this._target, size, 0, this._usage);
        }

        public void UpdateData<T>(nint offset, T[] data) where T : struct
        {
            Gl.BindBuffer(this._target, this._id);
            Gl.BufferSubData(this._target, offset, (uint)(Marshal.SizeOf<T>() * data.Length), data);
        }

        public void SetData<T>(T[] data) where T : struct
        {
            Gl.BindBuffer(this._target, this._id);
            Gl.BufferData(this._target, (uint)(Marshal.SizeOf<T>() * data.Length), data, this._usage);
        }

        public void Delete()
        {
            if (this._isDeleted) return;
            this.Unbind();
            Gl.DeleteBuffers(this._id);
            this._isDeleted = true;
        }
    }
}
