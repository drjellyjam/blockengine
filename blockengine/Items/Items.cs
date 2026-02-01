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
        ErrorItem = 0
    }
    public enum ItemRenderType
    {
        ItemRender = 0,
        BlockRender
    }

    public abstract class Item
    {
        public string texture = "Assets/Textures/missing.png";
        public string viewmodel = "Assets/Models/block.obj";
        public BlockType blockrender = BlockType.ErrorBlock;
        public ItemRenderType renderype = ItemRenderType.ItemRender;

        public abstract ItemType GetItemType();
        public abstract string GetDisplayName();

    }
}
