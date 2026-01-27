using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public struct ParsedOBJVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public ParsedOBJVertex(Vector3 _Position, Vector2 _UV, Vector3 _Normal)
        {
            Position = _Position;
            Normal = _Normal;
            UV = _UV;
        }
    }
    public class ParsedOBJ
    {
        public List<Vector3> vertex_positions;
        public List<Vector3> vertex_normals;
        public List<Vector2> vertex_uvs;
        public List<Int3> faces;
        public int face_count;
        public ParsedOBJ()
        {
            vertex_positions = new List<Vector3>();
            vertex_normals = new List<Vector3>();
            vertex_uvs = new List<Vector2>();
            faces = new List<Int3>();
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

        public void AddFace(Int3 v)
        {
            faces.Add(v);
            face_count++;
        }

        public ParsedOBJVertex GetVertex(int index)
        {
            Int3 face = faces[index];
            return new ParsedOBJVertex(vertex_positions[face.x], vertex_uvs[face.y], vertex_normals[face.z]);
        }
    }
}
