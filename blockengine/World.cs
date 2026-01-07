using Raylib_cs;
using static Raylib_cs.Raymath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using blockengine.Entitys;

namespace blockengine
{
    public struct WorldInfo
    {
        public string world_name;
        public int world_seed;
        
        public WorldInfo(string _name = "Untitled_world", int _seed = 4206721)
        {
            world_name = _name;
            world_seed = _seed;
        }
    }
    public class World
    {
        public WorldInfo info;
        private WorldGenerator world_generator;

        private Dictionary<string, Chunk> chunks;
        private List<Entity> entities;

        private List<Int3> chunks_to_upload;
        private List<Int3> uploaded_chunks;
        private List<Int3> chunks_to_unload;

        //chunk drawling
        Int3 last_viewable_position;

        Material chunk_material;
        int shader_uniform_texture_pos;
        int shader_uniform_camera_pos_pos;

        private int render_distance = 2;
        private int unload_distance = 4;
        //Texture2D shadtex;
        public World(WorldInfo world_info)
        {
            info = world_info;

            world_generator = new WorldGenerator(info.world_seed);

            world_generator.AddCarver(0.8f, 0.008f, 3);

            var spaghetti_fnl = new FastNoiseLite(info.world_seed);

            spaghetti_fnl.SetFractalType(FastNoiseLite.FractalType.Ridged);
            spaghetti_fnl.SetFractalOctaves(2);
            spaghetti_fnl.SetFrequency(0.007f);

            world_generator.AddCarverEX(0.98f, 0, spaghetti_fnl);

            chunks_to_upload = new List<Int3>();
            uploaded_chunks = new List<Int3>();
            chunks_to_unload = new List<Int3>();

            chunk_material = Raylib.LoadMaterialDefault();
            Shader shad = Raylib.LoadShader("Assets/Shaders/terrain_shader.vs", "Assets/Shaders/terrain_shader.fs");
            shader_uniform_texture_pos = Raylib.GetShaderLocation(shad, "atlastexture");
            shader_uniform_camera_pos_pos = Raylib.GetShaderLocation(shad, "camera_pos");
            chunk_material.Shader = shad;

            //Raylib.SetMaterialTexture(ref chunk_material, MaterialMapIndex.Albedo, Raylib.LoadTexture("Assets/blocktextureatlas.png"));

            chunks = new Dictionary<string, Chunk>();
            var zero = new Int3(0, 0, 0);
            
            if (File.Exists(GetChunkSaveDirectory(zero)))
            {
                LoadChunk(zero);
            }
            else
            {
                AddChunk(zero);
                Generate(zero);
            }
        }
        //public int PositionToIndex(Vector3 pos)
        //{
        //    return ((int)pos.Z * width * height) + ((int)pos.Y * width) + (int)pos.X;
        //}

        //public Vector3 IndexToPosition(int idx)
        //{
        //    int x, y, z;
        //    x = idx % width;
        //    y = (idx / width) % height;
        //    z = idx / (width * height);
        //    return new Vector3(x, y, z);
        //}

        public string GetChunkID(Int3 pos)
        {
            return pos.x.ToString() + pos.y.ToString() + pos.z.ToString();
        }


        public string GetChunkSaveDirectory(Int3 pos)
        {
            return "Saves/" + info.world_name + "/chunk" + pos.x + "_" + pos.y + "_" + pos.z;
        }
        public bool OutOfBounds(Int3 pos)
        {
            return !chunks.ContainsKey(GetChunkID(pos));
            //return (pos.X < 0 || pos.X > width - 1 || pos.Y < 0 || pos.Y > height - 1 || pos.Z < 0 || pos.Z > depth - 1);
        }

        //World Functions
        public Chunk GetChunk(Int3 pos)
        {
            var id = GetChunkID(pos);
            if (chunks.ContainsKey(id))
            {
                return chunks[id];
            }
            return null;
        }

        private void AddChunk(Int3 pos)
        {
            string id = GetChunkID(pos);
            if (!chunks.ContainsKey(id))
            {
                chunks[id] = new Chunk(pos);
            }
        }

        private void RemoveChunk(Int3 pos)
        {
            string id = GetChunkID(pos);
            if (chunks.ContainsKey(id))
            {
                chunks.Remove(id);
                if (uploaded_chunks.Contains(pos))
                {
                    uploaded_chunks.Remove(pos);
                }
                if (chunks_to_upload.Contains(pos))
                {
                    chunks_to_upload.Remove(pos);
                }
                //Console.WriteLine("Removed chunk");
            }
        }

