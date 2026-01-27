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
using System.Reflection.Metadata;

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

        private Vector3 last_player_position;
        private Int3 last_player_chunk_position;

        private Dictionary<Int3, Chunk> chunks;
        private FlipList<Int3> chunk_upload_list;

        private Dictionary<string, Entity> entities;
        private Dictionary<string, Entity> garbage_entities;
        private string focused_entity_id = "-1";

        private FastNoiseLite fnl;
        
        private AutoResetEvent upload_wait = new AutoResetEvent(false);
        private bool upload_running = false;

        private AutoResetEvent blockchange_wait = new AutoResetEvent(false);
        private bool blocks_changing = false;
        private int block_changes_being_made = 0;
        private List<Int3> blocks_changed_chunks;

        public Camera3D cam;

        Material chunk_material;
        int shader_uniform_texture_pos;
        int shader_uniform_camera_pos_pos;

        //Texture2D shadtex;
        public World(WorldInfo world_info)
        {
            info = world_info;
            fnl = new FastNoiseLite(info.world_seed);
            fnl.SetFrequency(0.02f);

            chunk_material = Raylib.LoadMaterialDefault();
            Shader shad = Raylib.LoadShader("Assets/Shaders/terrain_shader.vs", "Assets/Shaders/terrain_shader.fs");
            shader_uniform_texture_pos = Raylib.GetShaderLocation(shad, "atlastexture");
            shader_uniform_camera_pos_pos = Raylib.GetShaderLocation(shad, "camera_pos");
            chunk_material.Shader = shad;

            //Raylib.SetMaterialTexture(ref chunk_material, MaterialMapIndex.Albedo, Raylib.LoadTexture("Assets/blocktextureatlas.png"));

            entities = new Dictionary<string, Entity>();
            garbage_entities = new Dictionary<string, Entity>();

            chunks = new Dictionary<Int3, Chunk>();
            chunk_upload_list = new FlipList<Int3>(false);

            blocks_changed_chunks = new List<Int3>();

            cam = new Camera3D();
            cam.Position = Vector3.Zero;
            cam.Target = new Vector3(1, 0, 0);
            cam.Up = new Vector3(0, 0, 1);
            cam.FovY = 70f;
            cam.Projection = CameraProjection.Perspective;

            var zero = new Int3(0, 0, 0);

            Raylib.SetRandomSeed((uint)info.world_seed);
        }

        #region ENTITY SYSTEM
        public bool AddEntity(Entity entity, bool is_focus = false)
        {
            if (!entities.ContainsKey(entity.GetID()))
            {
                entities.Add(entity.GetID(), entity);

                entity.Start();

                if (is_focus)
                {
                    focused_entity_id = entity.GetID();
                }
                return true;
            }
            return false;
        }

        public bool DestroyEntity(string ID)
        {
            if (entities.ContainsKey(ID))
            {
                var entity = entities[ID];
                entity.End();

                entities.Remove(ID);
                garbage_entities.Add(ID, entity); // add to garbage to be destroyed at the end of the cycle

                return true;
            }
            return false;
        }

        public Entity? GetFocusEntity()
        {
            if (entities.ContainsKey(focused_entity_id))
            {
                return entities[focused_entity_id];
            }
            return null;
        }

        public void UpdateEntities()
        {
            var deltatime = Globals.GetDelta();
            if (entities.ContainsKey(focused_entity_id))
            {
                var _focused_entity = entities[focused_entity_id];
                _focused_entity.Update(deltatime);
            }

            foreach (Entity entity in entities.Values)
            {
                if (entity.GetID() != focused_entity_id)
                {
                    entity.Update(deltatime);
                }
            }

            //destroy garbage

            foreach (Entity entity in garbage_entities.Values)
            {
                GC.SuppressFinalize(entity);
            }
            garbage_entities.Clear();
        }

        public void DrawEntities()
        {
            foreach (Entity entity in entities.Values)
            {
                entity.Draw();
            }
        }

        #endregion

        #region CHUNK SYSTEM
        public Int3 WBP_to_ChunkPos(Int3 world_block_pos)
        {
            return new Int3(
                (int)Math.Floor((float)world_block_pos.x / (float)Globals.chunk_size.x),
                (int)Math.Floor((float)world_block_pos.y / (float)Globals.chunk_size.y),
                (int)Math.Floor((float)world_block_pos.z / (float)Globals.chunk_size.z)
            );
        }
        public Int3 WBP_to_CBP(Int3 world_block_pos)
        {
            return new Int3(
                (int)Globals.better_mod((float)world_block_pos.x, (float)Globals.chunk_size.x),
                (int)Globals.better_mod((float)world_block_pos.y, (float)Globals.chunk_size.y),
                (int)Globals.better_mod((float)world_block_pos.z, (float)Globals.chunk_size.z)
            );
        }
        public float ChunkDistance(Int3 chunk1, Int3 chunk2)
        {
            return Vector3Distance(chunk1.to_vector3(), chunk2.to_vector3());
        }

        public BoxCollider GetBlockCollider(Int3 world_block_pos)
        {
            ChunkBlock? _block = GetBlock(world_block_pos);
            if (_block != null)
            {
                Block blockdef = _block.GetBlockDef();
                BoxCollider c = blockdef.collider;
                c.Position = world_block_pos.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f);
                return c;
            }

            return new BoxCollider(world_block_pos.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
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
            ChunkBlock? FoundBlock = null;
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

                ChunkBlock? block = GetBlock(new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z));
                if (block != null)
                {
                    Block blockdef = block.GetBlockDef();
                    if (blockdef.IsExists() && !blockdef.IsNonSolid())
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

            if (TileFound && FoundBlock != null)
            {
                Vector3 Intersection = RayStart + Direction * TraveledDistance;
                result = new RaycastResult(Intersection, new Int3((int)LastMove.X, (int)LastMove.Y, (int)LastMove.Z), FoundBlock, new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z), GetBlockCollider(new Int3((int)MapCheck.X, (int)MapCheck.Y, (int)MapCheck.Z)));
            }

            return result;
        }

        public Chunk? GetChunk(Int3 chunk_pos)
        {
            Chunk? _val = null;
            chunks.TryGetValue(chunk_pos, out _val);
            return _val;
        }

        public ChunkBlock? GetBlock(Int3 world_block_pos)
        {
            Int3 blockpos = WBP_to_CBP(world_block_pos);
            Int3 chunkpos = WBP_to_ChunkPos(world_block_pos);

            Chunk? chunk = GetChunk(chunkpos);
            if (chunk != null)
            {
                return chunk.map.Get(blockpos);
            }
            return null;
        }

        public void StartBlockChange()
        {
            blocks_changing = true;
            block_changes_being_made += 1;
        }

        public void CommitBlockChange()
        {
            block_changes_being_made -= 1;
            if (block_changes_being_made <= 0)
            {
                Task.Run(() =>
                {
                    foreach (Int3 chunkpos in blocks_changed_chunks)
                    {
                        ChunkBuildMesh(chunkpos);
                    }
                    block_changes_being_made = 0;
                    blockchange_wait.Set();
                    blocks_changed_chunks.Clear();
                    blocks_changing = false;
                });
            }
        }

        private void BlockChangeAddChunk(Int3 chunkpos)
        {
            if (!blocks_changed_chunks.Contains(chunkpos))
            {
                blocks_changed_chunks.Add(chunkpos);
            }
        }

        public bool SetBlock(Int3 world_block_pos,BlockType newblock,bool preform_block_updates = true)
        {
            if (!blocks_changing)
            {
                Console.WriteLine("BLOCK CHANGE NOT STARTED! CANT EDIT WORLD.");
                return false;
            }

            Int3 blockpos = WBP_to_CBP(world_block_pos);
            Int3 chunkpos = WBP_to_ChunkPos(world_block_pos);

            Chunk? chunk = GetChunk(chunkpos);
            if (chunk != null)
            {
                bool chunk_was_changed = chunk.map.Set(blockpos, newblock);

                if (chunk_was_changed)
                {
                    if (newblock == BlockType.AirBlock)
                    {
                        AddEntity(new DroppedItemEntity(this, "DroppedItem", world_block_pos.to_vector3() + (Vector3.One * 0.5f) + new Vector3(Raylib.GetRandomValue(-50, 50) / 100f, Raylib.GetRandomValue(-50, 50) / 100f, Raylib.GetRandomValue(-50, 50) / 100f)));
                    }

                    BlockChangeAddChunk(chunkpos);
                    foreach (Int3 norm in Globals.block_normals)
                    {
                        Int3 at_chunkpos = WBP_to_ChunkPos(world_block_pos + norm);
                        if (at_chunkpos != chunkpos)
                        {
                            BlockChangeAddChunk(at_chunkpos);
                        }

                        if (preform_block_updates)
                        {
                            //BLOCK UPDATE
                            ChunkBlock? at_block = GetBlock(world_block_pos + norm);
                            if (at_block != null && at_block.active)
                            {
                                Block block_def = at_block.GetBlockDef();
                                block_def.OnNearBlockChanged(this, world_block_pos + norm, at_block.block, world_block_pos);
                            }
                        }
                    }
                }

                return true;
            }
            return false;
        }

        public bool BreakBlock(Int3 world_block_pos)
        {
            StartBlockChange();
            bool did = SetBlock(world_block_pos, BlockType.AirBlock);
            CommitBlockChange();
            return did;
        }

        public bool PlaceBlock(Int3 world_block_pos, BlockType block_to_place)
        {
            StartBlockChange();
            bool did = SetBlock(world_block_pos, block_to_place);
            CommitBlockChange();
            return did;
        }

        public void SetBlockActive(Int3 world_block_pos, bool active)
        {
            ChunkBlock? block = GetBlock(world_block_pos);
            if (block != null)
            {
                block.active = !block.active;
            }
        }

        public bool ChunkExists(Int3 chunk_pos)
        {
            return chunks.ContainsKey(chunk_pos);
        }

        public bool ChunkCreate(Int3 chunk_pos)
        {
            if (chunks.ContainsKey(chunk_pos))
            {
                return false;
            }

            chunks.Add( chunk_pos, new Chunk(chunk_pos) );

            return true;
        }

        public void ChunkBuildMesh(Int3 chunk_pos,bool _frominside = false)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                chunk.generator.Clear();
                for (int idx = 0; idx < chunk.map.fullsize; idx++)
                {
                    var CBP = chunk.map.IndexToPosition(idx);
                    var WBP = (chunk_pos * Globals.chunk_size) + CBP;
                    var CBP_v = CBP.to_vector3();

                    ChunkBlock? block = GetBlock(WBP);
                    if (block == null) { return; }
                    Block block_def = block.GetBlockDef();

                    if (!block_def.IsExists() || block_def.IsTranslucent())
                    {
                        bool enclosed = true;
                        if (block_def.IsTranslucent())
                        {
                            foreach (Int3 norm in Globals.block_normals)
                            {
                                var at_WBP = WBP + norm;

                                ChunkBlock? at_block = GetBlock(at_WBP);
                                if (at_block == null) { break; }
                                Block at_block_def = at_block.GetBlockDef();
                                if (!at_block_def.IsExists() || at_block_def.IsTranslucent())
                                {
                                    enclosed = false;
                                    break;
                                }
                            }
                        }

                        foreach (Int3 norm in Globals.block_normals)
                        {
                            var at_WBP = WBP + norm;

                            ChunkBlock? at_block = GetBlock(at_WBP);
                            if (at_block == null) { return; }
                            Block at_block_def = at_block.GetBlockDef();

                            if (at_block_def.IsExists())
                            {
                                if (block != at_block)
                                {
                                    if ((!block_def.IsTranslucent() && at_block_def.BlockModel == null) || (block_def.IsTranslucent() && !enclosed))
                                    {
                                        chunk.generator.AddBlockFace(CBP_v, at_block_def, norm, true);
                                    }
                                }
                            }
                        }

                        //model block
                        if (block_def.BlockModel != null && !enclosed)
                        {
                            chunk.generator.AddParsedOBJ(block_def.BlockModel, CBP_v + (Vector3.One * 0.5f), block_def.BlockModel.model_offset, block_def.BlockModel.model_scale);
                        }
                    }
                }

                if (upload_running && !_frominside)
                {
                    upload_wait.WaitOne();
                }

                chunk_upload_list.Add(chunk_pos);
            }
            
        }

        public void GenerateArea()
        {
            var size = 3;
            var buildsize = size - 1;

            Console.WriteLine("GENERATING...");

            for (int x = -size; x<=size; x++)
            {
                for (int y = -size; y <=size; y++)
                {
                    for (int z = -size; z <=size; z++)
                    {
                        var pos = new Int3(x, y, z);
                        bool created = ChunkCreate(pos);
                        if (created)
                        {
                            ChunkGenerate(pos);
                        }
                    }
                }
            }

            Console.WriteLine("BUILDING...");

            for (int x = -buildsize; x <= buildsize; x++)
            {
                for (int y = -buildsize; y <= buildsize; y++)
                {
                    for (int z = -buildsize; z <= buildsize; z++)
                    {
                        var pos = new Int3(x, y, z);
                        Chunk? chunk = GetChunk(pos);
                        if (chunk != null && chunk.WillBuild())
                        {
                            ChunkBuildMesh(pos);
                        }
                    }
                }
            }

            Console.WriteLine("Generation Cycle Finished!");
        }

        public void UploadChunks()
        {
            upload_running = true;
            chunk_upload_list.Flip();

            List<Int3> upload_list = chunk_upload_list.GetInactiveList();
            var _amt = upload_list.Count;

            foreach (Int3 chunk_pos in upload_list)
            {
                Chunk? chunk = GetChunk(chunk_pos);

                if (chunk != null)
                {
                    int built = chunk.UploadMeshes();
                    if (built == 1) //inconsistant error code
                    {
                        ChunkBuildMesh(chunk_pos,true);
                    }
                }
            }
            upload_list.Clear();

            if (_amt > 0)
            {
                Console.WriteLine("Upload Cycle Finished (" + _amt.ToString() + ")");
            }

            upload_wait.Set();
            upload_running = false;
        }

        public void ChunkGenerate(Int3 chunk_pos)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                for (int idx = 0; idx < chunk.map.fullsize; idx++)
                {
                    var CBP = chunk.map.IndexToPosition(idx);
                    var WBP = (chunk_pos * Globals.chunk_size) + CBP;
                    var v = BlockType.GreyStoneBlock;

                    if (fnl.GetNoise(WBP.x, WBP.y, WBP.z) > 0.5f)
                    {
                        v = BlockType.AirBlock;
                    }
                    else
                    {
                        if (Raylib.GetRandomValue(0,256)==256)
                        {
                            v = BlockType.MineBlock;
                        }
                    }

                    chunk.map.Set(CBP, v);
                }
            }
        }
        public void DrawAllChunks()
        {
            Raylib.SetShaderValueTexture(chunk_material.Shader, shader_uniform_texture_pos, TextureHandler.block_atlas.Texture);
            var buildsize = 2;
            for (int x = -buildsize; x <= buildsize; x++)
            {
                for (int y = -buildsize; y <= buildsize; y++)
                {
                    for (int z = -buildsize; z <= buildsize; z++)
                    {
                        var pos = new Int3(x, y, z);
                        Chunk? chunk = GetChunk(pos);
                        if (chunk != null)
                        {
                            if (chunk.generator.Drawable)
                            {
                                Raylib.DrawMesh(chunk.generator.mesh, chunk_material, chunk.transform);
                            }
                            if (chunk.generator.DrawableT)
                            {
                                Raylib.DrawMesh(chunk.generator.meshT, chunk_material, chunk.transform);
                            }
                            Raylib.DrawCubeWires(pos.to_vector3() * Globals.chunk_size.to_vector3(), Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, Color.White);
                        }
                    }
                }
            }
        }

        #endregion

        public void Cleanup()
        {
        }
    }
}
