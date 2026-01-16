using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public enum BlockFace : int
    {
        Forward = 0,
        Backward = 1,
        Right = 2,
        Left = 3,
        Top = 4,
        Bottom = 5
    }
    public class BlockFaces
    {
        public string TOP;
        public string BOTTOM;
        public string LEFT;
        public string RIGHT;
        public string FORWARD;
        public string BACKWARD;

        public BlockFaces(string _top, string _bottom, string _left, string _right, string _forward, string _backward)
        {
            TOP = _top;
            BOTTOM = _bottom;
            LEFT = _left;
            RIGHT = _right;
            FORWARD = _forward;
            BACKWARD = _backward;
        }

        public BlockFaces(string _all)
        {
            TOP = _all;
            BOTTOM = _all;
            LEFT = _all;
            RIGHT = _all;
            FORWARD = _all;
            BACKWARD = _all;
        }

        public BlockFaces(string _top,string _bottom,string _sides)
        {
            TOP = _top;
            BOTTOM = _bottom;
            LEFT = _sides;
            RIGHT = _sides;
            FORWARD = _sides;
            BACKWARD = _sides;
        }
    }

    public class Block
    {
        public string definition_ID;
        public float health;

        public Block(string ID = "AIR")
        {
            SetDefinition(ID);
        }

        public void SetDefinition(string ID)
        {
            definition_ID = ID;

            var _def = Globals.BlockDefinitions[definition_ID];

            health = _def.durability;
        }

        public BlockDefinition GetDefinition()
        {
            return Globals.BlockDefinitions[definition_ID];
        }
    }

    public class BlockDefinition
    {
        public string Name;
        public bool Exists;
        public bool NonSolid;
        public bool Translucent;
        public float durability;

        public BoxCollider Collider;
        public BlockFaces FaceTextureIds;
        public BlockDefinition(string _name,bool _exists = true, bool _nonsolid = false, bool _translucent = false, float _durability = 1f, BlockFaces? _face_texture_ids = null, Vector3? _collider_position = null, Vector3? _collider_min = null, Vector3? _collider_max = null)
        {
            Name = _name;
            Exists = _exists;
            NonSolid = _nonsolid;
            Translucent = _translucent;
            durability = _durability;

            if (_face_texture_ids == null)
            {
                _face_texture_ids = new BlockFaces("Assets/Textures/missing.png");
            }
            FaceTextureIds = _face_texture_ids;
            if (_collider_position == null) {
                _collider_position = new Vector3(0.5f,0.5f,0.5f);
            }
            if (_collider_min == null)
            {
                _collider_min = new Vector3(-0.5f,-0.5f,-0.5f);
            }
            if (_collider_max == null)
            {
                _collider_max = new Vector3(0.5f, 0.5f, 0.5f);
            }
            Collider = new BoxCollider( (Vector3)_collider_position, (Vector3)_collider_min, (Vector3)_collider_max);
        }

        public UV GetBlockUV(BlockFace face)
        {
            switch (face)
            {
                case BlockFace.Left:
                    return TextureHandler.GetTextureUV(FaceTextureIds.LEFT);
                case BlockFace.Right:
                    return TextureHandler.GetTextureUV(FaceTextureIds.RIGHT);
                case BlockFace.Top:
                    return TextureHandler.GetTextureUV(FaceTextureIds.TOP);
                case BlockFace.Bottom:
                    return TextureHandler.GetTextureUV(FaceTextureIds.BOTTOM);
                case BlockFace.Forward:
                    return TextureHandler.GetTextureUV(FaceTextureIds.FORWARD);
                case BlockFace.Backward:
                    return TextureHandler.GetTextureUV(FaceTextureIds.BACKWARD);
                default:
                    return TextureHandler.GetTextureUV("Assets/Textures/missing.png");
            }

            /*
            int atlaspixelsize_x = 512;
            int atlaspixelsize_y = 512;
            int atlastilesize = 16;
            int atlassize_x = atlaspixelsize_x / atlastilesize;
            int atlassize_y = atlaspixelsize_y / atlastilesize;
            int idx;
            switch (face)
            {
                case BlockFace.Left:
                    idx = FaceTextureIds.LEFT;
                    break;
                case BlockFace.Right:
                    idx = FaceTextureIds.RIGHT;
                    break;
                case BlockFace.Top:
                    idx = FaceTextureIds.TOP;
                    break;
                case BlockFace.Bottom:
                    idx = FaceTextureIds.BOTTOM;
                    break;
                case BlockFace.Forward:
                    idx = FaceTextureIds.FORWARD;
                    break;
                case BlockFace.Backward:
                    idx = FaceTextureIds.BACKWARD;
                    break;
                default:
                    idx = 0;
                    break;
            }

            float x, y;
            x = idx % atlassize_x;
            y = (idx / atlassize_x) % atlassize_y;

            x = x * atlastilesize;
            y = y * atlastilesize;

            float xmin, ymin, xmax, ymax;
            xmin = x;
            ymin = y+atlastilesize; ///flipped upside down
            xmax = x+atlastilesize;
            ymax = y; ///flipped upside down

            return new UV(xmin/ atlaspixelsize_x,ymin/ atlaspixelsize_y, xmax/ atlaspixelsize_x, ymax/ atlaspixelsize_y);
            */
        }
    }
}
