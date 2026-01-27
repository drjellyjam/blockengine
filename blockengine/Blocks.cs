using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public enum BlockType : int
    {
        ErrorBlock = 0,
        AirBlock,
        GreyStoneBlock,
        BrownStoneBlock,
        MineBlock,

        BlockCount
    }
    public class ErrorBlock : Block
    {
        public override string GetDisplayName()
        {
            return "Error";
        }

        public override BlockType GetBlockType()
        {
            return BlockType.ErrorBlock;
        }
    }
    public class AirBlock : Block
    {
        public override string GetDisplayName()
        {
            return "Air";
        }

        public override bool IsExists()
        {
            return false;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.AirBlock;
        }
    }

    public class GreyStoneBlock : Block
    {
        public GreyStoneBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/greystone.png");
        }

        public override string GetDisplayName()
        {
            return "Greystone";
        }

        public override BlockType GetBlockType()
        {
            return BlockType.GreyStoneBlock;
        }
    }

    public class BrownStoneBlock : Block
    {
        public BrownStoneBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/brownstone.png");
        }

        public override string GetDisplayName()
        {
            return "Brownstone";
        }

        public override BlockType GetBlockType()
        {
            return BlockType.BrownStoneBlock;
        }
    }

    public class MineBlock : Block
    {
        public MineBlock()
        {
            BlockModel = new BlockModel("Assets/Models/mine.obj", "Assets/Textures/mine.png", Vector3.One * 0.4f, Vector3.Zero, new BlockFaces<bool>(true));
        }
        public override string GetDisplayName()
        {
            return "Mine";
        }

        public override bool IsTranslucent()
        {
            return true;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.MineBlock;
        }

        public override void OnNearBlockChanged(World world, Int3 WBP, BlockType changed_block_type, Int3 changed_WBP)
        {
            Console.WriteLine("Block Changed " + "(" + WBP.x + " " + WBP.y + " " + WBP.z + ")");
            world.SetBlockActive(WBP,false);
            world.StartBlockChange();
            world.SetBlock(WBP, BlockType.AirBlock);
            var _size = 4;
            for (int x = -_size; x < _size; x++) {
                for (int y = -_size; y < _size; y++)
                {
                    for (int z = -_size; z < _size; z++)
                    {
                        var pos = new Int3(x, y, z);
                        var dist = pos.to_vector3().Length();
                        if (dist <= _size)
                        {
                            world.SetBlock(WBP + pos, BlockType.AirBlock);
                        }
                    }
                }
            }
            
            world.CommitBlockChange();
        }
    }
}
