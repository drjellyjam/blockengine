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
        public ChunkMeshGenerator()
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

        public void AddParsedOBJ(BlockModel blockmodel, Vector3 center_pos, Vector3 vertex_offset, Vector3 scale)
        {
            //Console.WriteLine("Adding Parsed OBJ");
            ParsedOBJ? obj = ModelHandler.GetModel(blockmodel.model_file);
            if (obj != null)
            {
                UV tex_uv = TextureHandler.GetTextureUV(blockmodel.model_texture);
                var _size = obj.face_count;
                //Console.WriteLine(_size);
                for (int i = 0; i < _size; i++)
                {
                    var vertex = obj.GetVertex(i);

                    var uv = new Vector2(
                        Raymath.Lerp(tex_uv.XMIN, tex_uv.XMAX, vertex.UV.X),
                        Raymath.Lerp(tex_uv.YMIN, tex_uv.YMAX, vertex.UV.Y)
                    );
                    AddVertex((vertex.Position * scale) + center_pos + vertex_offset, vertex.Normal, uv, Color.White);
                }
            }
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

        public void AddBlockFace(Vector3 cbp_v,Block blockdef, Int3 normal,bool _flipped = false)
        {
            if (normal == Globals.block_normals[5])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Top);
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Bottom);
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
            }
            else if (normal == Globals.block_normals[4])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Bottom);
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Top);
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                }
            }
            else if (normal == Globals.block_normals[2])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Forward);
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Backward);
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
            }
            else if (normal == Globals.block_normals[3])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Backward);
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Forward);
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
            }
            else if (normal == Globals.block_normals[1])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Right);
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Left);
                    AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
            }
            else if (normal == Globals.block_normals[0])
            {
                if (!_flipped)
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Left);
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                }
                else
                {
                    UV bUv = blockdef.GetBlockUV(BlockFace.Right);
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.IsTranslucent());

                    AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.IsTranslucent());
                    AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.IsTranslucent());
                }
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

        public int GenerateMeshT()
        {
            if (normalsT.Count != vertsT.Count || uvsT.Count != vertsT.Count || vertcolorsT.Count != vertsT.Count)
            {
                Console.WriteLine("INCONSISTANT VERTEX DATA");
                return 2;
            }

            vertcountT = vertsT.Count;

            if (vertcountT <= 2)
            {
                Console.WriteLine("Not building empty mesh! (translucent)");
                DrawableT = false;
                return 1;
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

            return 0;
        }
        public int GenerateMesh()
        {
            if (normals.Count != verts.Count || uvs.Count != verts.Count || vertcolors.Count != verts.Count)
            {
                Console.WriteLine("INCONSISTANT VERTEX DATA");
                return 2;
            }

            vertcount = verts.Count;

            if (vertcount <= 2)
            {
                Console.WriteLine("Not building empty mesh!");
                Drawable = false;
                return 1;
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

            return 0;
        }
    }
}
