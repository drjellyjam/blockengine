using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class ChunkMeshGenerator
    {
        List<Vector3> verts;
        List<Vector3> normals;
        List<Vector2> uvs;
        List<Color> vertcolors;

        List<Vector3> vertsT;
        List<Vector3> normalsT;
        List<Vector2> uvsT;
        List<Color> vertcolorsT;

        int vertcount = 0;
        int vertcountT = 0;
        public Mesh mesh;
        public Mesh meshT;

        public bool Drawable = false;
        public bool DrawableT = false;
        public ChunkMeshGenerator(uint special_chunk_mesh_id = 0)
        {
            mesh = new Mesh();
            verts = new List<Vector3>();
            normals = new List<Vector3>();
            uvs = new List<Vector2>();
            vertcolors = new List<Color>();

            meshT = new Mesh();
            vertsT = new List<Vector3>();
            normalsT = new List<Vector3>();
            uvsT = new List<Vector2>();
            vertcolorsT = new List<Color>();
        }

        public void Clear()
        {
            verts.Clear();
            normals.Clear();
            uvs.Clear();
            vertcolors.Clear();
            vertcount = 0;

            vertsT.Clear();
            normalsT.Clear();
            uvsT.Clear();
            vertcolorsT.Clear();
            vertcountT = 0;
        }
        public void AddVertex(Vector3 pos, Vector3 norm, Vector2 uv, Color color, bool blocktranslucent = false) {
            if (!blocktranslucent)
            {
                verts.Add(pos);
                normals.Add(norm);
                uvs.Add(uv);
                vertcolors.Add(color);
            }
            else
            {
                vertsT.Add(pos);
                normalsT.Add(norm);
                uvsT.Add(uv);
                vertcolorsT.Add(color);
            }
        }

        public void UnloadMesh()
        {
            if (Drawable)
            {
                Raylib.UnloadMesh(mesh);
                mesh.VaoId = 0;
                Drawable = false;
            }
            if (DrawableT)
            {
                Raylib.UnloadMesh(meshT);
                meshT.VaoId = 0;
                DrawableT = false;
            }
        }
        public bool GenerateMeshT()
        {
            vertcountT = vertsT.Count();

            if (vertcountT == 0)
            {
                Console.WriteLine("Not building empty mesh! (translucent)");
                DrawableT = false;
                return false;
            }

            meshT.TriangleCount = vertcountT / 3;
            meshT.VertexCount = vertcountT;

            meshT.AllocVertices();
            meshT.AllocNormals();
            meshT.AllocTexCoords();
            meshT.AllocColors();

            for (int i = 0; i < vertcountT; i++)
            {
                meshT.VerticesAs<Vector3>()[i] = vertsT[i];
                meshT.NormalsAs<Vector3>()[i] = normalsT[i];
                meshT.TexCoordsAs<Vector2>()[i] = uvsT[i];
                meshT.ColorsAs<Color>()[i] = vertcolorsT[i];
            }

            Raylib.UploadMesh(ref meshT, false);

            DrawableT = true;

            return true;
        }
        public bool GenerateMesh()
        {
            vertcount = verts.Count();

            if (vertcount == 0)
            {
                Console.WriteLine("Not building empty mesh!");
                Drawable = false;
                return false;
            }

            mesh.TriangleCount = vertcount / 3;
            mesh.VertexCount = vertcount;

            mesh.AllocVertices();
            mesh.AllocNormals();
            mesh.AllocTexCoords();
            mesh.AllocColors();

            for (int i = 0; i < vertcount; i++)
            {
                mesh.VerticesAs<Vector3>()[i] = verts[i];
                mesh.NormalsAs<Vector3>()[i] = normals[i];
                mesh.TexCoordsAs<Vector2>()[i] = uvs[i];
                mesh.ColorsAs<Color>()[i] = vertcolors[i];
            }

            Raylib.UploadMesh(ref mesh,false);

            Drawable = true;

            return true;
        }
    }
}
