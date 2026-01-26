using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class ParsedOBJ
    {
        public List<Vector3> vertex_positions;
        public List<Vector3> vertex_normals;
        public List<Vector2> vertex_uvs;

        public ParsedOBJ()
        {
            vertex_positions = new List<Vector3>();
            vertex_normals = new List<Vector3>();
            vertex_uvs = new List<Vector2>();
        }

        public void AddVertex(Vector3 v)
        {
            vertex_positions.Add(v);
        }

        public void AddNormal(Vector3 v)
        {
            vertex_normals.Add(v);
        }

        public void AddUV(Vector2 v)
        {
            vertex_uvs.Add(v);
        }

        public (Vector3 position, Vector3 normal, Vector2 uv) GetVertex(int index)
        {
            return (vertex_positions[index], vertex_positions[index], vertex_uvs[index]);
        }
    }
}
