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
        public List<AnimationSequence> animationSequences = new List<AnimationSequence>();
        public List<Bone> bones = new List<Bone>();
        public List<Vector3> pivots = new List<Vector3>();
        public List<Attachment> attachments = new List<Attachment>();
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
        public int geoBone;
        public List<int> boneGroups;
        public List<List<int>> bones = new List<List<int>>();
        public List<int> groups;
        public BoneWeight[] boneWeights;
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

    // Animation Sequence
    public class AnimationSequence
    {
        public string name;
        public int seqIntStart;
        public int seqIntEnd;
        public float seqMoveSpeed;
        public int seqNoLoop;
        public float seqRarity;
        public int seqLong;
    }

    // Bone
    public class Bone
    {
        public string name;
        public int index;
        public int parent;
        public int mesh;
        public List<TrackPoint> translation;
        public List<TrackPoint> rotation;
        public List<TrackPoint> scale;
        public int id;
        public int aid;
        public int end;
        public int no;
    }

    // Track Point
    public class TrackPoint
    {
        public int time;
        public object point;
        public object inTan;
        public object outTan;
    }

    public class Attachment
    {
        public string name;
        public int index;
        public int parent;
        public List<TrackPoint> translation;
        public List<TrackPoint> rotation;
        public List<TrackPoint> scale;
    }

}
