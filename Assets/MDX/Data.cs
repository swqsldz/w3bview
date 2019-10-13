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
    }

    // Submesh
    public class Submesh
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector4> tangents = new List<Vector4>();
        public List<Vector2> uvs = new List<Vector2>();
    }

}
