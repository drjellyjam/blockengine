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
        public Chunk(Int3 chunkpos)
        {
            //(chunkpos * Globals.chunk_size).Print();
            this.chunkpos = chunkpos;
            transform = Matrix4x4.Transpose(Matrix4x4.CreateTranslation((chunkpos * Globals.chunk_size).to_vector3()) * Matrix4x4.CreateScale(Globals.BlockScale));
            map = new Map3D(Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, BlockType.GreyStoneBlock);
            
            generator = new ChunkMeshGenerator();
        }

        public int UploadMeshes()
        {
            generator.UnloadMesh();
            int b1 = generator.GenerateMesh();
            int b2 = generator.GenerateMeshT();

            if (b1 == 2 || b2 == 2)
            {
                return 1;
            }
            return 0;
        }
        public void UnloadMesh()
        {
            generator.UnloadMesh();
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
