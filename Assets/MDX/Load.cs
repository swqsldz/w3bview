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
        public static Data data;
        public UnityEngine.Material defaultMaterial;
        public Debug_Bones debugBones;
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

            if (debugBones.gameObject.activeSelf)
            {
                debugBones.BuildPivots_DEBUG();
            }
        }

        string filePath = @"D:\creeps\chenstormstout\chenstormstout.mdx";
        //string filePath = @"D:\creeps\ancienthydra\ancienthydra.mdx";
        //string filePath = @"D:\creeps\murlocwarrior\murlocwarrior.mdx";
        //string filePath = @"D:\creeps\tuskar\tuskar.mdx";
        

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
                                ReadSEQS(br, chunkSize);
                                break;
                            case "MTLS":
                                ReadMTLS(br, chunkSize);
                                break;
                            case "GEOS":
                                ReadGEOS(br, chunkSize);
                                break;
                            case "TEXS":
                                ReadTEXS(br, chunkSize);
                                break;
                            case "GEOA":
                                ReadGEOA(br, chunkSize);
                                break;
                            case "BONE":
                                ReadBONE(br, chunkSize);
                                break;
                            case "ATCH":
                                ReadATCH(br, chunkSize);
                                break;
                            case "PIVT":
                                ReadPIVT(br, chunkSize);
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

        // read model info
        private void ReadMODL(BinaryReader br)
        {
            data.model.name = ReadString(br, 336);
            br.BaseStream.Position += 8;
            br.BaseStream.Position += 28;
        }

        // read animation sequences
        private void ReadSEQS(BinaryReader br, int chunkSize)
        {
            data.model.animationSequences = new List<AnimationSequence>();
            int nSeqs = chunkSize / 0x84;      // number of animation sequences
            for (int s = 0; s < nSeqs; s++)
            {
                AnimationSequence animationSequence = new AnimationSequence();
                animationSequence.name = ReadString(br, 80);
                animationSequence.seqIntStart = br.ReadInt32();
                animationSequence.seqIntEnd = br.ReadInt32();
                animationSequence.seqMoveSpeed = br.ReadSingle();
                animationSequence.seqNoLoop = br.ReadInt32();
                animationSequence.seqRarity = br.ReadSingle();
                animationSequence.seqLong = br.ReadInt32();
                br.BaseStream.Position += 28;                           // skip unk
                data.model.animationSequences.Add(animationSequence);
            }
        }

        // read materials
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

        // read geometry
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

        // read textures
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

        // read geometry animation
        private void ReadGEOA(BinaryReader br, int chunkSize)
        {
            long endPosition = br.BaseStream.Position + chunkSize;
            while (br.BaseStream.Position < endPosition)
            {
                int bytes = br.ReadInt32();
                int unk1 = br.ReadInt32();
                int unk2 = br.ReadInt32();
                int unk3 = br.ReadInt32();
                int unk4 = br.ReadInt32();
                int unk5 = br.ReadInt32();
                int j = br.ReadInt32();
                string KGAO = new string(br.ReadChars(4));
                if (KGAO == "KGAO")
                {
                    int num = br.ReadInt32();
                    int ltype = br.ReadInt32();
                    int unk = br.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        int frame = br.ReadInt32();
                        float state = br.ReadSingle();
                    }
                }
                else
                {
                    br.BaseStream.Position -= 4;
                }
            }
        }

        // read bone animation
        private void ReadBONE(BinaryReader br, int chunkSize)
        {
            long endPosition = br.BaseStream.Position + chunkSize;
            while (br.BaseStream.Position < endPosition)
            {
                int bytes = br.ReadInt32();

                long endPosition2 = br.BaseStream.Position + bytes - 4;

                Bone bone = new Bone();                 // create a new instance of Bone
                bone.name = ReadString(br, 80);         // bone name string
                bone.index = br.ReadInt32();            // index of the bone 0, 1, 2..
                bone.parent = br.ReadInt32();           // bone parent index
                int unk1 = br.ReadInt32();              // always 256
                while (br.BaseStream.Position < endPosition2)
                {
                    string curChunk = new string(br.ReadChars(4));
                    switch(curChunk)
                    {
                        case "KGTR":
                            bone.translation = ReadKG(br, curChunk);
                            break;
                        case "KGRT":
                            bone.rotation = ReadKG(br, curChunk);
                            break;
                        case "KGSC":
                            bone.scale = ReadKG(br, curChunk);
                            break;
                        default:
                            break;
                    }
                }
                bone.id = br.ReadInt32();                               // always -1
                bone.aid = br.ReadInt32();                              // always -1
                if (bone.id >= 0)
                {
                    data.model.submeshes[bone.id].geoBone = bone.index;
                    print(bone.name + " " + bone.id);
                }
                else
                    for (int i = 0; i < data.model.submeshes.Count; i++)
                    {
                        data.model.submeshes[i].geoBone = bone.index;
                    }

                data.model.bones.Add(bone);
            }
        }

        // read attachment points
        private void ReadATCH(BinaryReader br, int chunkSize)
        {
            data.model.attachments = new List<Attachment>();
            long endPosition = br.BaseStream.Position + chunkSize;
            while(br.BaseStream.Position < endPosition)
            {
                Attachment attachment = new Attachment();
                int bytes = br.ReadInt32();                                         // size of current attachment
                long endPosition2 = br.BaseStream.Position + bytes - 4;             // stream end position of current attachment
                int unk1 = br.ReadInt32();                                          // ??
                attachment.name = ReadString(br, 80);                               // attachment name
                attachment.index = br.ReadInt32();                                  // attachment index
                attachment.parent = br.ReadInt32();                                 // parent bone index of attachment
                int unk2 = br.ReadInt32();                                          // ??
                while (br.BaseStream.Position < endPosition2)
                {
                    string curChunk = new string(br.ReadChars(4));
                    switch (curChunk)
                    {
                        case "KGTR":
                            attachment.translation = ReadKG(br, curChunk);                           // read translation track
                            break;
                        case "KGRT":
                            attachment.rotation = ReadKG(br, curChunk);                           // read rotation track
                            break;
                        case "KGSC":
                            attachment.scale = ReadKG(br, curChunk);                           // read scale track
                            break;
                        default:
                            br.BaseStream.Position += 260;                  // empty space
                            break;
                    }
                }
                data.model.attachments.Add(attachment);
            }
        }

        // read track points
        private List<TrackPoint> ReadKG(BinaryReader br, string curChunk) // make sure you return data
        {
            float x;
            float y;
            float z;
            float w;
            object pt;
            List<TrackPoint> trackPoints = new List<TrackPoint>();
            int num = br.ReadInt32();
            int lineType = br.ReadInt32();
            int unk1 = br.ReadInt32();

            for (int j = 0; j < num; j++)
            {
                TrackPoint trackPoint = new TrackPoint();
                trackPoint.time = br.ReadInt32();

                if (curChunk == "KGRT")
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    w = br.ReadSingle();
                    pt = new Quaternion(y, z, x, w);
                }
                else
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    if (curChunk == "KGSC")
                    {
                        pt = new Vector3(y, -z, x);
                    }
                    else
                    {
                        pt = new Vector3(y, z, x);
                    }
                }
                trackPoint.point = pt;
                
                if (lineType > 1)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    if (curChunk == "KGRT")
                    {
                        w = br.ReadSingle();
                        pt = new Quaternion(y, z, x, w);
                    }
                    else
                    {
                        pt = new Vector3(y, z, x);
                    }
                    trackPoint.inTan = pt;
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    if (curChunk == "KGRT")
                    {
                        w = br.ReadSingle();
                        pt = new Quaternion(y, z, x, w);
                    }
                    else
                    {
                        pt = new Vector3(y, z, x);
                    }
                    trackPoint.outTan = pt;
                }
                
                trackPoints.Add(trackPoint);
            }
            return trackPoints;
        }

        private void ReadPIVT(BinaryReader br, int chunkSize)
        {
            long endPosition = br.BaseStream.Position + chunkSize;
            while (br.BaseStream.Position < endPosition)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                float z = br.ReadSingle();
                data.model.pivots.Add(new Vector3(y, z, x));
            }
        }

        #region Geometry Subchunks

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
            submeshBuffer.boneGroups = new List<int>();
            int numberOFMTGC = br.ReadInt32();              // coincides with the number of bones
            for (int i = 0; i < numberOFMTGC; i++)
            {
                submeshBuffer.boneGroups.Add(br.ReadInt32());
            }
        }

        // bone matrices
        private void ReadMATS(BinaryReader br)
        {
            submeshBuffer.bones = new List<List<int>>();
            submeshBuffer.groups = new List<int>();
            int numberOfMats = br.ReadInt32();              // coincides with the number of bones

            for (int i = 0; i < numberOfMats; i++)
            {
                br.ReadInt32();
            }
            // I'm not really using this  v
            /*
            int n = 0;
            int o = 0;
            for (int i = 0; i < numberOfMats; i++)
            {
                if ((n == 0) || (o == submeshBuffer.boneGroups[n]))
                {
                    n++;
                    o = 0;
                    submeshBuffer.bones[n] = new List<int>();
                }
                o++;
                submeshBuffer.bones[n][o] = br.ReadInt32();
                bool b = true;
                for (int l = 0; l < submeshBuffer.groups.Count; l++)
                {
                    if (submeshBuffer.groups[l] == submeshBuffer.bones[n][o])
                    {
                        b = false;
                        break;
                    }
                }
                if (b)
                {
                    int index = submeshBuffer.groups.Count;
                    submeshBuffer.groups[index] = submeshBuffer.bones[n][o];
                }
                submeshBuffer.groups.Sort();
            }
            */
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

        // bone weights
        private void ReadSKIN(BinaryReader br)
        {
            submeshBuffer.boneWeights = new BoneWeight[submeshBuffer.vertices.Count];
            int numberOfSkins = br.ReadInt32(); // numberOfVerts * 8
            for (int i = 0; i < numberOfSkins / 8; i++)
            {
                submeshBuffer.boneWeights[i].boneIndex0 = br.ReadByte();
                submeshBuffer.boneWeights[i].boneIndex1 = br.ReadByte();
                submeshBuffer.boneWeights[i].boneIndex2 = br.ReadByte();
                submeshBuffer.boneWeights[i].boneIndex3 = br.ReadByte();
                submeshBuffer.boneWeights[i].weight0 = br.ReadByte() / 255f;
                submeshBuffer.boneWeights[i].weight1 = br.ReadByte() / 255f;
                submeshBuffer.boneWeights[i].weight2 = br.ReadByte() / 255f;
                submeshBuffer.boneWeights[i].weight3 = br.ReadByte() / 255f;
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

        #endregion

        public void BuildModel()
        {
            Debug.Log("Building Model : " + data.model.name +
                        " | Meshes: " + data.model.submeshCount.ToString() +
                        " | Bones: " + data.model.bones.Count.ToString() +
                        " | Animations: " + data.model.animationSequences.Count.ToString());

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

            Transform[] bones = BuildBones(model);
            BuildMeshes(model, LoD0Indices, bones);
            BuildAnimations(model, bones);
        }

        private void BuildMeshes(GameObject obj, List<int> LoD0Indices, Transform[] bones)
        {
            // Group Object
            GameObject meshesGroup = new GameObject("Meshes");
            meshesGroup.transform.SetParent(obj.transform);

            // Submeshes
            for (int sm = 0; sm < LoD0Indices.Count; sm++)
            {
                int index = LoD0Indices[sm];

                // Object
                GameObject submesh = new GameObject();
                submesh.name = data.model.submeshes[index].name;
                submesh.transform.SetParent(meshesGroup.transform);

                // Mesh
                Mesh m = new Mesh();
                m.SetVertices(data.model.submeshes[index].vertices);
                m.SetTriangles(data.model.submeshes[index].triangles, 0);
                m.SetNormals(data.model.submeshes[index].normals);
                m.SetTangents(data.model.submeshes[index].tangents);
                m.SetUVs(0, data.model.submeshes[index].uvs);
                SkinnedMeshRenderer meshRenderer = submesh.AddComponent<SkinnedMeshRenderer>();
                MeshFilter meshFilter = submesh.AddComponent<MeshFilter>();
                meshFilter.mesh = m;

                // weights
                m.boneWeights = data.model.submeshes[index].boneWeights;

                // bind poses
                Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
                for (int b = 0; b < bindPoses.Length; b++)
                {
                    bindPoses[b] = bones[b].worldToLocalMatrix * submesh.transform.localToWorldMatrix;
                }
                m.bindposes = bindPoses;
                meshRenderer.bones = bones;
                meshRenderer.sharedMesh = m;

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
                    //mat.SetTexture("_MetallicGlossMap", texture);
                    mat.SetTexture("_ORM", texture);
                }
                mat.SetFloat("_Glossiness", 0.44f);
                mat.SetFloat("_GlossMapScale", 0.44f);
                meshRenderer.material = mat;
            }
        }

        private Transform[] BuildBones(GameObject obj)
        {
            // Group Object
            GameObject bonesGroup = new GameObject("Bones");
            bonesGroup.transform.SetParent(obj.transform);

            // Bones
            List<GameObject> bones = new List<GameObject>();
            Transform[] boneTransforms = new Transform[data.model.bones.Count];

            for (int a = 0; a < data.model.bones.Count; a++)
            {
                GameObject bone = new GameObject();
                bones.Add(bone);
                bones[a].transform.position = data.model.pivots[a];
                //bones[a].transform.SetParent(bonesGroup.transform);
            }

            for (int b = 0; b < data.model.bones.Count; b++)
            {
                bones[b].name = data.model.bones[b].name;
                boneTransforms[b] = bones[b].transform;
                if (data.model.bones[b].parent == -1)
                {
                    bones[b].transform.SetParent(bonesGroup.transform);
                }
                else
                {
                    bones[b].transform.SetParent(bones[data.model.bones[data.model.bones[b].index].parent].transform);
                }
            }

            // Attachments
            List<GameObject> attachments = new List<GameObject>();
            Transform[] attachmentTransforms = new Transform[data.model.attachments.Count];

            for (int c = 0; c < data.model.attachments.Count; c++)
            {
                GameObject attachment = new GameObject();
                attachments.Add(attachment);
                attachments[c].transform.position = data.model.pivots[data.model.attachments[c].index];
                //attachments[c].transform.SetParent(bonesGroup.transform);
            }

            for (int d = 0; d < data.model.attachments.Count; d++)
            {
                attachments[d].name =data.model.attachments[d].name;
                attachmentTransforms[d] = attachments[d].transform;
                if (data.model.attachments[d].parent == -1)
                {
                    attachments[d].transform.SetParent(bonesGroup.transform);
                }
                else
                {
                    attachments[d].transform.SetParent(bones[data.model.attachments[d].parent].transform);
                }
            }

            // DEBUG
            Camera.main.GetComponent<ViewSkeleton>().rootNode = bonesGroup.transform;
            Camera.main.GetComponent<ViewSkeleton>().PopulateChildren();

            return boneTransforms;
        }


        private void BuildAnimations(GameObject obj, Transform[] bones)
        {
            
            // Animations //
            Animation anim = obj.AddComponent<Animation>();
            AnimationClip clip = new AnimationClip();

            for (int p = 0; p < data.model.bones.Count; p++)
            {
                
                AnimationCurve rotation_x_curve = new AnimationCurve();
                AnimationCurve rotation_y_curve = new AnimationCurve();
                AnimationCurve rotation_z_curve = new AnimationCurve();
                AnimationCurve rotation_w_curve = new AnimationCurve();

                AnimationCurve translation_x_curve = new AnimationCurve();
                AnimationCurve translation_y_curve = new AnimationCurve();
                AnimationCurve translation_z_curve = new AnimationCurve();

                if (data.model.bones[p].rotation != null)
                {
                    Keyframe[] rotation_x_keyframes = new Keyframe[data.model.bones[p].rotation.Count];
                    Keyframe[] rotation_y_keyframes = new Keyframe[data.model.bones[p].rotation.Count];
                    Keyframe[] rotation_z_keyframes = new Keyframe[data.model.bones[p].rotation.Count];
                    Keyframe[] rotation_w_keyframes = new Keyframe[data.model.bones[p].rotation.Count];

                    for (int t = 0; t < data.model.bones[p].rotation.Count; t++)
                    {
                        if (data.model.bones[p].rotation[t].inTan != null)
                        {
                            Quaternion point = (Quaternion)data.model.bones[p].rotation[t].point;
                            Quaternion inTan = (Quaternion)data.model.bones[p].rotation[t].inTan;
                            Quaternion outTan = (Quaternion)data.model.bones[p].rotation[t].outTan;

                            rotation_x_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.x, inTan.x, outTan.x);
                            rotation_y_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.y, inTan.y, outTan.y);
                            rotation_z_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.z, inTan.z, outTan.z);
                            rotation_w_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.w, inTan.w, outTan.w);
                        }
                        else
                        {
                            Quaternion point = (Quaternion)data.model.bones[p].rotation[t].point;

                            rotation_x_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.x);
                            rotation_y_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.y);
                            rotation_z_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.z);
                            rotation_w_keyframes[t] = new Keyframe(data.model.bones[p].rotation[t].time / 1000f, point.w);
                        }
                    }

                    rotation_x_curve.keys = rotation_x_keyframes;
                    rotation_y_curve.keys = rotation_y_keyframes;
                    rotation_z_curve.keys = rotation_z_keyframes;
                    rotation_w_curve.keys = rotation_w_keyframes;

                    string bonePath = GetGameObjectPath(bones[p].gameObject);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", rotation_x_curve);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", rotation_y_curve);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", rotation_z_curve);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", rotation_w_curve);
                }
                
                if (data.model.bones[p].translation != null)
                {
                    Keyframe[] translation_x_keyframes = new Keyframe[data.model.bones[p].translation.Count];
                    Keyframe[] translation_y_keyframes = new Keyframe[data.model.bones[p].translation.Count];
                    Keyframe[] translation_z_keyframes = new Keyframe[data.model.bones[p].translation.Count];

                    for (int t = 0; t < translation_x_keyframes.Length; t++)
                    {
                        
                        if (data.model.bones[p].translation[t].inTan != null)
                        {
                            Vector3 point = (Vector3)data.model.bones[p].translation[t].point + bones[p].transform.localPosition;
                            Vector3 inTan = (Vector3)data.model.bones[p].translation[t].inTan + bones[p].transform.localPosition;
                            Vector3 outTan = (Vector3)data.model.bones[p].translation[t].outTan + bones[p].transform.localPosition;

                            translation_x_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.x, inTan.x, outTan.x);
                            translation_y_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.y, inTan.y, outTan.y);
                            translation_z_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.z, inTan.z, outTan.z);
                        }
                        
                        else
                        {
                            Vector3 point = (Vector3)data.model.bones[p].translation[t].point + bones[p].transform.localPosition;

                            translation_x_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.x);
                            translation_y_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.y);
                            translation_z_keyframes[t] = new Keyframe(data.model.bones[p].translation[t].time / 1000f, point.z);
                        }
                    }

                    translation_x_curve.keys = translation_x_keyframes;
                    translation_y_curve.keys = translation_y_keyframes;
                    translation_z_curve.keys = translation_z_keyframes;

                    string bonePath = GetGameObjectPath(bones[p].gameObject);
                    clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", translation_x_curve);
                    clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", translation_y_curve);
                    clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", translation_z_curve);
                }
                
                    
            }
            clip.legacy = true;
            clip.wrapMode = WrapMode.Loop;
            /*
            for (int c = 0; c < data.model.animationSequences.Count; c++)
            {
                anim.AddClip(clip, data.model.animationSequences[c].name, data.model.animationSequences[c].seqIntStart/60, data.model.animationSequences[c].seqIntEnd/60);
            }
            */
            anim.AddClip(clip, "clip");

            anim.Play("clip");
            
            //print(GetGameObjectPath(bones[10].gameObject));
        }

        #region Helpers

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

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        #endregion



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
