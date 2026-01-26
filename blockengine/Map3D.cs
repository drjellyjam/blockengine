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
        Block[] map;
        public int width;
        public int height;
        public int depth;
        public int fullsize;
        public bool changed = true; //sets to true when changed.
        public int air = 0;

        //Dictionary<string, int> trackers;

        public Map3D(int _width, int _height, int _depth, string initial_value = "AIR")
        {
            width = _width;
            height = _height;
            depth = _depth;
            fullsize = (_width * _height) * _depth;
            map = new Block[fullsize];
            //trackers = new Dictionary<string, int>();

            for (int i = 0; i < fullsize; i++)
            {
                Block b = new Block(initial_value);
                if (!b.GetDefinition().Exists)
                {
                    air++;
                }
                map[i] = b;
            }
        }

        public byte[] too_bytes()
        {
            byte[] bytes = new byte[fullsize];

            for (int i = 0; i < fullsize; i++)
            {
                bytes[i] = (byte)1; //map[i];
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
        public Block? Get(Int3 pos)
        {
            if (!OutOfBounds(pos))
            {
                return map[PositionToIndex(pos)];
            }
            return null;
        }
        public bool Set(Int3 pos, string set_definition)
        {
            if (!OutOfBounds(pos))
            {
                int idx = PositionToIndex(pos);
                Block prev = map[idx];

                if (set_definition == prev.definition_ID)
                {
                    return false;
                }

                BlockDefinition next = Globals.BlockDefinitions[set_definition];
                
                if (!prev.GetDefinition().Exists && next.Exists)
                {
                    air--;
                }
                else if (prev.GetDefinition().Exists && !next.Exists)
                {
                    air++;
                }

                map[idx].SetDefinition(set_definition);
                return true;
            }
            return false;
        }
    }
}
