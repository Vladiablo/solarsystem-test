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
            List<int> faces = new List<int>(this._initialCapacity * 9);

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
                            1.0f - float.Parse(values[2])
                        ));
                        break;
                    }
                    case "f":
                    {
                        for (int i = 1; i < 4; ++i)
                        {
                            string[] indexTuple = values[i].Split('/');

                            int vertIndex = int.Parse(indexTuple[0]) - 1;
                            int texIndex = int.Parse(indexTuple[1]) - 1;
                            int normalIndex = int.Parse(indexTuple[2]) - 1;

                            faces.Add(vertIndex);
                            faces.Add(texIndex);
                            faces.Add(normalIndex);
                        }
                        break;
                    }

                    default:
                    {
                        break;
                    }

                }

            }

            Vector3[] verticesUnpacked = new Vector3[faces.Count / 3];
            Vector2[] texCoordsUnpacked = new Vector2[faces.Count / 3];
            Vector3[] normalsUnpacked = new Vector3[faces.Count / 3];

            uint[] indices = new uint[faces.Count / 3];

            for (int i = 0; i < indices.Length; ++i)
            {
                int index = i * 3;
                verticesUnpacked[i] = vertices[faces[index]];
                texCoordsUnpacked[i] = texCoords[faces[index + 1]];
                normalsUnpacked[i] = normals[faces[index + 2]];
                indices[i] = (uint)i;
            }

            return new Mesh(verticesUnpacked, normalsUnpacked, texCoordsUnpacked, indices) { AssetId = assetId };
        }
    }
}
