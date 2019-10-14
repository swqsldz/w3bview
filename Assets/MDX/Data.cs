using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDX
{
    public class Data
    {
        public Model model = new Model();
    }

    // Object
    public class Model
    {
        public string name;
        public int submeshCount = 0;
        public List<Submesh> submeshes = new List<Submesh>();
        public List<Material> materials = new List<Material>();
        public List<string> textures = new List<string>();
    }

    // Submesh
    public class Submesh
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector4> tangents = new List<Vector4>();
        public List<Vector2> uvs = new List<Vector2>();
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public string name;
        public int id;
    }

    // Material
    public class Material
    {
        public int unk0;                      // if it's larger than 1 then a KMTA chunk follows, otherwise just shader reference
        public int fmode; // 
        public int shade; // 
        public int unk2; // ??
        public int texture;  // 
        public int unk3; // ?? always -1
        public float alpha;  // 0
        public byte[] flags; // ?? must be material flags
    }

}