        public void SaveChunk(Int3 pos)
        {
            Chunk chunk = GetChunk(pos);

            if (chunk != null)
            {
                if (!Directory.Exists("Saves/" + info.world_name + "/"))
                {
                    Directory.CreateDirectory("Saves/" + info.world_name + "/");
                }
                string chunk_file_path = GetChunkSaveDirectory(pos);

                FileStream chunk_file = File.Create(chunk_file_path);
                chunk_file.Write(chunk.map.too_bytes(), 0, chunk.map.fullsize);
                chunk_file.Close();

                Console.WriteLine("saved chunk : " + GetChunkID(pos));
            }
        }

        private void LoadChunk(Int3 pos)
        {
            if (Directory.Exists("Saves/" + info.world_name + "/"));
            {
                string chunk_file_path = GetChunkSaveDirectory(pos);

                if (File.Exists(chunk_file_path))
                {
                    if (!OutOfBounds(pos))
                    {
                        UnloadChunk(pos, false);
                    }
                    AddChunk(pos);
                    Chunk chunk = GetChunk(pos);

                    chunk.status = Chunk.chunk_generation_status.generating;

                    byte[] bytes = File.ReadAllBytes(chunk_file_path);
                    for (int i = 0; i < chunk.map.fullsize; i++)
                    {
                        chunk.map.Set(chunk.map.IndexToPosition(i), (int)bytes[i]);
                    }
                    //Console.WriteLine("");
                    Console.WriteLine("Loaded chunk: " + chunk_file_path);

                    chunk.status = Chunk.chunk_generation_status.generated;
                }
            }
            
        }

        private void UnloadChunk(Int3 pos,bool save = true) //remove chunk from world safely
        {
            Chunk chunk = GetChunk(pos);
            if (chunk != null)
            {
                if (chunk.player_edited && save)
                {
                    SaveChunk(pos);
                }
                chunk.UnloadMesh();
                RemoveChunk(pos);

                Console.WriteLine("unloaded chunk: " + GetChunkID(pos));
            }
        }

        public Int3 GetChunkPosWorldPos(Int3 world_block_pos)
        {
            return new Int3(
                (int)Math.Floor((float)world_block_pos.x / (float)Globals.chunk_size.x),
                (int)Math.Floor((float)world_block_pos.y / (float)Globals.chunk_size.y),
                (int)Math.Floor((float)world_block_pos.z / (float)Globals.chunk_size.z)
            );
        }

        public Int3 GetChunkBlockPosWorldPos(Int3 world_block_pos)
        {
            return new Int3(
                (int)Globals.better_mod((float)world_block_pos.x, (float)Globals.chunk_size.x),
                (int)Globals.better_mod((float)world_block_pos.y, (float)Globals.chunk_size.y),
                (int)Globals.better_mod((float)world_block_pos.z, (float)Globals.chunk_size.z)
            );
        }

        public int GetLoadedChunksCount()
        {
            return chunks.Count();
        }

        public float ChunkDistance(Int3 chunk1, Int3 chunk2)
        {
             return Vector3Distance(chunk1.to_vector3(), chunk2.to_vector3());
        }

