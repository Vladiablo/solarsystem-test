using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Rendering
{
    internal class ObjImporter
    {

        private string _path;
        private int _initialCapacity;

        public ObjImporter(string path, int initialCapacity = 32768)
        {
            this._path = path;
            _initialCapacity = initialCapacity;
        }

        public Mesh Import(string assetId)
        {
            StreamReader reader = new StreamReader(File.OpenRead(this._path));

            List<Vector3> vertices = new List<Vector3>(this._initialCapacity);
            List<Vector3> normals = new List<Vector3>(this._initialCapacity);
            List<Vector2> texCoords = new List<Vector2>(this._initialCapacity);

            Vector3[] normalsUnpacked = null;
            Vector2[] texCoordsUnpacked = null;
            List<uint> indices = new List<uint>(this._initialCapacity * 3);

            bool arraysAllocated = false;
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (line == null)
                    break;

                string[] values = line.Split(' ');

                switch(values[0])
                {
                    case "v":
                    {
                        vertices.Add(new Vector3
                        (
                            float.Parse(values[1]), 
                            float.Parse(values[2]), 
                            float.Parse(values[3])
                        ));
                        break;
                    }
                    case "vn":
                    {
                        normals.Add(new Vector3
                        (
                            float.Parse(values[1]),
                            float.Parse(values[2]),
                            float.Parse(values[3])
                        ));
                        break;
                    }
                    case "vt":
                    {
                        texCoords.Add(new Vector2
                        (
                            float.Parse(values[1]),
                            float.Parse(values[2])
                        ));
                        break;
                    }
                    case "f":
                    {
                        if (!arraysAllocated)
                        {
                            normalsUnpacked = new Vector3[vertices.Count];
                            texCoordsUnpacked = new Vector2[vertices.Count];
                            arraysAllocated = true;
                        }

                        for (int i = 1; i < 4; ++i)
                        {
                            string[] indexTuple = values[i].Split('/');

                            int index = int.Parse(indexTuple[0]) - 1;
                            int texIndex = int.Parse(indexTuple[1]) - 1;
                            int normalIndex = int.Parse(indexTuple[2]) - 1;

                            indices.Add((uint)index);
                            texCoordsUnpacked[index] = texCoords[texIndex];
                            normalsUnpacked[index] = normals[normalIndex];
                        }
                        break;
                    }

                    default:
                    {
                        break;
                    }

                }

            }

            return new Mesh(vertices.ToArray(), normalsUnpacked, texCoordsUnpacked, indices.ToArray()) { AssetId = assetId };
        }
    }
}
