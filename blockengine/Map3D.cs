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
        int[] map;
        public int width;
        public int height;
        public int depth;
        public int fullsize;
        public bool changed = false; //sets to true when changed.
        
        Dictionary<int, int> trackers;

        public Map3D(int _width, int _height, int _depth, int initial_value = -1)
        {
            width = _width;
            height = _height;
            depth = _depth;
            fullsize = (_width * _height) * _depth;
            map = new int[fullsize];
            trackers = new Dictionary<int, int>();
            if (initial_value > -1)
            {
                for (int i = 0; i<fullsize; i++)
                {
                    map[i] = initial_value;
                }
                changed = true;
            }
        }

        public byte[] too_bytes()
        {
            byte[] bytes = new byte[fullsize];

            for (int i = 0; i < fullsize; i++)
            {
                bytes[i] = (byte)map[i];
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

        public void AddTracker(int block_to_track)
        {
            if (!trackers.ContainsKey(block_to_track))
            {
                int v = 0;
                for (int i = 0; i < fullsize; i++)
                {
                    if (map[i]== block_to_track)
                    {
                        v++;
                    }
                }
                trackers.Add(block_to_track, v);
            }
        }

        public int GetTrackerValue(int being_tracked)
        {
            if (trackers.ContainsKey(being_tracked))
            {
                return trackers[being_tracked];
            }
            return -1;
        }
        public bool OutOfBounds(Int3 pos)
        {
            return (pos.x < 0 || pos.x > width - 1 || pos.y < 0 || pos.y > height - 1 || pos.z < 0 || pos.z > depth - 1);
        }
        public int Get(Int3 pos)
        {
            if (!OutOfBounds(pos))
            {
                return map[PositionToIndex(pos)];
            }
            return -1;
        }
        public void Set(Int3 pos, int setto)
        {
            if (!OutOfBounds(pos))
            {
                int idx = PositionToIndex(pos);
                int prev = map[idx];
                if (trackers.ContainsKey(prev))
                {
                    trackers[prev] -= 1;
                }
                if (trackers.ContainsKey(setto))
                {
                    trackers[setto] += 1;
                }
                map[idx] = setto;

                if (setto != prev)
                {
                    changed = true;
                }
            }
        }
    }
}
