using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace blockengine
{
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
            map = new Map3D( Globals.chunk_size.x, Globals.chunk_size.y, Globals.chunk_size.z, "GREY_STONE");
            
            generator = new ChunkMeshGenerator();
        }

        public bool UploadMeshes()
        {
            generator.UnloadMesh();
            bool b1 = generator.GenerateMesh();
            bool b2 = generator.GenerateMeshT();

            return b1 || b2;
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
