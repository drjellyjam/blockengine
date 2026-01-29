using blockengine.Entitys;
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
    public class BlockFaces<T>
    {
        public T TOP;
        public T BOTTOM;
        public T LEFT;
        public T RIGHT;
        public T FORWARD;
        public T BACKWARD;

        public BlockFaces(T _top, T _bottom, T _left, T _right, T _forward, T _backward)
        {
            TOP = _top;
            BOTTOM = _bottom;
            LEFT = _left;
            RIGHT = _right;
            FORWARD = _forward;
            BACKWARD = _backward;
        }

        public BlockFaces(T _all)
        {
            TOP = _all;
            BOTTOM = _all;
            LEFT = _all;
            RIGHT = _all;
            FORWARD = _all;
            BACKWARD = _all;
        }

        public BlockFaces(T _top,T _bottom,T _sides)
        {
            TOP = _top;
            BOTTOM = _bottom;
            LEFT = _sides;
            RIGHT = _sides;
            FORWARD = _sides;
            BACKWARD = _sides;
        }
    }

    public abstract class Block //base block class
    {
        public BoxCollider collider = new BoxCollider(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        public BlockModel? BlockModel = null;
        public BlockFaces<string> FaceTextureIds = new BlockFaces<string>("Assets/Textures/missing.png");
        public abstract string GetDisplayName();
        public virtual bool IsExists() { return true; }
        public virtual bool IsTranslucent() { return false; }
        public virtual bool IsNonSolid() { return false; }
        public virtual float GetDurability() { return 1f; }

        public abstract BlockType GetBlockType();

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
        }

        public virtual void OnBlockBreak(World world,Int3 WBP)
        {
            world.AddEntity(new DroppedItemEntity(world, "DroppedItem", WBP.to_vector3() + (Vector3.One * 0.5f)));
        }
        public virtual void OnNearBlockChanged(World world,Int3 WBP, BlockType changed_block_type,Int3 changed_WBP) //A block around this one was placed or removed
        {

        }
    }
    public class BlockModel
    {
        public string model_file;
        public string model_texture;
        public BlockFaces<bool> gen_faces;
        public Vector3 model_scale;
        public Vector3 model_offset;
        public BlockModel(string _model_file, string _model_texture,Vector3 _model_scale,Vector3 _model_offset, BlockFaces<bool> _gen_faces)
        {
            model_file = _model_file;
            gen_faces = _gen_faces;
            model_texture = _model_texture;
            model_scale = _model_scale;
            model_offset = _model_offset;
        }
    }
    public class BlockDefinition
    {
        public string Name;
        public bool Exists;
        public bool NonSolid;
        public bool Translucent;
        public BlockModel? BlockModel;
        public Block BlockClass;

        public float durability;

        public BoxCollider Collider;
        public BlockFaces<string> FaceTextureIds;
        public BlockDefinition(Block _block_class, string _name,bool _exists = true, bool _nonsolid = false, bool _translucent = false, float _durability = 1f, BlockModel? _blockmodel = null, BlockFaces<string>? _face_texture_ids = null, Vector3? _collider_position = null, Vector3? _collider_min = null, Vector3? _collider_max = null)
        {
            Name = _name;
            Exists = _exists;
            NonSolid = _nonsolid;
            Translucent = _translucent;
            BlockModel = _blockmodel;
            BlockClass = _block_class;

            durability = _durability;

            if (_face_texture_ids == null)
            {
                _face_texture_ids = new BlockFaces<string>("Assets/Textures/missing.png");
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