        public int GetBlock(Int3 world_block_pos)
        {
            Int3 chunk_pos = GetChunkPosWorldPos(world_block_pos);
            Chunk chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                Int3 chunk_block_pos = GetChunkBlockPosWorldPos(world_block_pos);
                return chunk.map.Get(chunk_block_pos);
            }
            return -1;
        }
        public void SetBlock(Int3 world_block_pos,int setto,bool build_chunks = true)
        {
            Int3 chunk_pos = GetChunkPosWorldPos(world_block_pos);
            Chunk chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                Int3 chunk_block_pos = GetChunkBlockPosWorldPos(world_block_pos);
                Console.WriteLine(chunk_block_pos.x + ", " + chunk_block_pos.y + ", " + chunk_block_pos.z);
                Console.WriteLine(chunk_pos.x + ", " + chunk_pos.y + ", " + chunk_pos.z);
                chunk.map.Set(chunk_block_pos, setto);
                chunk.player_edited = true;

                if (build_chunks)
                {
                    bool left = chunk_block_pos.x == 0;
                    bool right = chunk_block_pos.x == Globals.chunk_size.x - 1;

                    bool forward = chunk_block_pos.y == 0;
                    bool backward = chunk_block_pos.y == Globals.chunk_size.y - 1;

                    bool bottom = chunk_block_pos.z == 0;
                    bool top = chunk_block_pos.z == Globals.chunk_size.z - 1;

                    Task.Run(() => {
                        BuildChunk(chunk_pos, false,true,true);

                        if (left)
                        {
                            BuildChunk(chunk_pos + new Int3(-1, 0, 0), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(-1, 0, 0));
                        }
                        if (right)
                        {
                            BuildChunk(chunk_pos + new Int3(1, 0, 0), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(1, 0, 0));
                        }
                        if (forward)
                        {
                            BuildChunk(chunk_pos + new Int3(0, -1, 0), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(0, -1, 0));
                        }
                        if (backward)
                        {
                            BuildChunk(chunk_pos + new Int3(0, 1, 0), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(0, 1, 0));
                        }
                        if (bottom)
                        {
                            BuildChunk(chunk_pos + new Int3(0, 0, -1), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(0, 0, -1));
                        }
                        if (top)
                        {
                            BuildChunk(chunk_pos + new Int3(0, 0, 1), false, true, true);
                            //build_list.Add(chunk_pos + new Vector3(0, 0, 1));
                        }
                    });
                }
            }
        }

        public bool GetBlockExists(int block)
        {
            if (block == -1)
            {
                return false;
            }
            BlockDefinition? bd = Globals.BlockDefinitions[block];
            if (bd != null)
            {
                return bd.Exists;
            }
            return false;
        }

        public BoxCollider GetBlockCollider(Int3 world_block_pos)
        {
            BlockDefinition? blockdef = Globals.BlockDefinitions[GetBlock(world_block_pos)];
            if (blockdef != null)
            {
                BoxCollider c = blockdef.Collider;
                c.Position = world_block_pos.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f);
                return c;
            }
            return new BoxCollider(world_block_pos.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f,-0.5f,-0.5f),new Vector3(0.5f,0.5f,0.5f));
        }

        public RaycastResult? Raycast(Vector3 RayStart, Vector3 Direction)
        {
            RaycastResult? result = null;

            float MaxTravelDistance = Direction.Length();
            Direction = Vector3.Normalize(Direction);

            float absDirX = MathF.Abs(Direction.X);
            float absDirY = MathF.Abs(Direction.Y);
            float absDirZ = MathF.Abs(Direction.Z);

            Vector3 RayStepSize = new Vector3(
                MathF.Sqrt(1 + (absDirY / absDirX) * (absDirY / absDirX) + (absDirZ / absDirX) * (absDirZ / absDirX)),
                MathF.Sqrt(1 + (absDirX / absDirY) * (absDirX / absDirY) + (absDirZ / absDirY) * (absDirZ / absDirY)),
                MathF.Sqrt(1 + (absDirX / absDirZ) * (absDirX / absDirZ) + (absDirY / absDirZ) * (absDirY / absDirZ))
            );

            Vector3 MapCheck = new Vector3((int)RayStart.X, (int)RayStart.Y, (int)RayStart.Z);
            Vector3 RayLength1D;
            Vector3 Vstep;

            if (Direction.X < 0)
            {
                Vstep.X = -1;
                RayLength1D.X = (RayStart.X - MapCheck.X) * RayStepSize.X;
            }
            else
            {
                Vstep.X = 1;
                RayLength1D.X = ((MapCheck.X + 1) - RayStart.X) * RayStepSize.X;
            }

            if (Direction.Y < 0)
            {
                Vstep.Y = -1;
                RayLength1D.Y = (RayStart.Y - MapCheck.Y) * RayStepSize.Y;
            }
            else
            {
                Vstep.Y = 1;
                RayLength1D.Y = ((MapCheck.Y + 1) - RayStart.Y) * RayStepSize.Y;
            }

            if (Direction.Z < 0)
            {
                Vstep.Z = -1;
                RayLength1D.Z = (RayStart.Z - MapCheck.Z) * RayStepSize.Z;
            }
            else
            {
                Vstep.Z = 1;
                RayLength1D.Z = ((MapCheck.Z + 1) - RayStart.Z) * RayStepSize.Z;
            }

            bool TileFound = false;
            float TraveledDistance = 0;
            int FoundBlock = -1;
            Vector3 LastMove = Vector3.Zero;
            while (!TileFound && TraveledDistance < MaxTravelDistance)
            {
                Vector3 MapCheckBefore = MapCheck;
                if (RayLength1D.X < RayLength1D.Y)
                {
                    if (RayLength1D.X < RayLength1D.Z)
                    {
                        MapCheck.X += Vstep.X;
                        TraveledDistance = RayLength1D.X;
                        RayLength1D.X += RayStepSize.X;
                    }
                    else
                    {
                        MapCheck.Z += Vstep.Z;
                        TraveledDistance = RayLength1D.Z;
                        RayLength1D.Z += RayStepSize.Z;
                    }
                }
                else
                {
                    if (RayLength1D.Z < RayLength1D.Y)
                    {
                        MapCheck.Z += Vstep.Z;
                        TraveledDistance = RayLength1D.Z;
                        RayLength1D.Z += RayStepSize.Z;
                    }
                    else
                    {
                        MapCheck.Y += Vstep.Y;
                        TraveledDistance = RayLength1D.Y;
                        RayLength1D.Y += RayStepSize.Y;
                    }
                }
                LastMove = MapCheckBefore - MapCheck;

                int block = GetBlock(new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z));
                if (block > -1)
                {
                    BlockDefinition? blockdef = Globals.BlockDefinitions[block];
                    if (blockdef != null && blockdef.Exists && !blockdef.NonSolid)
                    {
                        TileFound = true;
                        FoundBlock = block;
                    }
                }
                else
                {
                    break;
                }
            }

            if (TileFound)
            {
                Vector3 Intersection = RayStart + Direction * TraveledDistance;
                result = new RaycastResult(Intersection, new Int3((int)LastMove.X, (int)LastMove.Y, (int)LastMove.Z), FoundBlock, new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z), GetBlockCollider(new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z)));
            }

            return result;
        }

        //CHUNK FUNCTIONS
        public void Draw(Camera3D cam, int distance,bool drawdebug = false)
        {
            Int3 start_chunk_pos = Globals.WorldPosToChunkPos(cam.Position);
            last_viewable_position = start_chunk_pos;

            for (int z = -distance; z<=distance; z++)
            {
                for (int y = -distance; y <= distance; y++)
                {
                    for (int x = -distance; x <= distance; x++)
                    {
                        Int3 chunkpos = start_chunk_pos + new Int3(x, y, z);

                        Chunk chunk = GetChunk(chunkpos);
                        if (chunk != null)
                        {
                            
                            Raylib.SetShaderValueTexture(chunk_material.Shader, shader_uniform_texture_pos, TextureHandler.block_atlas.Texture);
                            Raylib.SetShaderValue(chunk_material.Shader, shader_uniform_camera_pos_pos, last_viewable_position, ShaderUniformDataType.Vec3);
                            if (chunk.generator.Drawable)
                            {
                                Raylib.DrawMesh(chunk.generator.mesh, chunk_material, chunk.transform);
                            }
                            if (chunk.generator.DrawableT)
                            {
                                Raylib.DrawMesh(chunk.generator.meshT, chunk_material, chunk.transform);
                            }
                            if (drawdebug)
                            {
                                Raylib.DrawCubeWires(((chunkpos * Globals.chunk_size) + (Globals.chunk_size / 2)).to_vector3(), Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, Color.White);
                            }
                        }
                    }
                }
            }
        }
        private bool BuildChunk(Int3 chunkpos,bool and_upload = false,bool force = false,bool important = false)
        {
            Chunk chunk = GetChunk(chunkpos);
            if (chunk != null)
            {
                if (!chunk.map.changed && !force)
                {
                    return false;
                }
                //if (chunk.WontBuild())
                //{
                //    Console.WriteLine("Not attempting to build air chunk.");
                //   chunk.map.changed = false;
                //    return false;
                //}

                lock (chunk)
                {
                    Int3 world_chunk_pos = chunkpos * Globals.chunk_size;
                    chunk.generator.Clear();
                    //chunk.generator.UnloadMesh();

                    for (int b = 0; b < chunk.map.fullsize; b++)
                    {

                        Int3 chunk_block_pos = chunk.map.IndexToPosition(b);
                        Int3 world_block_pos = world_chunk_pos + chunk_block_pos;
                        Vector3 cbp_v = chunk_block_pos.to_vector3();

                        int block = chunk.map.Get(chunk_block_pos);
                        BlockDefinition? blockdef = Globals.BlockDefinitions[block];

                        if (blockdef != null && blockdef.Exists)
                        {
                            foreach (Int3 normal in Globals.block_normals)
                            {
                                int atblock = GetBlock(world_block_pos + normal);
                                
                                BlockDefinition? atblockdef = Globals.BlockDefinitions[atblock];

                                if (atblockdef != null && (!atblockdef.Exists || atblockdef.Translucent))
                                {
                                    bool draw = true;
                                    if (blockdef.Translucent)
                                    {
                                        draw = !(block == atblock);
                                    }
                                    if (draw)
                                    {
                                        if (normal == Globals.block_normals[5])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Top);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                        }
                                        else if (normal == Globals.block_normals[4])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Bottom);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, 0, -1), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                        }
                                        else if (normal == Globals.block_normals[2])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Forward);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(0, 1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                        }
                                        else if (normal == Globals.block_normals[3])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Backward);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(0, -1, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(0, -1, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                        }
                                        else if (normal == Globals.block_normals[1])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Right);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(1, 1, 0), new Vector3(1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                        }
                                        else if (normal == Globals.block_normals[0])
                                        {
                                            UV bUv = blockdef.GetBlockUV(BlockFace.Left);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);

                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMIN), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMIN, bUv.YMAX), Color.White, blockdef.Translucent);
                                            chunk.generator.AddVertex(cbp_v + new Vector3(0, 1, 1), new Vector3(-1, 0, 0), new Vector2(bUv.XMAX, bUv.YMAX), Color.White, blockdef.Translucent);
                                        }
                                    }
                                }
                            }
                            /*
                            if (topblockdef != null && (!topblockdef.Exists || topblockdef.Translucent) )
                            {
                                bool draw = true;
                                if (blockdef.Translucent)
                                {
                                    draw = !(block == topblock);
                                }
                                if (draw)
                                {

                                }
                            }
                            */
                        }
                    }

                    //chunk.generator.GenerateMesh();
                    //chunk.generator.GenerateMeshT();
                    chunk.map.changed = false;

                    Console.WriteLine("MESHING: ");
                    chunkpos.Print();
                }

                if (and_upload)
                {
                    chunk.UploadMeshes();
                }
                else
                {
                    if (!chunks_to_upload.Contains(chunkpos))
                    {
                        if (important)
                        {
                            chunks_to_upload.Insert(0, chunkpos);
                        }
                        else
                        {
                            chunks_to_upload.Add(chunkpos);
                        }
                    }
                }

                Console.WriteLine("Meshed Chunk");
            }

            return true;
        }
        public void GenerateAroundFlood(Vector3 position)
        {
            int distance = render_distance;

            Int3 chunk_pos = Globals.WorldPosToChunkPos(position);

            ///UNLOAD OLD CHUNKS
            foreach (string id in chunks.Keys)
            {
                Chunk unload_chunk = chunks[id];
                Int3 unload_chunkpos = unload_chunk.chunkpos;

                if (!chunks_to_unload.Contains(unload_chunkpos))
                {
                    float dist = ChunkDistance(unload_chunkpos, chunk_pos);
                    if (dist > unload_distance)
                    {
                        chunks_to_unload.Add(unload_chunkpos);
                    }
                }
            }

            //load new ones
            var lists = new List<Int3>[2]
            {
                new List<Int3>(),
                new List<Int3>()
            };
            int list_id = 1;
            lists[0].Add(chunk_pos);

            Chunk start_chunk = GetChunk(chunk_pos);

            int d = 0;
            while(d < distance)
            {
                foreach (Int3 pos in lists[1 - list_id])
                {
                    lock (chunks)
                    {
                        if (OutOfBounds(pos))
                        {
                            if (File.Exists(GetChunkSaveDirectory(pos)))
                            {
                                LoadChunk(pos);
                            }
                            else
                            {
                                AddChunk(pos);
                                Generate(pos);
                            }
                        }
                    }
                    Chunk Current_chunk = GetChunk(pos);

                    foreach (Int3 normal in Globals.around_positions)
                    {
                        lock (chunks)
                        {
                            if (OutOfBounds(pos + normal))
                            {
                                if (File.Exists(GetChunkSaveDirectory(pos + normal)))
                                {
                                    LoadChunk(pos + normal);
                                }
                                else
                                {
                                    AddChunk(pos + normal);
                                    Generate(pos + normal);
                                }
                            }
                            //Chunk next_chunk = GetChunk(pos + normal);
                        }
                        bool add = true;

                        if (start_chunk != null)
                        {
                            add = false;
                            if (start_chunk.WillBuild() && Current_chunk.WillBuild())
                            {
                                add = true;
                            }
                            if (start_chunk.WontBuild())
                            {
                                add = true;
                            }
                        }

                        if (add)
                        {
                            lists[list_id].Add(pos + normal);
                        }
                    }

                    lock ( chunks )
                    {
                        if (!uploaded_chunks.Contains(pos))
                        {
                            BuildChunk(pos);
                        }
                    }

                    if (last_viewable_position != chunk_pos)
                    {
                        break;
                    }
                }

                list_id = 1 - list_id;
                lists[list_id].Clear();

                d++;

                
            }

            //Console.WriteLine("DONE GENERATING AROUND");
        }

        public void UploadChunkQueue()
        {
            for (int i = 0; i<3; i++)
            {
                if (chunks_to_unload.Count() > 0)
                {
                    Int3 pos = chunks_to_unload[0];

                    if (ChunkDistance(pos, last_viewable_position) > unload_distance)
                    {
                        UnloadChunk(pos);
                    }

                    chunks_to_unload.RemoveAt(0);
                }

                if (chunks_to_upload.Count() > 0)
                {

                    Int3 pos = chunks_to_upload[0];

                    Chunk chunk = GetChunk(pos);
                    if (chunk != null)
                    {
                        chunk.UploadMeshes();
                        uploaded_chunks.Add(pos);
                    }

                    chunks_to_upload.RemoveAt(0);
                }
            }
        }

        private void Generate(Int3[] chunk_array)
        {
            foreach (Int3 chunkpos in chunk_array)
            {
                Generate(chunkpos);
            }
        }
        private void Generate(Int3 chunkpos)
        {
            Chunk chunk = GetChunk(chunkpos);

            if (chunk != null)
            {
                lock (chunk)
                {
                    chunk.status = Chunk.chunk_generation_status.generating;

                    for (int b = 0; b < chunk.map.fullsize; b++)
                    {
                        Int3 blockpos = chunk.map.IndexToPosition(b);
                        Int3 worldpos = (chunkpos * Globals.chunk_size) + blockpos;
                        
                        int block = world_generator.GetBlock(worldpos);

                        chunk.map.Set(blockpos, block); //
                    }

                    chunk.status = Chunk.chunk_generation_status.generated;
                }
                Console.WriteLine("GENERATING: ");
                chunkpos.Print();
            }
            else
            {
                //Console.WriteLine("Could not generate chunk (" + chunkpos.X + ", " + chunkpos.Y + ", " + chunkpos.Z + ")");
            }
        }

        private bool CanBuildChunk(Int3 chunkpos)
        {
            var left = chunkpos + new Int3(1, 0, 0);
            var right = chunkpos + new Int3(1, 0, 0);
            var up = chunkpos + new Int3(0, -1, 0);
            var down = chunkpos + new Int3(0, 1, 0);
            var forward = chunkpos + new Int3(0, 0, -1);
            var backward = chunkpos + new Int3(0, 0, 1);
            Chunk chunk = GetChunk(chunkpos);
            return (!OutOfBounds(chunkpos) && chunk.status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(left) && GetChunk(left).status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(right) && GetChunk(right).status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(up) && GetChunk(up).status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(down) && GetChunk(down).status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(forward) && GetChunk(forward).status == Chunk.chunk_generation_status.generated) &&
                (!OutOfBounds(backward) && GetChunk(backward).status == Chunk.chunk_generation_status.generated) && chunk.status == Chunk.chunk_generation_status.generated && !chunk.built_meshes;
        }

        private void UnloadAllChunkMeshes()
        {
            foreach (var chunkpos in chunks.Keys)
            {
                //Vector3 chunkpos = IndexToPosition(c);
                Chunk chunk = chunks[chunkpos];
                lock(chunk)
                {
                    chunk.UnloadMesh();
                }
            }
        }
        public void Cleanup()
        {
            // Raylib.UnloadTexture(shadtex);
            foreach (string chunkpos in chunks.Keys)
            {
                Chunk chunk = chunks[chunkpos];
                if (chunk.player_edited)
                {
                    SaveChunk(chunk.chunkpos);
                }
            }
            Raylib.UnloadMaterial(chunk_material);
            UnloadAllChunkMeshes();
        }
    }
}
