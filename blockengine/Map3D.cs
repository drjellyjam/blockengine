using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class Map3D
    {
        ChunkBlock[] map;
        public int width;
        public int height;
        public int depth;
        public int fullsize;
        public bool changed = true; //sets to true when changed.
        public int air = 0;

        //Dictionary<string, int> trackers;

        public Map3D(int _width, int _height, int _depth, BlockType initial_block)
        {
            width = _width;
            height = _height;
            depth = _depth;
            fullsize = (_width * _height) * _depth;
            map = new ChunkBlock[fullsize];
            //map.Initialize();
            //trackers = new Dictionary<string, int>();
            
            for (int i = 0; i < fullsize; i++)
            {
                Block b = Globals.BlockDefinitions[initial_block];
                if (!b.IsExists())
                {
                    air++;
                }
                map[i] = new ChunkBlock(initial_block);
            }
            
        }

        public byte[] too_bytes()
        {
            byte[] bytes = new byte[fullsize];

            for (int i = 0; i < fullsize; i++)
            {
                bytes[i] = (byte)map[i].block;
            }

            return bytes;
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
            z = idx / (width*height);
            return new Int3(x,y,z);
        }

        public bool OutOfBounds(Int3 pos)
        {
            return (pos.x < 0 || pos.x > width - 1 || pos.y < 0 || pos.y > height - 1 || pos.z < 0 || pos.z > depth - 1);
        }
        public ChunkBlock? Get(Int3 pos)
        {
            if (!OutOfBounds(pos))
            {
                return map[PositionToIndex(pos)];
            }
            return null;
        }
        public bool Set(Int3 pos, BlockType newblock)
        {
            if (!OutOfBounds(pos))
            {
                int idx = PositionToIndex(pos);
                ChunkBlock prev = map[idx];
                Block prev_def = prev.GetBlockDef();
                Block next_def = Globals.BlockDefinitions[newblock];

                if (newblock == prev.block)
                {
                    changed = false;
                    return false;
                }

                if (!prev_def.IsExists() && next_def.IsExists())
                {
                    air--;
                }
                else if (prev_def.IsExists() && !next_def.IsExists())
                {
                    air++;
                }

                map[idx] = new ChunkBlock(newblock);
                changed = true;
                return true;
            }
            return false;
        }
    }
}
