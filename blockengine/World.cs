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
    public enum WorldType : int
    {
        WorldType_normal,
        WorldType_flat,
        WorldType_void
    }
    public struct WorldInfo 
    {
        public string world_name;
        public int world_seed;
        public WorldType world_type;
        public WorldInfo(string _name = "Untitled_world", int _seed = 4206721, WorldType _type = WorldType.WorldType_normal)
        {
            world_name = _name;
            world_seed = _seed;
            world_type = _type;
        }
    }
    public class World
    {
        public WorldInfo info;

        private Vector3 last_player_position;
        private Int3 last_player_chunk_position;

        private Dictionary<Int3, Chunk> chunks;
        private FlipList<Int3> chunk_unload_list;
        private FlipList<Int3> chunk_upload_list;

        private Dictionary<string, Entity> entities;
        private Dictionary<string, Entity> garbage_entities;
        private string focused_entity_id = "-1";

        private FastNoiseLite fnl;

        private bool blocks_changing = false;
        private int block_changes_being_made = 0;
        private List<Int3> blocks_changed_chunks;
        private FlipList<Int3> scheduled_tick_blocks;

        private Random genrandom;
        public Camera3D cam;

        Material chunk_material;
        int shader_uniform_texture_pos;
        int shader_uniform_camera_pos_pos;
        int shader_uniform_texture_emissive_pos;
        int shader_uniform_fog_color;
        int shader_uniform_fog_mult;
        int render_distance = 2;
        int entities_updated = 0;
        int entities_updated_per_frame = 120;
        public Color fog_color = Color.Black;
        private float[] fog_color_array = new float[3] {0.0f,0.0f,0.0f};

        float tick = 1;
        int ticks = 0;

        //Texture2D shadtex;
        public World(WorldInfo world_info)
        {
            info = world_info;
            fnl = new FastNoiseLite(info.world_seed);
            fnl.SetFrequency(0.02f);

            genrandom = new Random(info.world_seed);

            chunk_material = Raylib.LoadMaterialDefault();
            Shader shad = Raylib.LoadShader("Assets/Shaders/terrain_shader.vs", "Assets/Shaders/terrain_shader.fs");
            shader_uniform_texture_pos = Raylib.GetShaderLocation(shad, "atlastexture");
            shader_uniform_texture_emissive_pos = Raylib.GetShaderLocation(shad, "atlastexture_emissive");
            shader_uniform_fog_color = Raylib.GetShaderLocation(shad, "fog_color");
            shader_uniform_fog_mult = Raylib.GetShaderLocation(shad, "fog_mult");
            shader_uniform_camera_pos_pos = Raylib.GetShaderLocation(shad, "camera_pos");
            chunk_material.Shader = shad;

            //Raylib.SetMaterialTexture(ref chunk_material, MaterialMapIndex.Albedo, Raylib.LoadTexture("Assets/blocktextureatlas.png"));

            entities = new Dictionary<string, Entity>();
            garbage_entities = new Dictionary<string, Entity>();

            chunks = new Dictionary<Int3, Chunk>();
            chunk_upload_list = new FlipList<Int3>(false);
            chunk_unload_list = new FlipList<Int3>(false);

            scheduled_tick_blocks = new FlipList<Int3>(false);
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
                entities_updated += 1;
            }

            for (int i = entities_updated; i < entities_updated + entities_updated_per_frame; i++)
            {
                if (i >= entities.Count)
                {

                    entities_updated = 0;
                    break;
                }

                var entity = entities[entities.ElementAt(i).Key];
                if (entity.GetID() != focused_entity_id)
                {
                    entity.Update(deltatime);
                }
                entities_updated += 1;
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

            Vector3 MapCheck = new Vector3((int)Math.Floor(RayStart.X), (int)Math.Floor(RayStart.Y), (int)Math.Floor(RayStart.Z));
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
            if (chunks.ContainsKey(chunk_pos))
            {
                return chunks[chunk_pos];
            }
            return null;
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
                //Console.WriteLine("Commiting block changes");
                foreach (Int3 chunkpos in blocks_changed_chunks)
                {
                    chunks[chunkpos].needs_rebuilt = true;
                }
                block_changes_being_made = 0;
                blocks_changed_chunks.Clear();
                blocks_changing = false;
                /*
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
                */
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
                    scheduled_tick_blocks.Remove(world_block_pos);

                    if (preform_block_updates)
                    {
                        ChunkBlock? new_block = chunk.map.Get(blockpos);
                        if (new_block != null)
                        {
                            new_block.GetBlockDef().OnBlockInit(this, world_block_pos);
                        }
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
                                block_def.OnNearBlockChanged(this, world_block_pos + norm, newblock, world_block_pos);
                            }
                        }
                    }

                    return true;
                }

                
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

        public float GetBlockData(Int3 world_block_pos,string name)
        {
            ChunkBlock? block = GetBlock(world_block_pos);
            if (block != null && block.HasBlockData(name))
            {
                return block.GetBlockData(name);
            }
            return 0;
        }
        public bool BlockHasData(Int3 world_block_pos, string name)
        {
            ChunkBlock? block = GetBlock(world_block_pos);
            if (block != null)
            {
                return block.HasBlockData(name);
            }
            return false;
        }
        public void SetBlockData(Int3 world_block_pos, string name,float value)
        {
            ChunkBlock? block = GetBlock(world_block_pos);
            if (block != null)
            {
                block.SetBlockData(name,value);
            }
        }

        public void BlockRequestScheduledTick(Int3 world_block_pos)
        {
            ChunkBlock? block = GetBlock(world_block_pos);
            if (block != null && block.active)
            {
                scheduled_tick_blocks.Add(world_block_pos);
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

        public void ChunkRemove(Int3 chunk_pos)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                chunk.UnloadMeshes();
                chunks.Remove(chunk_pos);
            }
        }

        public int ChunkBuildMesh(Int3 chunk_pos,bool _frominside = false)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null && (chunk.needs_rebuilt || chunk.IsChanged() || (chunk.WillBuild() && !chunk.first_built)))
            {
                chunk.map.changed = false;
                chunk.needs_rebuilt = false;
                chunk.first_built = true;

                if (chunk.generator.is_generating)
                {
                    chunk.generator.done_generating_event.WaitOne();
                }

                chunk.generator.Clear();
                for (int idx = 0; idx < chunk.map.fullsize; idx++)
                {
                    var CBP = chunk.map.IndexToPosition(idx);
                    var WBP = (chunk_pos * Globals.chunk_size) + CBP;
                    var CBP_v = CBP.to_vector3();

                    ChunkBlock? block = GetBlock(WBP);
                    if (block == null) { return 2; }
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

                        for (int ni = 0; ni<Globals.block_normals.Length; ni++)
                        {
                            Int3 norm = Globals.block_normals[ni];
                            var at_WBP = WBP + norm;

                            ChunkBlock? at_block = GetBlock(at_WBP);
                            if (at_block == null) { return 3; }
                            Block at_block_def = at_block.GetBlockDef();

                            if (at_block_def.IsExists())
                            {
                                if (block.block != at_block.block)
                                {
                                    if ((!block_def.IsTranslucent() && at_block_def.BlockModel == null) || (block_def.IsTranslucent() && at_block_def.BlockModel == null && !enclosed))
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

                //if (upload_running && !_frominside)
                //{
                //    //upload_wait.WaitOne();
                //}

                
                

                chunk_upload_list.Add(chunk_pos);
                return 1;
            }
            return 0;
        }

        public void UpdateWorld()
        {
            UpdateEntities();
            tick -= Raylib.GetFrameTime();
            if (tick <= 0)
            {
                tick = 0.5f;
                ticks += 1;
                var list = scheduled_tick_blocks.GetInactiveList();
                for (int i = 0; i < list.Count; i++)
                {
                    Int3 WBP = list[i];
                    ChunkBlock? block = GetBlock(WBP);
                    if (block != null)
                    {
                        block.GetBlockDef().OnScheduledTick(this, WBP);
                    }
                }
                list.Clear();
                scheduled_tick_blocks.Flip();
            }
        }

        public void GenerateArea()
        {
            Entity? player = GetFocusEntity();
            if (player != null)
            {
                last_player_position = player.Position;
                last_player_chunk_position = Globals.WorldPosToChunkPos(player.Position);
            }

            var size = render_distance;
            var build_size = size - 1;
            var unoad_size = size + 2;

            //Stopwatch stop = new Stopwatch();
            //stop.Start();

            //var generated_chunks = 0;
            //var built_chunks = 0;

            for (int i = 0; i<Globals.flood_draw_dist_low; i++)
            {
                Int3 floodpos = Globals.flood_positions[i];
                var cp = last_player_chunk_position + floodpos;

                bool created = ChunkCreate(cp);
                if (created)
                {
                    ChunkGenerate_Shaping(cp);
                }
                for (int j = 0; j<Globals.block_normals.Length; j++)
                {
                    var npos = Globals.block_normals[j];
                    bool ncreated = ChunkCreate(cp + npos);
                    if (ncreated)
                    {
                        ChunkGenerate_Shaping(cp + npos);
                    }
                }
                Chunk c = chunks[cp];
                if (c.generation_stage != ChunkGenerationStage.Populated)
                {
                    ChunkGenerate_Population(cp);
                }

                ChunkBuildMesh(cp);
            }

            //stop.Stop();
            //if (stop.ElapsedMilliseconds > 0)
            //{
                //Console.WriteLine(stop.ElapsedMilliseconds);
            //}

            
            foreach (Int3 cpos in chunks.Keys)
            {
                if (cpos.x < last_player_chunk_position.x - unoad_size || cpos.x > last_player_chunk_position.x + unoad_size ||
                    cpos.y < last_player_chunk_position.y - unoad_size || cpos.y > last_player_chunk_position.y + unoad_size ||
                    cpos.z < last_player_chunk_position.z - unoad_size || cpos.z > last_player_chunk_position.z + unoad_size)
                {
                    chunk_unload_list.Add(cpos);
                }
            }
            
            //Console.WriteLine("Generation Cycle Finished!");
        }

        public void UploadChunks()
        {  
            //Console.WriteLine("Uploading chunks");

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
                        Console.WriteLine("ERROR UPLOADING CHUNK");
                    }
                }
            }
            upload_list.Clear();

            chunk_upload_list.Flip();

            if (_amt > 0)
            {
                //Console.WriteLine("Upload Cycle Finished (" + _amt.ToString() + ")");
            }

            //unload

            
            //Console.WriteLine("Unloading chunks");

            List<Int3> unload_list = chunk_unload_list.GetInactiveList();
            var _unload_amt = unload_list.Count;

            foreach (Int3 chunk_pos in unload_list)
            {
                ChunkRemove(chunk_pos);
            }
            unload_list.Clear();

            chunk_unload_list.Flip();

            if (_unload_amt > 0)
            {
                //Console.WriteLine("Unload Cycle Finished (" + _unload_amt.ToString() + ")");
            }
            

        }

        
        public void Draw()
        {
            DrawEntities();

            float fog_mult = 0;
            if (render_distance == 3)
            {
                fog_mult = 0.03f;
            }
            else if (render_distance == 4)
            {
                fog_mult = 0.015f;
            }
            else if(render_distance == 2)
            {
                fog_mult = 0.05f;
            }
            else if (render_distance == 1)
            {
                fog_mult = 0.075f;
            }

            Raylib.DrawCubeWires((last_player_chunk_position.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f)) * Globals.chunk_size.to_vector3(), Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, Color.White);

            Raylib.BeginShaderMode(chunk_material.Shader);
            Raylib.DrawCube(Vector3.Zero, 1, 1, 1, Color.White);
            Raylib.SetShaderValue(chunk_material.Shader, shader_uniform_fog_color, fog_color_array, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(chunk_material.Shader, shader_uniform_fog_mult, fog_mult, ShaderUniformDataType.Float);
            Raylib.SetShaderValueTexture(chunk_material.Shader, shader_uniform_texture_pos, TextureHandler.block_atlas.Texture);
            Raylib.SetShaderValueTexture(chunk_material.Shader, shader_uniform_texture_emissive_pos, TextureHandler.block_atlas_emissive.Texture);
            var buildsize = render_distance;
            for (int x = -buildsize; x <= buildsize; x++)
            {
                for (int y = -buildsize; y <= buildsize; y++)
                {
                    for (int z = -buildsize; z <= buildsize; z++)
                    {
                        var pos = last_player_chunk_position + new Int3(x, y, z);
                        Chunk? chunk = GetChunk(pos);
                        if (chunk != null)
                        {
                            if (Globals.CubeInView(cam.Position,cam.Target,chunk.GetCollider()))
                            {
                                if (chunk.generator.Drawable)
                                {
                                    Raylib.DrawMesh(chunk.generator.mesh, chunk_material, chunk.transform);
                                }
                                
                                //Raylib.DrawCubeWires((pos.to_vector3() + new Vector3(0.5f, 0.5f, 0.5f)) * Globals.chunk_size.to_vector3(), Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, Color.White);
                            }
                        }
                    }
                }
            }
            for (int x = -buildsize; x <= buildsize; x++)
            {
                for (int y = -buildsize; y <= buildsize; y++)
                {
                    for (int z = -buildsize; z <= buildsize; z++)
                    {
                        var pos = last_player_chunk_position + new Int3(x, y, z);
                        Chunk? chunk = GetChunk(pos);
                        if (chunk != null)
                        {
                            if (Globals.CubeInView(cam.Position, cam.Target, chunk.GetCollider()))
                            {
                                if (chunk.generator.DrawableT)
                                {
                                    Raylib.DrawMesh(chunk.generator.meshT, chunk_material, chunk.transform);
                                }
                                
                            }
                        }
                    }
                }
            }
            Raylib.EndShaderMode();
        }

        #endregion

        #region CHUNK GENERATION

        public List<Int3> GetStructureGeneratePositions(Int3 chunk_pos, Int3 offset, int random_mult = 0,int scale = 8)
        {
            List<Int3> positions = new List<Int3>();
            var cp_wb = (chunk_pos * Globals.chunk_size) - Globals.chunk_size;
            var cp_wb_mod = new Int3(Globals.better_modI(cp_wb.x + offset.x, scale), Globals.better_modI(cp_wb.y + offset.y, scale), Globals.better_modI(cp_wb.z + offset.z, scale));

            var next_x = scale - cp_wb_mod.x;
            var next_y = scale - cp_wb_mod.y;
            var next_z = scale - cp_wb_mod.z;

            var p_x = cp_wb;

            for (int sx = -Globals.chunk_size.x; sx < Globals.chunk_size.x; sx++)
            {
                p_x.x += next_x;
                next_x = scale;

                var p_y = p_x;
                
                for (int sy = -Globals.chunk_size.y; sy < Globals.chunk_size.y; sy++)
                {
                    p_y.y += next_y;
                    next_y = scale;

                    var p_z = p_y;

                    for (int sz = -Globals.chunk_size.z; sz < Globals.chunk_size.z; sz++)
                    {
                        p_z.z += next_z;
                        next_z = scale;

                        var ppp = new Int3(p_x.x, p_y.y, p_z.z);

                        //var v2 = Globals.GetPseudoRandom(p_x.x);

                        var p = ppp + new Int3(genrandom.Next(-random_mult, random_mult), genrandom.Next(-random_mult, random_mult), genrandom.Next(-random_mult, random_mult));

                        if (p.x >= cp_wb.x && p.x <= cp_wb.x + Globals.chunk_size.x &&
                            p.y >= cp_wb.y && p.y <= cp_wb.y + Globals.chunk_size.y &&
                            p.z >= cp_wb.z && p.z <= cp_wb.z + Globals.chunk_size.z)
                        {
                            positions.Add(p);
                        }
                    }
                }
            }
            return positions;
        }
        public void ChunkGenerate_Shaping(Int3 chunk_pos)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                for (int idx = 0; idx < chunk.map.fullsize; idx++)
                {
                    var CBP = chunk.map.IndexToPosition(idx);
                    var WBP = (chunk_pos * Globals.chunk_size) + CBP;
                    var v = BlockType.GreyStoneBlock;

                    /*
                    
                    if (WBP.z == -2 && WBP.x >= -1 && WBP.x <= 1 && WBP.y >= -1 && WBP.y <= 1)
                    {
                        v = BlockType.GreyStoneBlock;
                    }
                    */

                    
                    if (WBP.x > -5 && WBP.x < 5 && WBP.y > -5 && WBP.y < 5 && WBP.z > -5 && WBP.z < 5)
                    {
                        if (WBP.x == 4 || WBP.y == 4 || WBP.z == 4 || WBP.z == -4 || WBP.x == -4 || WBP.y == -4)
                        {
                            v = BlockType.ProtoGlassBlock;
                        }
                        else
                        {
                            v = BlockType.AirBlock;
                        }
                    }
                    else
                    {
                        if (fnl.GetNoise(WBP.x, WBP.y, WBP.z) > 0.5)
                        {
                            v = BlockType.AirBlock;
                        }
                        else
                        {
                            var r = genrandom.Next(0, 512);
                            if (r == 512)
                            {
                                v = BlockType.BlueOreBlock;
                            }
                            else if (r == 256)
                            {
                                v = BlockType.WhiteOreBlock;
                            }
                            else if (r == 128)
                            {
                                v = BlockType.MineBlock;
                            }
                        }
                    }
                    


                    chunk.map.Set(CBP, v);
                }
                //Console.WriteLine("Generated chunk");

                chunk.generation_stage = ChunkGenerationStage.Shaped;
            }
        }

        public void ChunkGenerate_Population(Int3 chunk_pos)
        {
            Chunk? chunk = GetChunk(chunk_pos);
            if (chunk != null)
            {
                List<Int3> structure_positions = GetStructureGeneratePositions(chunk_pos, new Int3(0,0,0), 1, 4);
                for (int i = 0; i < structure_positions.Count; i++)
                {
                    Int3 structure_root_block_pos = structure_positions[i];

                    var CBP = WBP_to_CBP(structure_root_block_pos);
                    var CP = WBP_to_ChunkPos(structure_root_block_pos);

                    chunk.map.Set(CBP, BlockType.ObsidionBlock);
                }

                chunk.generation_stage = ChunkGenerationStage.Populated;
            }
        }

        #endregion

        public void DrawDebugGUI()
        {

            //Raylib.DrawRectangle(640, 0, TextureHandler.block_atlas.Texture.Width, TextureHandler.block_atlas.Texture.Height, Color.Pink);
            //Raylib.DrawTexture(TextureHandler.block_atlas.Texture, 640, 0, Color.White);

            var _target = 7 * ((render_distance * 2) * (render_distance * 2) * (render_distance * 2));
            Raylib.DrawText("Chunks Loaded: " + chunks.Count.ToString() + "/" + _target.ToString(), 32, 64, 25, Color.White);
            Raylib.DrawText("Entities Loaded: " + entities.Count.ToString() + "/" + garbage_entities.Count.ToString(), 32, 64 + 32, 25, Color.White);
            Raylib.DrawText(last_player_chunk_position.x + ", " + last_player_chunk_position.y + ", " + last_player_chunk_position.z, 32, 64 + 32 + 32, 25, Color.White);
            Raylib.DrawText("Scheduled tick blocks: " + scheduled_tick_blocks.Count().ToString(), 32, 64 + 32 + 32 + 32, 25, Color.White);
        }

        public void ChangeFogColor(Color newcolor)
        {
            fog_color = newcolor;
            fog_color_array[0] = newcolor.R/255f;
            fog_color_array[1] = newcolor.G/255f;
            fog_color_array[2] = newcolor.B/255f;
        }
        public void ChangeRenderDistance(int distance)
        {
            render_distance = Math.Clamp(distance,2,4);
        }

        public void Cleanup()
        {
        }
    }
}
