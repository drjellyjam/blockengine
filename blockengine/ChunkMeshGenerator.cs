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

        public int last_vertcount = 0;
        public int last_vertcountT = 0;

        public bool mesh_changed = false;
        public bool meshT_changed = false;

        public bool is_generating = false;
        public AutoResetEvent done_generating_event = new AutoResetEvent(false);
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
            last_vertcount = vertcount;
            last_vertcountT = vertcountT;

            Console.WriteLine("Clearing mesh data!!");
            verts.Clear();
            normals.Clear();
            uvs.Clear();
            vertcolors.Clear();
            vertcount = 0;
            mesh_changed = false;

            vertsT.Clear();
            normalsT.Clear();
            uvsT.Clear();
            vertcolorsT.Clear();
            vertcountT = 0;
            meshT_changed = false;
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
                mesh_changed = true;
            }
            else
            {
                vertsT.Add(pos);
                normalsT.Add(norm);
                uvsT.Add(uv);
                vertcolorsT.Add(color);
                meshT_changed = true;
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

        public void UnloadAllMeshes()
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

        public int UploadMeshes()
        {

            //Console.WriteLine("vertex: " + verts.Count + "/" + vertsT.Count);
            //Console.WriteLine("normal: " + normals.Count + "/" + normalsT.Count);
            //Console.WriteLine("uvs: " + uvs.Count + "/" + uvsT.Count);
            //Console.WriteLine("vcolors: " + vertcolors.Count + "/" + vertcolorsT.Count);

            is_generating = true;

            int b1 = 0;
            int b2 = 0;

            vertcount = verts.Count;
            vertcountT = vertsT.Count;

            if (mesh_changed || (last_vertcount > 0 && vertcount == 0))
            {
                b1 = GenerateMesh();
            }
            if (meshT_changed || (last_vertcountT > 0 && vertcountT == 0))
            {
                b2 = GenerateMeshT();
            }

            is_generating = false;
            done_generating_event.Set();

            if (b1 == 2 || b2 == 2)
            {
                return 1;
            }
            return 0;
        }
        public int GenerateMeshT()
        {
            if (!(vertsT.Count == normalsT.Count && normalsT.Count == uvsT.Count && uvsT.Count == vertcolorsT.Count && vertcolorsT.Count == vertsT.Count))
            {
                Console.WriteLine("INCONSISTANT VERTEX DATA");
                return 2;
            }

            if (vertcountT <= 2)
            {
                //Console.WriteLine("Not building empty mesh! (translucent)");
                DrawableT = false;
                return 1;
            }

            if (DrawableT)
            {
                Raylib.UnloadMesh(meshT);
                meshT.VaoId = 0;
                DrawableT = false;
            }

            var _vertcountT = vertcountT;
            var _vertsT = vertsT.ToArray();
            var _normalsT = normalsT.ToArray();
            var _uvsT = uvsT.ToArray();
            var _vertcolorsT = vertcolorsT.ToArray();

            meshT.TriangleCount = _vertcountT / 3;
            meshT.VertexCount = _vertcountT;

            meshT.AllocVertices();
            meshT.AllocNormals();
            meshT.AllocTexCoords();
            meshT.AllocColors();

            for (int i = 0; i < _vertcountT; i++)
            {
                meshT.VerticesAs<Vector3>()[i] = _vertsT[i];
                meshT.NormalsAs<Vector3>()[i] = _normalsT[i];
                meshT.TexCoordsAs<Vector2>()[i] = _uvsT[i];
                meshT.ColorsAs<Color>()[i] = _vertcolorsT[i];
            }

            Raylib.UploadMesh(ref meshT, false);

            DrawableT = true;

            return 0;
        }
        public int GenerateMesh()
        {
            if (!(verts.Count == normals.Count && normals.Count == uvs.Count && uvs.Count == vertcolors.Count && vertcolors.Count == verts.Count))
            {
                Console.WriteLine("INCONSISTANT VERTEX DATA");
                return 2;
            }

            vertcount = verts.Count;

            if (vertcount <= 2)
            {
                //Console.WriteLine("Not building empty mesh!");
                Drawable = false;
                return 1;
            }

            if (Drawable)
            {
                Raylib.UnloadMesh(mesh);
                mesh.VaoId = 0;
                Drawable = false;
            }

            var _vertcount = vertcount;
            var _verts = verts.ToArray();
            var _normals = normals.ToArray();
            var _uvs = uvs.ToArray();
            var _vertcolors = vertcolors.ToArray();

            mesh.TriangleCount = _vertcount / 3;
            mesh.VertexCount = _vertcount;

            mesh.AllocVertices();
            mesh.AllocNormals();
            mesh.AllocTexCoords();
            mesh.AllocColors();

            for (int i = 0; i < _vertcount; i++)
            {
                mesh.VerticesAs<Vector3>()[i] = _verts[i];
                mesh.NormalsAs<Vector3>()[i] = _normals[i];
                mesh.TexCoordsAs<Vector2>()[i] = _uvs[i];
                mesh.ColorsAs<Color>()[i] = _vertcolors[i];
            }

            Raylib.UploadMesh(ref mesh,false);

            Drawable = true;

            return 0;
        }
    }
}
