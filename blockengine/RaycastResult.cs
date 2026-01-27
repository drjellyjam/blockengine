using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class RaycastResult
    {
        public Vector3 Hit;
        public Int3 Normal;
        public ChunkBlock Block;
        public Int3 BlockPosition;
        public BoxCollider Collider;
        public RaycastResult(Vector3 _hit, Int3 _norm, ChunkBlock _block, Int3 _blockpos, BoxCollider _collider) {
            Hit = _hit; 
            Normal = _norm; 
            Block = _block; 
            BlockPosition = _blockpos;
            Collider = _collider;
        }
    }
}
