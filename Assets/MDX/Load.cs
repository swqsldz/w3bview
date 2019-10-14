using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using UnityEditor;

namespace MDX
{
    public class Load : MonoBehaviour
    {
        public Data data;
        public UnityEngine.Material defaultMaterial;
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

        string filePath = @"D:\creeps\chenstormstout\chenstormstout.mdx";
        //string filePath = @"D:\creeps\ancienthydra\ancienthydra.mdx";
        //string filePath = @"D:\creeps\murlocwarrior\murlocwarrior.mdx";
        
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
                                ReadMTLS(br, chunkSize); // skip WIP
                                //fs.Position += chunkSize;
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
            int unk1 = br.ReadInt32();
            br.BaseStream.Position += 88; // ???
            int totalLayers = 0;        // seems to coincide with number of submeshes

            while (br.BaseStream.Position < endPosition)
            {
                string LAYS = new string(br.ReadChars(4));

                MDX.Material material = new MDX.Material();
                material.unk0 = br.ReadInt32();                      // if it's larger than 1 then a KMTA chunk follows, otherwise just shader reference
                material.fmode = br.ReadInt32();
                material.shade = br.ReadInt32();
                material.unk2 = br.ReadInt32();
                material.texture = br.ReadInt32();
                material.unk3 = br.ReadInt32();                     // ?? always -1
                material.alpha = br.ReadSingle();                   // 0
                material.flags = br.ReadBytes(8);
                data.model.materials.Add(material);

                //print("unk0: " + material.unk0 + " | " + "fmode: " + material.fmode + " | " + "shade: " + material.shade + " | " + "unk2: " + material.unk2 + " | " + "texture: " + material.texture + " | " + "unk3: " + material.unk3 + " | " + "alpha: " + material.alpha);

                if (material.unk0 > 1)
                {
                    string KMTA = new string(br.ReadChars(4));
                    int nunks = br.ReadInt32();
                    int unk4 = br.ReadInt32();
                    int type = br.ReadInt32(); // ?? always -1 ?
                    for (int i = 0; i < nunks; i++)
                    {
                        int unk5 = br.ReadInt32();
                        int unk6 = br.ReadInt32();
                    }
                    br.BaseStream.Position += 160; // ??

                    if (br.BaseStream.Position < endPosition)
                    {
                        int unk7 = br.ReadInt32();
                        int unk8 = br.ReadInt32();
                        int unk9 = br.ReadInt32();
                        string shaderName = new string(br.ReadChars(80));
                    }
                }
                else
                {
                    int unk7 = br.ReadInt32();
                    int unk8 = br.ReadInt32();
                    int unk9 = br.ReadInt32();
                    string shaderName = new string(br.ReadChars(80));
                }
                totalLayers++;
            }
        }

