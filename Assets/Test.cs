using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Test : MonoBehaviour
{
    // mesh buffer
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector4> tangents = new List<Vector4>();
    List<Vector2> uvs = new List<Vector2>();

    // Start is called before the first frame update
    private void Start()
    {
        ReadMDX();
    }

    //public string filePath = @"D:\creeps\chenstormstout\chenstormstout.mdx";
    private string filePath = @"D:\creeps\chenstormstoutearth\chenstormstoutearth.mdx";
    
    public bool stopWhileLoop = false;

    public void ReadMDX()
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                string MDLX = new string(br.ReadChars(4));
                string VERS = new string(br.ReadChars(4));
                int ofsTags = br.ReadInt32();
                int nTags = br.ReadInt32();

                // parse chunks
                while (fs.Position < fs.Length || !stopWhileLoop)
                {
                    string chunkType = new string(br.ReadChars(4));
                    int chunkSize = br.ReadInt32();

                    switch (chunkType)
                    {
                        case "MODL":
                            ReadMODL(br);
                            break;
                        case "SEQS":
                            fs.Position += chunkSize;
                            break;
                        case "MTLS":
                            //ReadMTLS(br, chunkSize); // skip WIP
                            fs.Position += chunkSize;
                            break;
                        case "GEOS":
                            ReadGEOS(br, chunkSize);
                            break;
                        case "TEXS":
                            fs.Position += chunkSize;
                            break;
                        default:
                            fs.Position += chunkSize;
                            Debug.LogWarning("Skipping : "  + " at : " + br.BaseStream.Position);
                            break;
                    }
                }
            }
        }
    }

    private void ReadMODL(BinaryReader br)
    {
        string modelName = new string(br.ReadChars(336));
        br.BaseStream.Position += 8;
        br.BaseStream.Position += 28;
    }

    private void ReadSEQS()
    {

    }

    private void ReadMTLS(BinaryReader br, int chunkSize)
    {
        long endPositionA = chunkSize + br.BaseStream.Position;
        int i = 1;
        do
        {
            int sizeA = br.ReadInt32();
            br.BaseStream.Position += 88; // unknown
            print(br.BaseStream.Position);
            string LAYS = new string(br.ReadChars(4));
            if (LAYS == "LAYS")
            {
                int numberOfLAYS = br.ReadInt32();
                for (int j = 0; j < numberOfLAYS; j++)
                {
                    int numberOfSubLays = br.ReadInt32(); // if this is >1 then KMTA exists ~~
                    long endPositionB = numberOfSubLays + br.BaseStream.Position - 4;

                    int fmode = br.ReadInt32(); // 
                    int shade = br.ReadInt32(); // 

                    int unk = br.ReadInt32(); // ??

                    int texture = br.ReadInt32();  // 

                    int unk2 = br.ReadInt32(); // ?? always -1

                    float alpha = br.ReadSingle();  // 0

                    br.BaseStream.Position += 8; // ?? big numbers

                    string KMTA = new string(br.ReadChars(4));

                    if (KMTA == "KMTA")
                    //if (numberOfSubLays > 1)
                    {
                        //string KMTA = new string(br.ReadChars(4));
                        int nunks = br.ReadInt32();
                        br.BaseStream.Position += 4; // ?? always 1 ?
                        uint ltype = br.ReadUInt32(); // ?? always -1 ?

                        for (int k = 0; k < nunks; k++)
                        {
                            br.BaseStream.Position += 8; // ??
                        }

                        br.BaseStream.Position += 160; // ??

                        // if it has shader name
                        br.BaseStream.Position += 12; // ??

                        string shaderName = new string(br.ReadChars(80));
                        print(shaderName);
                    }
                    else
                    {
                        br.BaseStream.Position += 4; // ??
                        string shaderName = new string(br.ReadChars(80));
                        print(shaderName);
                    }

                }
            }
            i++;
        } while (br.BaseStream.Position < endPositionA);
    }

    private void ReadTEXS()
    {

    }

    private void ReadGEOS(BinaryReader br, int chunkSize)
    {
        long chunkStartPosition = br.BaseStream.Position;
        int bytes = br.ReadInt32();
        long endpos = br.BaseStream.Position + bytes - 4;
        while (br.BaseStream.Position < chunkStartPosition + chunkSize)
        {
            string subChunkType = new string(br.ReadChars(4));

            switch (subChunkType)
            {
                case "VRTX":
                    ReadVRTX(br);
                    break;
                case "NRMS":
                    ReadNRMS(br);
                    break;
                case "PTYP":
                    ReadPTYP(br);
                    break;
                case "PCNT":
                    ReadPCNT(br);
                    break;
                case "PVTX":
                    ReadPVTX(br);
                    break;
                case "GNDX":
                    ReadGNDX(br);
                    break;
                case "MTGC":
                    ReadMTGC(br);
                    break;
                case "MATS":
                    ReadMATS(br);
                    break;
                case "TANG":
                    ReadTANG(br);
                    break;
                case "SKIN":
                    ReadSKIN(br);
                    break;
                case "UVAS":
                    ReadUVAS(br);
                    break;
                case "UVBS":
                    ReadUVBS(br);
                    break;
                default:
                    stopWhileLoop = true;
                    Debug.Log(" Unk subschunk : " + subChunkType);
                    break;
            }
        }
    }

    // read mesh vertices
    private void ReadVRTX(BinaryReader br)
    {
        int numberOfVerts = br.ReadInt32();
        for (int k = 0; k < numberOfVerts; k++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            vertices.Add(new Vector3(y, z, x));
        }
    }

    // read mesh normals
    private void ReadNRMS(BinaryReader br)
    {
        int numberOfNormals = br.ReadInt32();
        for (int k = 0; k < numberOfNormals; k++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            normals.Add(new Vector3(y, z, x));
        }
    }

    // ???
    private void ReadPTYP(BinaryReader br)
    {
        int numberOfptyp = br.ReadInt32();
        br.BaseStream.Position += numberOfptyp * 8;
        br.BaseStream.Position += 8;
    }

    // ???
    private void ReadPCNT(BinaryReader br)
    {
        int numberOfpcnt = br.ReadInt32();
        br.BaseStream.Position += numberOfpcnt * 4;
    }

    // read mesh triangles
    private void ReadPVTX(BinaryReader br)
    {
        int numberOfFaces = br.ReadInt32();
        for (int k = 0; k < numberOfFaces / 3; k++)
        {
            triangles.Add( br.ReadInt16());
            triangles.Add( br.ReadInt16());
            triangles.Add( br.ReadInt16());
        }
    }

    // ???
    private void ReadGNDX(BinaryReader br)
    {
        int numberOfGroups = br.ReadInt32();
        for (int k = 0; k < numberOfGroups; k++)
        {
            br.ReadByte();
        }
    }

    // ???
    private void ReadMTGC(BinaryReader br)
    {
        int numberOfBoneGroups = br.ReadInt32();
        for (int i = 0; i < numberOfBoneGroups; i++)
        {
            br.ReadInt32();
        }
    }

    // ??? something for bones
    private void ReadMATS(BinaryReader br)
    {
        int numberOfMats = br.ReadInt32();
        for (int i = 0; i < numberOfMats; i++)
        {
            br.ReadInt32();
        }
        br.BaseStream.Position += 16; // skip 16
        br.BaseStream.Position += 112; // skip 112 // contains a string too
    }

    // read mesh tangents
    private void ReadTANG(BinaryReader br)
    {
        int numberOfTangents = br.ReadInt32();
        for (int i = 0; i < numberOfTangents; i++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            float w = br.ReadSingle();
            tangents.Add(new Vector4(y, z, x, w));
        }
    }

    // ???
    private void ReadSKIN(BinaryReader br)
    {
        int numberOfSkins = br.ReadInt32();
        for (int i = 0; i < numberOfSkins; i++)
        {
            br.ReadByte();
        }
    }

    // ???
    private void ReadUVAS(BinaryReader br)
    {
        //print(br.ReadInt32());
        br.ReadInt32();
    }

    // read mesh uvs
    private void ReadUVBS(BinaryReader br)
    {
        int numberOfUVs = br.ReadInt32();
        for (int i = 0; i < numberOfUVs; i++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            uvs.Add(new Vector2(x, 1 - y));
        }

        br.BaseStream.Position += 4; // skip 4 unknown

        BuildMesh();
        ClearBuffer();
    }

    private void BuildMesh()
    {
        GameObject test = new GameObject();
        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetNormals(normals);
        m.SetTangents(tangents);
        m.SetUVs(0 ,uvs);
        MeshRenderer meshRenderer = test.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = test.AddComponent<MeshFilter>();
        meshFilter.mesh = m;
        Material material = new Material(Shader.Find("Standard"));
        meshRenderer.sharedMaterial = material;

    }

    private void ClearBuffer()
    {
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        tangents.Clear();
        uvs.Clear();
    }


    /*
            --MDLX HEAD
            0x584C444D: #MDLX

            0x53524556: #VERS
            0x4C444F4D: #MODL
            0x53514553: #SEQS

            0x53424C47: #GLBS

            --MATERIALS
            0x534C544D: #MTLS
            0x41544D4B: #KMTA
            0x53584554: #TEXS


            --GEOMETRY
            0x534F4547: #GEOS

            0x58545256: #VRTX
            0x534D524E: #NRMS
            0x50595450: #PTYP
            0x544E4350: #PCNT
            0x58545650: #PVTX
            0x58444E47: #GNDX
            0x4347544D: #MTGC
            0x5354414D: #MATS
            0x53415655: #UVAS
            0x53425655: #UVBS

            --GEOMETRY ANIMATION
            0x414F4547: #GEOA

            --BONE
            0x454E4F42: #BONE

            --LIGHT
            0x4554494C: #LITE

            --HELPER
            0x504C4548: #HELP

            --ATTACHMENT
            0x48435441: #ATCH

            --PIVOT
            0x54564950: #PIVT

            --PARTICLE EMITTER
            0x32455250: #PRE2

            --EVENT
            0x53545645: #EVTS
            0x5456454B: #KEVT

            --COLLISION
            0x44494C43: #CLID

            --KEYFRAME TRACK
            0x4F41474B: #KGAO

            0x5254474B: #KGTR
            0x5452474B: #KGRT
            0x4353474B: #KGSC

            0x56414C4B: #KLAV
            0x5654414B: #KATV
            0x5632504B: #KP2V

            0x5632504B: #KP2E

            --UNKNOWN
            default: #UNKNOWN
    */


}

