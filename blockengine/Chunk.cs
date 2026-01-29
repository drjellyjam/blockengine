using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace blockengine
{
    public class ChunkBlock
    {
        public float health;
        public bool active;
        public BlockType block;

        public ChunkBlock(BlockType _block)
        {
            block = _block;
            var _block_def = Globals.BlockDefinitions[block];
            health = _block_def.GetDurability();
            active = true;
        }

        public Block GetBlockDef()
        {
            return Globals.BlockDefinitions[block];
        }
    }
    public class Chunk
    {
        public Map3D map;
        public ChunkMeshGenerator generator;
        public Matrix4x4 transform;
        public Int3 chunkpos;
        public bool needs_rebuilt;
        public bool first_built;
        public Chunk(Int3 chunkpos)
        {
            needs_rebuilt = false;
            first_built = false;
            //(chunkpos * Globals.chunk_size).Print();
            this.chunkpos = chunkpos;
            transform = Matrix4x4.Transpose(Matrix4x4.CreateTranslation((chunkpos * Globals.chunk_size).to_vector3()) * Matrix4x4.CreateScale(Globals.BlockScale));
            map = new Map3D(Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, BlockType.GreyStoneBlock);
            
            generator = new ChunkMeshGenerator();
        }
        public int UploadMeshes()
        {
            var b = generator.UploadMeshes();
            return b;
        }
        public void UnloadMeshes()
        {
            generator.UnloadMeshes();
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
    }
}
