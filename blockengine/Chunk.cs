using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Xml.Serialization;

namespace blockengine
{
    public enum ChunkGenerationStage
    {
        NotGenerated = 0,
        Shaped,
        Populated
    }
    public class ChunkBlock
    {
        //public float health;
        public bool active;
        public BlockType block;
        private Dictionary<string, float> blockData;
        

        public ChunkBlock(BlockType _block)
        {
            block = _block;
            //var _block_def = Globals.BlockDefinitions[block];
            //health = _block_def.GetDurability();
            blockData = new Dictionary<string, float>();
            active = true;
        }

        public Block GetBlockDef()
        {
            return Globals.BlockDefinitions[block];
        }

        public bool HasBlockData(string name)
        {
            return blockData.ContainsKey(name);
        } 
        public void SetBlockData(string name, float value)
        {
            blockData[name] = value;
        }
        public float GetBlockData(string name) { return blockData[name]; }
    }
    public class Chunk
    {
        public Map3D map;
        public ChunkMeshGenerator generator;
        public Matrix4x4 transform;
        public Int3 chunkpos;
        public bool needs_rebuilt;
        public bool first_built;
        public ChunkGenerationStage generation_stage = ChunkGenerationStage.NotGenerated;
        public BiomeType biome;
        public Chunk(Int3 chunkpos)
        {
            needs_rebuilt = false;
            first_built = false;
            //(chunkpos * Globals.chunk_size).Print();
            this.chunkpos = chunkpos;
            transform = Matrix4x4.Transpose(Matrix4x4.CreateTranslation((chunkpos * Globals.chunk_size).to_vector3()) * Matrix4x4.CreateScale(Globals.BlockScale));
            map = new Map3D(Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, BlockType.GreyStoneBlock);
            biome = BiomeType.Caverns;
            generator = new ChunkMeshGenerator();
        }
        public int UploadMeshes()
        {
            var b = generator.UploadMeshes();
            return b;
        }
        public void UnloadMeshes()
        {
            generator.UnloadAllMeshes();
        }
        public bool IsChanged()
        {
            return map.changed;
        }

        public bool IsAir()
        {
            return map.air >= map.fullsize;
        }

        public bool IsFull()
        {
            return map.air <= 0;
        }

        public bool WillBuild()
        {
            return !IsAir() && !IsFull();
        }

        public bool WontBuild()
        {
            return IsAir() || IsFull();
        }

        public BoxCollider GetCollider()
        {
            var _p = (chunkpos * Globals.chunk_size).to_vector3();
            return new BoxCollider(_p,_p,_p + Globals.chunk_size.to_vector3() );
        }
    }
}
