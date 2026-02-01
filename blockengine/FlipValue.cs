using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockengine
{
    public class FlipMesh
    {
        public Mesh mesh1;
        public Mesh mesh2;
        public bool mesh1_uploaded;
        public bool mesh2_uploaded;

        public bool active = true;

        public FlipMesh()
        {
            mesh1_uploaded = false;
            mesh2_uploaded = false;
            mesh1 = new Mesh();
            mesh2 = new Mesh();
        }

        public Mesh GetActiveMesh()
        {
            if (active) { return mesh1; }
            return mesh2;
        }

        public Mesh GetInactiveMesh()
        {
            if (!active) { return mesh1; }
            return mesh2;
        }

        public bool GetActiveMeshUploaded()
        {
            if (active) { return mesh1_uploaded; }
            return mesh2_uploaded;
        }
        public bool GetInactiveMeshUploaded()
        {
            if (!active) { return mesh1_uploaded; }
            return mesh2_uploaded;
        }

        public void Flip()
        {
            active = !active;
        }

        public void UploadMesh()
        {
            if (!GetActiveMeshUploaded())
            {
                Mesh mesh = GetActiveMesh();
                Raylib.UploadMesh(ref mesh, false);
                if (active) { mesh1_uploaded = true; } else { mesh2_uploaded = true; }
            }
        }

        public void UnloadMesh()
        {
            if (GetActiveMeshUploaded())
            {
                Mesh mesh = GetActiveMesh();
                Raylib.UnloadMesh(mesh);
                mesh.VaoId = 0;
                if (active) { mesh1_uploaded = false; } else { mesh2_uploaded = false; }
            }
        }

        public void UnloadAllMeshes()
        {
            if (mesh1_uploaded)
            {
                Raylib.UnloadMesh(mesh1);
                mesh1.VaoId = 0;
                mesh1_uploaded = false;
            }
            if (mesh2_uploaded)
            {
                Raylib.UnloadMesh(mesh2);
                mesh2.VaoId = 0;
                mesh2_uploaded = false;
            }
        }
    }
}
