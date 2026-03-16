using blockengine.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine.Items
{
    public enum ItemType : int
    {
        //NON BLOCK ITEMS
        ErrorItem = 0,
        TestPickaxe,


        //BLOCK ITEMS
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
        MushroomGrass,
        MushroomStone,
        MushroomLog,
        MushroomBlock,

        Count
    }
    public enum ItemRenderType
    {
        ItemRender = 0,
        BlockRender,
        ModelRender
    }

    public abstract class Item
    {
        public string texture = "Assets/Textures/missing.png";
        public string viewmodel = "Assets/Models/block.obj";
        public BlockType blockrender = BlockType.ErrorBlock;
        public ItemRenderType renderype = ItemRenderType.ItemRender;

        public virtual int GetMaxStack()
        {
            return 999;
        }
        public abstract ItemType GetItemType();
        public abstract string GetDisplayName();

        public virtual void OnRightClicked(PlayerEntity from,InventorySlot myslot, Inventory inv, World world)
        {
            
        }
    }


    //ITEMS

    public class BasicBlockItem : Item
    {
        private ItemType item_type;
        private string display_name;
        public BasicBlockItem(BlockType blocktype,string name, ItemType type)
        {
            item_type = type;
            display_name = name;

            blockrender = blocktype;
            renderype = ItemRenderType.BlockRender;

            var blockdef = Globals.BlockDefinitions[blocktype];
            texture = blockdef.FaceTextureIds.FORWARD;
            viewmodel = "Assets/Models/block.obj";
        }

        public override ItemType GetItemType()
        {
            return item_type;
        }
        public override string GetDisplayName()
        {
            return display_name;
        }

        public override void OnRightClicked(PlayerEntity from, InventorySlot myslot, Inventory inv, World world)
        {
            from.PlaceBlock(blockrender);
            myslot.DecrimentCount();
        }
    }

    public class ErrorItem : Item
    {

        public override ItemType GetItemType()
        {
            return ItemType.ErrorItem;
        }
        public override string GetDisplayName()
        {
            return "Error";
        }
    }

    public class TestPickaxe : Item
    {
        public TestPickaxe()
        {
            renderype = ItemRenderType.ItemRender;
            texture = "Assets/Textures/blue_pickaxe.png";
            viewmodel = "Assets/Models/pickaxe.obj";
        }

        public override int GetMaxStack()
        {
            return 1;
        }

        public override ItemType GetItemType()
        {
            return ItemType.TestPickaxe;
        }
        public override string GetDisplayName()
        {
            return "Test Pickaxe";
        }
    }
}
