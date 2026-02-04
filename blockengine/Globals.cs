using Raylib_cs;
using static Raylib_cs.Raymath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using blockengine.Items;

namespace blockengine
{
    public static class Globals
    {
        public static Int3 chunk_size = new Int3(16,16,16);
        public static float chunk_fullsize = (chunk_size.x * chunk_size.y) * chunk_size.z;
        public static int PositionToIndex(Int3 pos, Int3 size)
        {
            return ((int)pos.z * size.x * size.y) + ((int)pos.y * size.x) + (int)pos.x;
        }

        public static Int3 IndexToPosition(int idx, Int3 size)
        {
            int x, y, z;
            x = idx % size.x;
            y = (idx / size.x) % size.y;
            z = idx / (size.x * size.y);
            return new Int3(x, y, z);
        }

        public static float BlockScale = 1f;
        public static Dictionary<BlockType, Block> BlockDefinitions = new Dictionary<BlockType, Block>()
        {
            //["null"] = null, /// the void
            [BlockType.ErrorBlock] = new ErrorBlock(),//new BlockDefinition(ErrorBlock, "Error",true,true,false,1,null,new BlockFaces<string>("Assets/Textures/missing.png")),
            [BlockType.AirBlock] = new AirBlock(),
            //[1] = new BlockDefinition("Grass", true, false, false, new BlockFaces("Assets/Textures/grassblock_top.png", "Assets/Textures/dirt.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png", "Assets/Textures/grassblock_side.png")),
            [BlockType.GreyStoneBlock] = new GreyStoneBlock(),//new BlockDefinition(new GreyStoneBlock(), "Grey Stone", true, false, false, 1, null, new BlockFaces<string>("Assets/Textures/greystone.png")),
            [BlockType.BrownStoneBlock] = new BrownStoneBlock(),//new BlockDefinition(new BrownStoneBlock(), "Brown Stone", true, false, false, 1, null, new BlockFaces<string>("Assets/Textures/brownstone.png")),
            [BlockType.BlueOreBlock] = new BlueOreBlock(),
            [BlockType.WhiteOreBlock] = new WhiteOreBlock(),
            [BlockType.WaterBlock] = new WaterBlock(),
            [BlockType.LavaBlock] = new LavaBlock(),
            [BlockType.ObsidionBlock] = new ObsidionBlock(),
            [BlockType.StoneBrickBlock] = new StoneBrickBlock(),
            //["WHITE_ORE"] = new BlockDefinition("White Ore", true, false, false, 1, null, new BlockFaces<string>("Assets/Textures/greystone_whiteore.png")),
            [BlockType.ProtoGlassBlock] = new ProtoGlassBlock(),
            [BlockType.MineBlock] = new MineBlock(),//new BlockDefinition(new MineBlock(), "Mine", true, false, true, 1, new BlockModel("Assets/Models/mine.obj", "Assets/Textures/mine.png", Vector3.One * 0.4f, Vector3.Zero, new BlockFaces<bool>(true)))
            //[3] = new BlockDefinition("Dirt", true, false, false, new BlockFaces("Assets/Textures/dirt.png")),
            //[4] = new BlockDefinition("Log", true, false, false, new BlockFaces("Assets/Textures/log_top.png", "Assets/Textures/log_top.png", "Assets/Textures/log_side.png"))
        };
        public static Dictionary<ItemType, Item> ItemDefinitions = new Dictionary<ItemType, Item>() {
            
        };
        public static Dictionary<StructuresType, Structure> StructureDefinitions = new Dictionary<StructuresType, Structure>()
        {
            [StructuresType.MiniCastle] = new StructureMiniCastle()
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

        public static Int3[] flood_positions = new Int3[343]
        {
            new Int3(0,0,0),
            new Int3(-1,-1,-1),
            new Int3(-1,-1,0),
            new Int3(-1,-1,1),
            new Int3(-1,0,-1),
            new Int3(-1,0,0),
            new Int3(-1,0,1),
            new Int3(-1,1,-1),
            new Int3(-1,1,0),
            new Int3(-1,1,1),
            new Int3(0,-1,-1),
            new Int3(0,-1,0),
            new Int3(0,-1,1),
            new Int3(0,0,-1),
            new Int3(0,0,1),
            new Int3(0,1,-1),
            new Int3(0,1,0),
            new Int3(0,1,1),
            new Int3(1,-1,-1),
            new Int3(1,-1,0),
            new Int3(1,-1,1),
            new Int3(1,0,-1),
            new Int3(1,0,0),
            new Int3(1,0,1),
            new Int3(1,1,-1),
            new Int3(1,1,0),
            new Int3(1,1,1),
            new Int3(-2,-2,-2),
            new Int3(-2,-2,-1),
            new Int3(-2,-2,0),
            new Int3(-2,-1,-2),
            new Int3(-2,-1,-1),
            new Int3(-2,-1,0),
            new Int3(-2,0,-2),
            new Int3(-2,0,-1),
            new Int3(-2,0,0),
            new Int3(-1,-2,-2),
            new Int3(-1,-2,-1),
            new Int3(-1,-2,0),
            new Int3(-1,-1,-2),
            new Int3(-1,0,-2),
            new Int3(0,-2,-2),
            new Int3(0,-2,-1),
            new Int3(0,-2,0),
            new Int3(0,-1,-2),
            new Int3(0,0,-2),
            new Int3(-2,-2,1),
            new Int3(-2,-1,1),
            new Int3(-2,0,1),
            new Int3(-1,-2,1),
            new Int3(0,-2,1),
            new Int3(-2,-2,2),
            new Int3(-2,-1,2),
            new Int3(-2,0,2),
            new Int3(-1,-2,2),
            new Int3(-1,-1,2),
            new Int3(-1,0,2),
            new Int3(0,-2,2),
            new Int3(0,-1,2),
            new Int3(0,0,2),
            new Int3(-2,1,-2),
            new Int3(-2,1,-1),
            new Int3(-2,1,0),
            new Int3(-1,1,-2),
            new Int3(0,1,-2),
            new Int3(-2,1,1),
            new Int3(-2,1,2),
            new Int3(-1,1,2),
            new Int3(0,1,2),
            new Int3(-2,2,-2),
            new Int3(-2,2,-1),
            new Int3(-2,2,0),
            new Int3(-1,2,-2),
            new Int3(-1,2,-1),
            new Int3(-1,2,0),
            new Int3(0,2,-2),
            new Int3(0,2,-1),
            new Int3(0,2,0),
            new Int3(-2,2,1),
            new Int3(-1,2,1),
            new Int3(0,2,1),
            new Int3(-2,2,2),
            new Int3(-1,2,2),
            new Int3(0,2,2),
            new Int3(1,-2,-2),
            new Int3(1,-2,-1),
            new Int3(1,-2,0),
            new Int3(1,-1,-2),
            new Int3(1,0,-2),
            new Int3(1,-2,1),
            new Int3(1,-2,2),
            new Int3(1,-1,2),
            new Int3(1,0,2),
            new Int3(1,1,-2),
            new Int3(1,1,2),
            new Int3(1,2,-2),
            new Int3(1,2,-1),
            new Int3(1,2,0),
            new Int3(1,2,1),
            new Int3(1,2,2),
            new Int3(2,-2,-2),
            new Int3(2,-2,-1),
            new Int3(2,-2,0),
            new Int3(2,-1,-2),
            new Int3(2,-1,-1),
            new Int3(2,-1,0),
            new Int3(2,0,-2),
            new Int3(2,0,-1),
            new Int3(2,0,0),
            new Int3(2,-2,1),
            new Int3(2,-1,1),
            new Int3(2,0,1),
            new Int3(2,-2,2),
            new Int3(2,-1,2),
            new Int3(2,0,2),
            new Int3(2,1,-2),
            new Int3(2,1,-1),
            new Int3(2,1,0),
            new Int3(2,1,1),
            new Int3(2,1,2),
            new Int3(2,2,-2),
            new Int3(2,2,-1),
            new Int3(2,2,0),
            new Int3(2,2,1),
            new Int3(2,2,2),
            new Int3(-3,-3,-3),
            new Int3(-3,-3,-2),
            new Int3(-3,-3,-1),
            new Int3(-3,-2,-3),
            new Int3(-3,-2,-2),
            new Int3(-3,-2,-1),
            new Int3(-3,-1,-3),
            new Int3(-3,-1,-2),
            new Int3(-3,-1,-1),
            new Int3(-2,-3,-3),
            new Int3(-2,-3,-2),
            new Int3(-2,-3,-1),
            new Int3(-2,-2,-3),
            new Int3(-2,-1,-3),
            new Int3(-1,-3,-3),
            new Int3(-1,-3,-2),
            new Int3(-1,-3,-1),
            new Int3(-1,-2,-3),
            new Int3(-1,-1,-3),
            new Int3(-3,-3,0),
            new Int3(-3,-2,0),
            new Int3(-3,-1,0),
            new Int3(-2,-3,0),
            new Int3(-1,-3,0),
            new Int3(-3,-3,1),
            new Int3(-3,-2,1),
            new Int3(-3,-1,1),
            new Int3(-2,-3,1),
            new Int3(-1,-3,1),
            new Int3(-3,0,-3),
            new Int3(-3,0,-2),
            new Int3(-3,0,-1),
            new Int3(-2,0,-3),
            new Int3(-1,0,-3),
            new Int3(-3,0,0),
            new Int3(-3,0,1),
            new Int3(-3,1,-3),
            new Int3(-3,1,-2),
            new Int3(-3,1,-1),
            new Int3(-2,1,-3),
            new Int3(-1,1,-3),
            new Int3(-3,1,0),
            new Int3(-3,1,1),
            new Int3(0,-3,-3),
            new Int3(0,-3,-2),
            new Int3(0,-3,-1),
            new Int3(0,-2,-3),
            new Int3(0,-1,-3),
            new Int3(0,-3,0),
            new Int3(0,-3,1),
            new Int3(0,0,-3),
            new Int3(0,1,-3),
            new Int3(1,-3,-3),
            new Int3(1,-3,-2),
            new Int3(1,-3,-1),
            new Int3(1,-2,-3),
            new Int3(1,-1,-3),
            new Int3(1,-3,0),
            new Int3(1,-3,1),
            new Int3(1,0,-3),
            new Int3(1,1,-3),
            new Int3(-3,-3,2),
            new Int3(-3,-2,2),
            new Int3(-3,-1,2),
            new Int3(-2,-3,2),
            new Int3(-1,-3,2),
            new Int3(-3,0,2),
            new Int3(-3,1,2),
            new Int3(0,-3,2),
            new Int3(1,-3,2),
            new Int3(-3,-3,3),
            new Int3(-3,-2,3),
            new Int3(-3,-1,3),
            new Int3(-2,-3,3),
            new Int3(-2,-2,3),
            new Int3(-2,-1,3),
            new Int3(-1,-3,3),
            new Int3(-1,-2,3),
            new Int3(-1,-1,3),
            new Int3(-3,0,3),
            new Int3(-2,0,3),
            new Int3(-1,0,3),
            new Int3(-3,1,3),
            new Int3(-2,1,3),
            new Int3(-1,1,3),
            new Int3(0,-3,3),
            new Int3(0,-2,3),
            new Int3(0,-1,3),
            new Int3(0,0,3),
            new Int3(0,1,3),
            new Int3(1,-3,3),
            new Int3(1,-2,3),
            new Int3(1,-1,3),
            new Int3(1,0,3),
            new Int3(1,1,3),
            new Int3(-3,2,-3),
            new Int3(-3,2,-2),
            new Int3(-3,2,-1),
            new Int3(-2,2,-3),
            new Int3(-1,2,-3),
            new Int3(-3,2,0),
            new Int3(-3,2,1),
            new Int3(0,2,-3),
            new Int3(1,2,-3),
            new Int3(-3,2,2),
            new Int3(-3,2,3),
            new Int3(-2,2,3),
            new Int3(-1,2,3),
            new Int3(0,2,3),
            new Int3(1,2,3),
            new Int3(-3,3,-3),
            new Int3(-3,3,-2),
            new Int3(-3,3,-1),
            new Int3(-2,3,-3),
            new Int3(-2,3,-2),
            new Int3(-2,3,-1),
            new Int3(-1,3,-3),
            new Int3(-1,3,-2),
            new Int3(-1,3,-1),
            new Int3(-3,3,0),
            new Int3(-2,3,0),
            new Int3(-1,3,0),
            new Int3(-3,3,1),
            new Int3(-2,3,1),
            new Int3(-1,3,1),
            new Int3(0,3,-3),
            new Int3(0,3,-2),
            new Int3(0,3,-1),
            new Int3(0,3,0),
            new Int3(0,3,1),
            new Int3(1,3,-3),
            new Int3(1,3,-2),
            new Int3(1,3,-1),
            new Int3(1,3,0),
            new Int3(1,3,1),
            new Int3(-3,3,2),
            new Int3(-2,3,2),
            new Int3(-1,3,2),
            new Int3(0,3,2),
            new Int3(1,3,2),
            new Int3(-3,3,3),
            new Int3(-2,3,3),
            new Int3(-1,3,3),
            new Int3(0,3,3),
            new Int3(1,3,3),
            new Int3(2,-3,-3),
            new Int3(2,-3,-2),
            new Int3(2,-3,-1),
            new Int3(2,-2,-3),
            new Int3(2,-1,-3),
            new Int3(2,-3,0),
            new Int3(2,-3,1),
            new Int3(2,0,-3),
            new Int3(2,1,-3),
            new Int3(2,-3,2),
            new Int3(2,-3,3),
            new Int3(2,-2,3),
            new Int3(2,-1,3),
            new Int3(2,0,3),
            new Int3(2,1,3),
            new Int3(2,2,-3),
            new Int3(2,2,3),
            new Int3(2,3,-3),
            new Int3(2,3,-2),
            new Int3(2,3,-1),
            new Int3(2,3,0),
            new Int3(2,3,1),
            new Int3(2,3,2),
            new Int3(2,3,3),
            new Int3(3,-3,-3),
            new Int3(3,-3,-2),
            new Int3(3,-3,-1),
            new Int3(3,-2,-3),
            new Int3(3,-2,-2),
            new Int3(3,-2,-1),
            new Int3(3,-1,-3),
            new Int3(3,-1,-2),
            new Int3(3,-1,-1),
            new Int3(3,-3,0),
            new Int3(3,-2,0),
            new Int3(3,-1,0),
            new Int3(3,-3,1),
            new Int3(3,-2,1),
            new Int3(3,-1,1),
            new Int3(3,0,-3),
            new Int3(3,0,-2),
            new Int3(3,0,-1),
            new Int3(3,0,0),
            new Int3(3,0,1),
            new Int3(3,1,-3),
            new Int3(3,1,-2),
            new Int3(3,1,-1),
            new Int3(3,1,0),
            new Int3(3,1,1),
            new Int3(3,-3,2),
            new Int3(3,-2,2),
            new Int3(3,-1,2),
            new Int3(3,0,2),
            new Int3(3,1,2),
            new Int3(3,-3,3),
            new Int3(3,-2,3),
            new Int3(3,-1,3),
            new Int3(3,0,3),
            new Int3(3,1,3),
            new Int3(3,2,-3),
            new Int3(3,2,-2),
            new Int3(3,2,-1),
            new Int3(3,2,0),
            new Int3(3,2,1),
            new Int3(3,2,2),
            new Int3(3,2,3),
            new Int3(3,3,-3),
            new Int3(3,3,-2),
            new Int3(3,3,-1),
            new Int3(3,3,0),
            new Int3(3,3,1),
            new Int3(3,3,2),
            new Int3(3,3,3)
        };
        public static int flood_draw_dist_high = 343;
        public static int flood_draw_dist_medium = 125;
        public static int flood_draw_dist_low = 27;

        

        public static float GetDelta()
        {
            return MathF.Min(Raylib.GetFrameTime(),1f);
        }
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

        public static bool CubeInView(Vector3 cam_pos,Vector3 cam_target,BoxCollider cube)
        {
            return true;
        }

        
    }
}
