using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace blockengine
{
    public enum StructurePositionType : int
    {
        Raw = 0,
        OnCeiling,
        OnGround,
    }

    public enum StructuresType : int
    {
        MiniCastle = 0
    }

    public struct BlockRect
    {
        public int XMIN;
        public int YMIN;
        public int ZMIN;
        public int XMAX;
        public int YMAX;
        public int ZMAX;

        public BlockRect(int xmin,int ymin,int zmin,int xmax,int ymax,int zmax)
        {
            XMIN = xmin;
            YMIN = ymin;
            ZMIN = zmin;
            XMAX = xmax;
            YMAX = ymax;
            ZMAX = zmax;
        }
    }

    public abstract class Structure
    {
        public int[] blockmap;

        public Int3 anchor_pos;

        public StructurePositionType structurePositionType;
        public int graph_scale;
        public int graph_randomness;
        public Int3 graph_offset;
        public StructuresType structureType;
        public int width, height, depth;

        public Structure()
        {
            blockmap = new int[3 * 3 * 3]
            {
                11,11,11,
                11,11,11,
                11,11,11,

                11,11,11,
                11,11,11,
                11,11,11,

                11,11,11,
                11,11,11,
                11,11,11,
            };
        }

        public int PositionToIndex(Int3 pos)
        {
            return ((int)pos.z * width * height) + ((int)pos.y * width) + (int)pos.x;
        }

        public Int3 IndexToPosition(int idx)
        {
            int x, y, z;
            x = idx % width;
            y = (idx / width) % height;
            z = idx / (width * height);
            return new Int3(x, y, z);
        }

        public BoxCollider GetBoxCollider(Int3 genpos)
        {
            var structx_min = -anchor_pos.x;
            var structy_min = -anchor_pos.y;
            var structz_min = -anchor_pos.z;
            var structx_max = (width - anchor_pos.x);
            var structy_max = (height - anchor_pos.y);
            var structz_max = (depth - anchor_pos.z);

            return new BoxCollider(new Vector3(genpos.x, genpos.y, genpos.z), new Vector3(structx_min, structy_min, structz_min), new Vector3(structx_max, structy_max, structz_max));
        }

        public BlockRect GetChunkIntersection(Int3 p,Int3 chunkp)
        {
            var structx_min = p.x - anchor_pos.x;
            var structy_min = p.y - anchor_pos.y;
            var structz_min = p.z - anchor_pos.z;
            var structx_max = p.x + (width - anchor_pos.x);
            var structy_max = p.y + (height - anchor_pos.y);
            var structz_max = p.z + (depth - anchor_pos.z);

            var cp_wp = chunkp * Globals.chunk_size;

            var chunkx_min = cp_wp.x;
            var chunky_min = cp_wp.y;
            var chunkz_min = cp_wp.z;
            var chunkx_max = cp_wp.x + Globals.chunk_size.x;
            var chunky_max = cp_wp.y + Globals.chunk_size.y;
            var chunkz_max = cp_wp.z + Globals.chunk_size.z;

            var sxmin = Math.Max(chunkx_min - structx_min, 0);
            var symin = Math.Max(chunky_min - structy_min, 0);
            var szmin = Math.Max(chunkz_min - structz_min, 0);

            var sxmax = Math.Min(Math.Min(width - sxmin, chunkx_max - structx_min), chunkx_max - chunkx_min);
            var symax = Math.Min(Math.Min(height - symin, chunky_max - structy_min), chunky_max - chunky_min);
            var szmax = Math.Min(Math.Min(depth - szmin, chunkz_max - structz_min), chunkz_max - chunkz_min);

            return new BlockRect(sxmin, symin, szmin, sxmax, symax, szmax);
        }
    }

    public class StructureMiniCastle : Structure
    {
        public StructureMiniCastle()
        {
            anchor_pos = new Int3(3, 3, 2);
            structurePositionType = StructurePositionType.OnGround;
            structureType = StructuresType.MiniCastle;

            graph_scale = 16;
            graph_randomness = 4;
            graph_offset = new Int3(0,0,0);

            width = 8; height = 8; depth = 10;

            blockmap = new int[(8 * 8) * 10]
            { 1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,11,11,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,11,11,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,11,11,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,11,11,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,11,11,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,11,11,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,11,11,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,11,11,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,11,11,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,11,11,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,11,11,11,11,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,11,11,11,11,1,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,1,11,11,11,11,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,11,11,11,11,1,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,11,11,11,11,11,11,1,
              1,1,11,11,11,11,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,11,1,11,1,1,1,
              1,1,1,1,1,1,11,1,
              1,11,1,1,1,1,1,1,
              1,1,1,1,1,1,11,1,
              1,11,1,1,1,1,1,1,
              1,1,1,11,1,11,1,1,
              1,1,1,1,1,1,1,1,

              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1,
              1,1,1,1,1,1,1,1
            };
        }
    }
}
