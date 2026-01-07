using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public static class Globals
    {
        public static Int3 chunk_size = new Int3(16,16,16);
        public static float BlockScale = 1f;
        public static Dictionary<int, BlockDefinition?> BlockDefinitions = new Dictionary<int, BlockDefinition?>()
        {
            [-1] = null, /// the void
            [0] = new BlockDefinition("Air",false),
            //[1] = new BlockDefinition("Grass", true, false, false, new BlockFaces("Assets/Textures/grassblock_top.png", "Assets/Textures/dirt.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png")),
            [1] = new BlockDefinition("Grey Stone", true, false, false, new BlockFaces("Assets/Textures/greystone.png")),
            [2] = new BlockDefinition("Brown Stone", true, false, false, new BlockFaces("Assets/Textures/brownstone.png")),
            [3] = new BlockDefinition("Blue Ore", true, false, false, new BlockFaces("Assets/Textures/greystone_blueore.png")),
            [4] = new BlockDefinition("White Ore", true, false, false, new BlockFaces("Assets/Textures/greystone_whiteore.png")),
            //[3] = new BlockDefinition("Dirt", true, false, false, new BlockFaces("Assets/Textures/dirt.png")),
            //[4] = new BlockDefinition("Log", true, false, false, new BlockFaces("Assets/Textures/log_top.png", "Assets/Textures/log_top.png", "Assets/Textures/log_side.png"))
        };
        public static Int3[] block_normals = new Int3[6]
        {
            new Int3(-1,0,0),
            new Int3(1,0,0),
            new Int3(0,-1,0),
            new Int3(0,1,0),
            new Int3(0,0,-1),
            new Int3(0,0,1)
        };

        public static Int3[] around_positions = new Int3[27]
        {
            new Int3(-1,-1,-1), new Int3(-1,-1,0), new Int3(-1,-1,1), 
            new Int3(-1,0,-1), new Int3(-1,0,0), new Int3(-1,0,1), 
            new Int3(-1,1,-1), new Int3(-1,1,0), new Int3(-1,1,1), 

            new Int3(0,-1,-1), new Int3(0,-1,0), new Int3(0,-1,1), 
            new Int3(0,0,-1), new Int3(0,0,0), new Int3(0,0,1), 
            new Int3(0,1,-1), new Int3(0,1,0), new Int3(0,1,1), 

            new Int3(1,-1,-1), new Int3(1,-1,0), new Int3(1,-1,1), 
            new Int3(1,0,-1), new Int3(1,0,0), new Int3(1,0,1), 
            new Int3(1,1,-1), new Int3(1,1,0), new Int3(1,1,1)
        };

        public static Int3 WorldPosToChunkPos(Vector3 world_pos)
        {
            return new Int3((int)MathF.Floor(world_pos.X / chunk_size.x), (int)MathF.Floor(world_pos.Y / chunk_size.y), (int)MathF.Floor(world_pos.Z / chunk_size.z));
        }

        static public int better_modI(float n, float m)
        {
            return (int)better_mod(n, m);
            /*
            if (n >= 0)
            {
                return (int)(n % m);
            }
            return (int)((Math.Ceiling(Math.Abs(n) / m) * m) + n);
            */
        }

        static public float better_mod(float n, float m)
        {
            if (n >= 0)
            {
                return n % m;
            }
            return (MathF.Ceiling(Math.Abs(n) / m) * m) + n;
        }

        static public Int3 better_modI(Int3 n, Int3 m)
        {
            return new Int3(
                better_modI(n.x, m.x),
                better_modI(n.y, m.y),
                better_modI(n.z, m.z)
            );
        }
    }
}
