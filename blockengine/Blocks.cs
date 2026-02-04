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
        ProtoGlassBlock,
        BlueOreBlock,
        WhiteOreBlock,
        WaterBlock,
        LavaBlock,
        ObsidionBlock,
        StoneBrickBlock,

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

    public class StoneBrickBlock : Block
    {
        public StoneBrickBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/stone_brick.png");
        }

        public override string GetDisplayName()
        {
            return "Stone Brick";
        }

        public override BlockType GetBlockType()
        {
            return BlockType.StoneBrickBlock;
        }
    }

    public class BrownStoneBlock : Block
    {
        public BrownStoneBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/brownstone.png");
        }

        public override float GetDurability()
        {
            return 2;
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

    public class ProtoGlassBlock : Block
    {
        public ProtoGlassBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/proto_glass.png");
        }

        public override float GetDurability()
        {
            return 0.5f;
        }

        public override bool IsTranslucent()
        {
            return true;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.ProtoGlassBlock;
        }

        public override string GetDisplayName()
        {
            return "Prototype Glass";
        }
    }

    public class BlueOreBlock : Block
    {
        public BlueOreBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/greystone_blueore.png");
        }

        public override float GetDurability()
        {
            return 1.5f;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.BlueOreBlock;
        }

        public override string GetDisplayName()
        {
            return "Blue Ore";
        }
    }

    public class WhiteOreBlock : Block
    {
        public WhiteOreBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/greystone_whiteore.png");
        }

        public override float GetDurability()
        {
            return 1.5f;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.WhiteOreBlock;
        }

        public override string GetDisplayName()
        {
            return "White Ore";
        }
    }

    public class ObsidionBlock : Block
    {
        public ObsidionBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/obsidion.png");
        }

        public override float GetDurability()
        {
            return 16;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.ObsidionBlock;
        }

        public override string GetDisplayName()
        {
            return "Obsidion";
        }
    }

    public class MineBlock : Block
    {
        public MineBlock()
        {
            collider = new BoxCollider(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.3f, -0.3f, -0.3f), new Vector3(0.3f, 0.3f, 0.3f));
            BlockModel = new BlockModel("Assets/Models/mine.obj", "Assets/Textures/mine.png", Vector3.One * 0.4f, Vector3.Zero, new BlockFaces<bool>(true));
            //BlockModel = new BlockModel("Assets/Models/pickaxe.obj", "Assets/Textures/blue_pickaxe.png", Vector3.One * 0.4f, Vector3.Zero, new BlockFaces<bool>(true));
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

        public override float GetDurability()
        {
            return 3;
        }

        private void Explode(World world, Int3 WBP)
        {
            world.SetBlockActive(WBP, false);
            world.StartBlockChange();
            world.SetBlock(WBP, BlockType.AirBlock);
            var _size = 4;
            for (int x = -_size; x < _size; x++)
            {
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

        public override void OnBlockBreak(World world, Int3 WBP)
        {
            Explode(world, WBP);
        }

        public override void OnNearBlockChanged(World world, Int3 WBP, BlockType changed_block_type, Int3 changed_WBP)
        {
            Explode(world, WBP);
        }
    }


    public class WaterBlock : Block
    {
        private int max_flow = 8;
        protected Int3[] flow_positions;
        public WaterBlock()
        {
            StartBlockData["flow"] = max_flow;
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/water.png");
            flow_positions = new Int3[4] { new Int3(1, 0, 0), new Int3(-1, 0, 0), new Int3(0, 1, 0), new Int3(0, -1, 0) };
        }

        public virtual bool Flow(World world, Int3 WBP, bool flow = true)
        {
            BlockType mytype = GetBlockType();
            Int3 down = new Int3(0, 0, -1);
            ChunkBlock? down_block = world.GetBlock(WBP + down);
            if (down_block != null && (down_block.block == BlockType.AirBlock || down_block.block == mytype))
            {
                if (down_block.block == mytype)
                {
                    return false;
                }
                if (flow) {
                    bool changed = world.PlaceBlock(WBP + down, mytype);
                }
                return true;
            }
            else
            {
                bool flowwed = false;
                for (int i = 0; i < 4; i++)
                {
                    Int3 dir = flow_positions[i];
                    ChunkBlock? flow_block = world.GetBlock(WBP + dir);
                    if (flow_block != null && flow_block.block == BlockType.AirBlock)
                    {
                        var v = world.GetBlockData(WBP, "flow") - 1;
                        if (flow && v > 0) {
                            bool changed = world.PlaceBlock(WBP + dir, mytype);
                            if (changed)
                            {
                                world.SetBlockData(WBP + dir, "flow", v);
                            }
                        }
                        if (v > 0)
                        {
                            flowwed = true;
                        }
                    }
                }
                return flowwed;
            }
            return false;
        }

        public override void OnBlockInit(World world, Int3 WBP)
        {
            if (Flow(world, WBP, false))
            {
                world.BlockRequestScheduledTick(WBP);
            }
        }

        public override bool NeedsInit()
        {
            return true;
        }

        public override void OnScheduledTick(World world, Int3 WBP)
        {
            bool flowwed = Flow(world, WBP);
            if (flowwed)
            {
                world.BlockRequestScheduledTick(WBP);
            }
        }

        public override void OnNearBlockChanged(World world, Int3 WBP, BlockType changed_block_type, Int3 changed_WBP)
        {
            if (changed_block_type == BlockType.LavaBlock)
            {
                world.PlaceBlock(WBP, BlockType.ObsidionBlock);
            }
            else
            {
                bool canflow = Flow(world, WBP, false);
                if (canflow)
                {
                    world.BlockRequestScheduledTick(WBP);
                }
            }
        }

        public override bool IsNonSolid()
        {
            return true;
        }

        public override bool IsTranslucent()
        {
            return true;
        }

        public override BlockType GetBlockType()
        {
            return BlockType.WaterBlock;
        }
        public override string GetDisplayName()
        {
            return "Water";
        }
    }

    public class LavaBlock : WaterBlock {

        public LavaBlock()
        {
            FaceTextureIds = new BlockFaces<string>("Assets/Textures/lava.png");
        }

        public override void OnNearBlockChanged(World world, Int3 WBP, BlockType changed_block_type, Int3 changed_WBP)
        {
            if (changed_block_type == BlockType.WaterBlock)
            {
                world.PlaceBlock(WBP, BlockType.GreyStoneBlock);
            }
            else
            {
                bool canflow = Flow(world, WBP, false);
                if (canflow)
                {
                    world.BlockRequestScheduledTick(WBP);
                }
            }
        }
        public override BlockType GetBlockType()
        {
            return BlockType.LavaBlock;
        }
        public override string GetDisplayName()
        {
            return "Lava";
        }
    }
}