        private void ReadTEXS(BinaryReader br, int chunkSize)
        {
            int totalTextures = chunkSize / 268;        // total number of texture files, 268 is the fixed string size
            for (int t = 0; t < totalTextures; t++)
            {
                br.BaseStream.Position += 4; // skip 4 empty bytes
                string texturePath = ReadString(br, 264);//new string(br.ReadChars(264)).Trim();
                //print(t + " - " +texturePath);
                data.model.textures.Add(texturePath);
            }
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

            // checking out the 4 bytes before each VRTX
            br.BaseStream.Position -= 8; // going back 8 because of the VRTX ID
            submeshBuffer.unk1 = br.ReadByte();
            submeshBuffer.unk2 = br.ReadByte();
            submeshBuffer.unk3 = br.ReadByte();
            submeshBuffer.unk4 = br.ReadByte();         // always 0
            //print(submeshBuffer.unk1 + " - " + submeshBuffer.unk2 + " - " + submeshBuffer.unk3 + " - " + submeshBuffer.unk4);
            br.BaseStream.Position += 4; // going over VRTX ID
            ////

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

        // vertex Groups
        private void ReadGNDX(BinaryReader br)
        {
            int numberOfGroups = br.ReadInt32();
            for (int k = 0; k < numberOfGroups; k++)
            {
                br.ReadByte();
            }
        }

        // matrices Group Count
        private void ReadMTGC(BinaryReader br)
        {
            int numberOFMTGC = br.ReadInt32();
            for (int i = 0; i < numberOFMTGC; i++)
            {
                br.ReadInt32();
            }
        }

        // bone matrices
        private void ReadMATS(BinaryReader br)
        {
            int numberOfMats = br.ReadInt32();
            for (int i = 0; i < numberOfMats; i++)
            {
                br.ReadInt32();
            }
            submeshBuffer.id = br.ReadInt32();              // submesh ID, used for correctly assigning materials
            br.ReadBytes(12);                               // same ID repeats 3 times
            submeshBuffer.name = ReadString(br, 112);       // submesh name
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

        // ??? vertex weights maybe
        private void ReadSKIN(BinaryReader br)
        {
            int numberOfSkins = br.ReadInt32(); // @ 100 to 50k
            for (int i = 0; i < numberOfSkins; i++)
            {
                byte b = br.ReadByte();
            }
        }

        // ??? second uv channel maybe, if that int32 is >1 ?
        private void ReadUVAS(BinaryReader br)
        {
            br.ReadInt32();
        }

        // read mesh uvs
        private void ReadUVBS(BinaryReader br, int chunkSize, long chunkStartPosition)
        {
            int numberOfUVs = br.ReadInt32();
            for (int i = 0; i < numberOfUVs; i++)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                submeshBuffer.uvs.Add(new Vector2(x, y));
            }
            if (br.BaseStream.Position < chunkStartPosition + chunkSize)
                br.BaseStream.Position += 4; // skip 4 unk
            data.model.submeshes.Add(submeshBuffer);
        }

        public void BuildModel()
        {
            Debug.Log("Building Model : " + data.model.name + " | " + "Submesh Count : " + data.model.submeshCount);

            // filter out LOD1 LOD2 LOD3 // the lazy way
            List<int> LoD0Indices = new List<int>();
            for (int i = 0; i < data.model.submeshCount; i++)
            {
                if (!data.model.submeshes[i].name.Contains("LOD"))
                {
                    LoD0Indices.Add(i);
                }
            }

            // Object
            GameObject model = new GameObject();
            model.name = data.model.name;

            // Submeshes
            for (int sm = 0; sm < LoD0Indices.Count; sm++)
            {
                int index = LoD0Indices[sm];

                // Object
                GameObject submesh = new GameObject();
                submesh.name = data.model.submeshes[index].name;
                submesh.transform.SetParent(model.transform);

                // Mesh
                Mesh m = new Mesh();
                m.SetVertices(data.model.submeshes[index].vertices);
                m.SetTriangles(data.model.submeshes[index].triangles, 0);
                m.SetNormals(data.model.submeshes[index].normals);
                m.SetTangents(data.model.submeshes[index].tangents);
                m.SetUVs(0, data.model.submeshes[index].uvs);
                MeshRenderer meshRenderer = submesh.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = submesh.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                
                // Texture
                string folderPath = Path.GetDirectoryName(filePath);
                string textureName_ = data.model.textures[data.model.materials[data.model.submeshes[index].id].texture];      // *sweat in RP*
                textureName_ = PathHelper.GetFileName(textureName_);
                textureName_ = textureName_.Remove(textureName_.Length - 12);

                string diffuseTexturePath = folderPath + @"\" + textureName_ + "diffuse" + ".dds";
                string emissiveTexturePath = folderPath + @"\" + textureName_ + "emissive" + ".dds";
                string normalTexturePath = folderPath + @"\" + textureName_ + "normal" + ".dds";
                string ormTexturePath = folderPath + @"\" + textureName_ + "orm" + ".dds";
                // Material
                UnityEngine.Material mat = new UnityEngine.Material(defaultMaterial);
                
                if (File.Exists(diffuseTexturePath))
                {
                    Texture2D texture = DDS.ImportToTexture2D(diffuseTexturePath);
                    mat.SetTexture("_MainTex", texture);
                }
                if (File.Exists(emissiveTexturePath))
                {
                    mat.EnableKeyword("_EMISSION");
                    Texture2D texture = DDS.ImportToTexture2D(emissiveTexturePath);
                    mat.SetTexture("_EmissionMap", texture);
                }
                if (File.Exists(normalTexturePath))
                {
                    mat.EnableKeyword("_NORMALMAP");
                    Texture2D texture = DDS.ImportToTexture2D(normalTexturePath);
                    mat.SetTexture("_BumpMap", texture);
                }
                if (File.Exists(ormTexturePath))
                {
                    mat.EnableKeyword("_METALLICGLOSSMAP");
                    Texture2D texture = DDS.ImportToTexture2D(ormTexturePath);
                    mat.SetTexture("_MetallicGlossMap", texture);
                }
                mat.SetFloat("_Glossiness", 0.44f);
                mat.SetFloat("_GlossMapScale", 0.44f);
                meshRenderer.material = mat;
            }
        }

        public string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public string ReadString(BinaryReader br, int size)
        {
            List<char> collectedChars = new List<char>();
            for (int i = 0; i < size; i++)
            {
                char c = br.ReadChar();
                if (c != '\0')
                {
                    collectedChars.Add(c);
                }
            }
            return new string(collectedChars.ToArray());
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
