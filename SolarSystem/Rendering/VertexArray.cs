using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class VertexArray
    {
        private uint _id;
        private bool _isDeleted;

        public VertexArray()
        {
            this._id = Gl.GenVertexArray();
            this._isDeleted = false;
        }

        ~VertexArray() 
        {
            this.Delete();
        }

        public void Bind()
        {
            Gl.BindVertexArray(this._id);
        }

        /// <summary>
        /// Binds VAO with name 0.
        /// </summary>
        public void Unbind()
        {
            Gl.BindVertexArray(0);
        }

        /// <summary>
        /// Binds attribute to specific location of currently bound vertex array.<br/>
        /// Call <c>Bind()</c> before this!
        /// </summary>
        /// <param name="index">Index of vertex attribute</param>
        /// <param name="size">Number of components per vertex attribute</param>
        /// <param name="type">Data type of each component in the array</param>
        /// <param name="stride">Byte offset between consecutive vertex attributes</param>
        /// <param name="offset">Offset of the first vertex attribute in the currently bound GL_ARRAY_BUFFER</param>
        public void BindAttribute(uint index, int size, VertexAttribType type, int stride, nint offset, bool normalize = false)
        {
            Gl.VertexAttribPointer(index, size, type, normalize, stride, offset);
            Gl.EnableVertexAttribArray(index);
        }

        public void Delete() 
        {
            if (this._isDeleted) return;
            Unbind();
            Gl.DeleteVertexArrays(this._id);
            this._isDeleted = true;
        }

    }
}
