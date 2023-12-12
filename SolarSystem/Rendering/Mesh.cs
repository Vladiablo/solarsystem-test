using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace SolarSystem.Rendering
{
    internal class Mesh : IAsset
    {
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _texCoords;
        private Vector3[] _tangents;
        private uint[] _indices;

        private Buffer[] _buffers;
        private VertexArray _vao;
        private bool _renderDataLoaded;


        public VertexArray Vao { get { return _vao; } }

        public int IndicesCount { get { return _indices.Length; } }

        public string AssetId { get; init; }

        public Mesh(Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, uint[] indices)
        {
            this._vertices = vertices;
            this._normals = normals;
            this._texCoords = texCoords;
            this._tangents = null;
            this._indices = indices;
            this._buffers = new Buffer[4];
            this._renderDataLoaded = false;
        }

        public Mesh(Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, Vector3[] tangents, uint[] indices)
        {
            this._vertices = vertices;
            this._normals = normals;
            this._texCoords = texCoords;
            this._tangents = tangents;
            this._indices = indices;
            this._buffers = new Buffer[5];
            this._renderDataLoaded = false;
        }

        ~Mesh()
        {
            this.Unload();
        }

        public static string GetAssetBaseDir() 
        {
            return "Models/"; 
        }

        public void LoadRenderData()
        {
            if (this._renderDataLoaded) return;

            this._vao = new VertexArray();
            this._vao.Bind();

            this._buffers[0] = new Buffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw);
            this._buffers[0].SetData(this._vertices);
            this._vao.BindAttribute(0, 3, VertexAttribType.Float, 0, 0);

            this._buffers[1] = new Buffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw);
            this._buffers[1].SetData(this._normals);
            this._vao.BindAttribute(1, 3, VertexAttribType.Float, 0, 0);

            this._buffers[2] = new Buffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw);
            this._buffers[2].SetData(this._texCoords);
            this._vao.BindAttribute(2, 2, VertexAttribType.Float, 0, 0);

            this._buffers[3] = new Buffer(BufferTarget.ElementArrayBuffer, BufferUsage.StaticDraw);
            this._buffers[3].SetData(this._indices);


            if (this._tangents != null)
            {
                this._buffers[4] = new Buffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw);
                this._buffers[4].SetData(this._tangents);
                this._vao.BindAttribute(3, 3, VertexAttribType.Float, 0, 0);
            }

            this._vao.Unbind();
            this._buffers[2].Unbind();
            this._buffers[3].Unbind();

            this._renderDataLoaded = true;
        }

        public void Unload()
        {
            if (!this._renderDataLoaded) return;

            this._vao.Delete();
            this._vao = null;
            for (int i = 0; i < this._buffers.Length; ++i)
            {
                this._buffers[i].Delete();
                this._buffers[i] = null;
            }

            this._renderDataLoaded = false;
        }
    }
}
