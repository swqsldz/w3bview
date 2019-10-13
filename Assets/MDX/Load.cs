using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MDX
{
    public class Load : MonoBehaviour
    {
        public Data data;
        private Submesh submeshBuffer = new Submesh();

        // Start is called before the first frame update
        private void Start()
        {
            data = new Data();
            data.model = new Model();
            data.model.submeshCount = 0;
            data.model.submeshes = new List<Submesh>();

            ReadMDX();
            BuildModel();
        }

        private string filePath = @"D:\creeps\chenstormstout\chenstormstout.mdx";
        //private string filePath = @"D:\creeps\ancienthydra\ancienthydra.mdx";

        public void ReadMDX()
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    string MDLX = new string(br.ReadChars(4));
                    string VERS = new string(br.ReadChars(4));
                    int version = br.ReadInt32();
                    int version2 = br.ReadInt32();

                    // parse chunks
                    while (fs.Position < fs.Length)
                    {
                        string chunkType = new string(br.ReadChars(4));
                        int chunkSize = br.ReadInt32();
                        //print(chunkType + " " + fs.Position);
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
                                ReadTEXS(br, chunkSize);
                                break;
                            default:
                                fs.Position += chunkSize;
                                Debug.LogWarning("Skipping : " + chunkType + " at : " + br.BaseStream.Position);
                                break;
                        }
                    }
                }
            }
        }

        private void ReadMODL(BinaryReader br)
        {
            data.model.name = new string(br.ReadChars(336)).Trim();
            br.BaseStream.Position += 8;
            br.BaseStream.Position += 28;
        }

        private void ReadSEQS()
        {

        }

        private void ReadMTLS(BinaryReader br, int chunkSize)
        {
            long endPosition = br.BaseStream.Position + chunkSize;
            int sizeA = br.ReadInt32();
            br.BaseStream.Position += 88; // ???

            while (br.BaseStream.Position < endPosition)
            {
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
            }
        }

        private void ReadTEXS(BinaryReader br, int chunkSize)
        {
            br.BaseStream.Position += chunkSize;
        }

        private void ReadGEOS(BinaryReader br, int chunkSize)
        {
            long chunkStartPosition = br.BaseStream.Position;
            int unk = br.ReadInt32();
            //long endpos = br.BaseStream.Position + bytes - 4;
            while (br.BaseStream.Position < chunkStartPosition + chunkSize)
            {
                string subChunkType = new string(br.ReadChars(4));

                //print(subChunkType);

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
                        ReadUVBS(br, chunkSize, chunkStartPosition);
                        break;
                    default:
                        Debug.Log(" Unk subschunk : " + subChunkType);
                        break;
                }
            }
        }

        // read mesh vertices
        private void ReadVRTX(BinaryReader br)
        {
            data.model.submeshCount++;
            submeshBuffer = new Submesh();

            int numberOfVerts = br.ReadInt32();
            for (int k = 0; k < numberOfVerts; k++)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                float z = br.ReadSingle();
                submeshBuffer.vertices.Add(new Vector3(y, z, x));
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
                submeshBuffer.normals.Add(new Vector3(y, z, x));
            }
        }

        // primitive type ?
        private void ReadPTYP(BinaryReader br)
        {
            int numberOfptyp = br.ReadInt32();
            br.BaseStream.Position += numberOfptyp * 8;
            br.BaseStream.Position += 8;
        }

        // primitive corners ?
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
                submeshBuffer.triangles.Add(br.ReadInt16());
                submeshBuffer.triangles.Add(br.ReadInt16());
                submeshBuffer.triangles.Add(br.ReadInt16());
            }
        }

        // Vertex Groups
        private void ReadGNDX(BinaryReader br)
        {
            int numberOfGroups = br.ReadInt32();
            for (int k = 0; k < numberOfGroups; k++)
            {
                br.ReadByte();
            }
        }

        // Matrices Group Count
        private void ReadMTGC(BinaryReader br)
        {
            int numberOFMTGC = br.ReadInt32();
            for (int i = 0; i < numberOFMTGC; i++)
            {
                br.ReadInt32();
            }
        }

        // Bone Matrices
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
                submeshBuffer.tangents.Add(new Vector4(y, z, x, w));
            }
        }

        // ???
        private void ReadSKIN(BinaryReader br)
        {
            int numberOfSkins = br.ReadInt32(); // @ 100 to 50k
            for (int i = 0; i < numberOfSkins; i++)
            {
                byte b = br.ReadByte();
            }
        }

        // ???
        private void ReadUVAS(BinaryReader br)
        {
            print(br.ReadInt32());
            //br.ReadInt32();
        }

        // read mesh uvs
        private void ReadUVBS(BinaryReader br, int chunkSize, long chunkStartPosition)
        {
            // long chunkStartPosition = br.BaseStream.Position;
            int numberOfUVs = br.ReadInt32();
            for (int i = 0; i < numberOfUVs; i++)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                submeshBuffer.uvs.Add(new Vector2(x, 1 - y));
            }


            if (br.BaseStream.Position < chunkStartPosition + chunkSize)
                br.BaseStream.Position += 4; // skip 4 unk

            data.model.submeshes.Add(submeshBuffer);

            //int unk1 = br.ReadByte();
            //int unk2 = br.ReadByte();
            //int unk3 = br.ReadByte();
            //int unk4 = br.ReadByte();
            //print(unk1 + " " + unk2 + " " + unk3 + " " + unk4);
            //int unk1 = br.ReadInt16();
            //int unk2 = br.ReadInt16();
            //print(unk1 + " " + unk2);
            //br.BaseStream.Position += 4; // skip 4 unknown

            //print(br.BaseStream.Position);

            //BuildMesh();
            //ClearBuffer();
        }

        public void BuildModel()
        {
            Debug.Log("Building Model : " + data.model.name + " | " + "Submesh Count : " + data.model.submeshCount);

            // haven't determined which submesh is what lod yet so this is a hack~~
            int lod0SubmeshCount = data.model.submeshCount / 4;

            GameObject model = new GameObject();
            model.name = data.model.name;

            for (int sm = 0; sm < lod0SubmeshCount; sm++)
            {
                GameObject submesh = new GameObject();
                submesh.name = "Submesh_" + sm;
                submesh.transform.SetParent(model.transform);
                Mesh m = new Mesh();
                m.SetVertices(data.model.submeshes[sm].vertices);
                m.SetTriangles(data.model.submeshes[sm].triangles, 0);
                m.SetNormals(data.model.submeshes[sm].normals);
                m.SetTangents(data.model.submeshes[sm].tangents);
                m.SetUVs(0, data.model.submeshes[sm].uvs);
                MeshRenderer meshRenderer = submesh.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = submesh.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                Material material = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterial = material;
            }
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
}
