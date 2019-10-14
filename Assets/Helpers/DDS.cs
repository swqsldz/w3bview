using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DDS
{

    private const uint DDSD_MIPMAPCOUNT_BIT = 0x00020000;
    private const uint DDPF_ALPHAPIXELS = 0x00000001;
    private const uint DDPF_ALPHA = 0x00000002;
    private const uint DDPF_FOURCC = 0x00000004;
    private const uint DDPF_RGB = 0x00000040;
    private const uint DDPF_YUV = 0x00000200;
    private const uint DDPF_LUMINANCE = 0x00020000;
    private const uint DDPF_NORMAL = 0x80000000;

    public static Texture2D ImportToTexture2D(string path)
    {
        using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
        {

            // Header
            byte[] dwMagic = reader.ReadBytes(4);                   // "DDS "
            int dwSize = (int)reader.ReadUInt32();                  // Size of structure. This member must be set to 124.
            int dwFlags = (int)reader.ReadUInt32();                 // Flags to indicate valid fields. Always include DDSD_CAPS, DDSD_PIXELFORMAT, DDSD_WIDTH, DDSD_HEIGHT.
            int dwHeight = (int)reader.ReadUInt32();                // Height of the main image in pixels
            int dwWidth = (int)reader.ReadUInt32();                 // Width of the main image in pixels
            int dwPitchOrLinearSize = (int)reader.ReadUInt32();     // For uncompressed formats, this is the number of bytes per scan line (DWORD> aligned)
                                                                    // for the main image. dwFlags should include DDSD_PITCH in this case. For compressed formats,
                                                                    // this is the total number of bytes for the main image. dwFlags should be include DDSD_LINEARSIZE in this case.
            int dwDepth = (int)reader.ReadUInt32();                 // For volume textures, this is the depth of the volume. dwFlags should include DDSD_DEPTH in this case.
            int dwMipMapCount = (int)reader.ReadUInt32();           // For items with mipmap levels, this is the total number of levels in the mipmap chain of the main image.
                                                                    // dwFlags should include DDSD_MIPMAPCOUNT in this case.
            if ((dwFlags & DDSD_MIPMAPCOUNT_BIT) == 0)
            {
                dwMipMapCount = 1;
            }

            for (int i = 0; i < 11; i++)
            {
                reader.ReadUInt32();                                // reserved bytes 11*4
            }

            // 32-byte value that specifies the pixel format structure.
            uint dds_pxlf_dwSize = reader.ReadUInt32();
            uint dds_pxlf_dwFlags = reader.ReadUInt32();
            byte[] dds_pxlf_dwFourCC = reader.ReadBytes(4);
            string fourCC = Encoding.ASCII.GetString(dds_pxlf_dwFourCC);
            uint dds_pxlf_dwRGBBitCount = reader.ReadUInt32();
            uint pixelSize = dds_pxlf_dwRGBBitCount / 8;
            uint dds_pxlf_dwRBitMask = reader.ReadUInt32();
            uint dds_pxlf_dwGBitMask = reader.ReadUInt32();
            uint dds_pxlf_dwBBitMask = reader.ReadUInt32();
            uint dds_pxlf_dwABitMask = reader.ReadUInt32();

            // 16-byte value that specifies the capabilities structure.
            int dwCaps = (int)reader.ReadUInt32();
            int dwCaps2 = (int)reader.ReadUInt32();
            int dwCaps3 = (int)reader.ReadUInt32();
            int dwCaps4 = (int)reader.ReadUInt32();

            int dwReserved2 = (int)reader.ReadUInt32();             // reserved bytes

            TextureFormat textureFormat = TextureFormat.DXT1;

            bool isNormalMap = (dds_pxlf_dwFlags & DDPF_NORMAL) != 0;
            bool fourcc = (dds_pxlf_dwFlags & DDPF_FOURCC) != 0;

            if (fourcc)
            {
                if (fourCCEquals(dds_pxlf_dwFourCC, "DXT1"))
                {
                    textureFormat = TextureFormat.DXT1;
                }
                else if (fourCCEquals(dds_pxlf_dwFourCC, "DXT5"))
                {
                    textureFormat = TextureFormat.DXT5;
                }
                else
                {
                    textureFormat = TextureFormat.BC5;
                }
            }
            else
            {
                Debug.Log("Problem");
            }

            long dataBias = 128;

            long dxtBytesLength = reader.BaseStream.Length - dataBias;
            reader.BaseStream.Seek(dataBias, SeekOrigin.Begin);
            byte[] dxtBytes = reader.ReadBytes((int)dxtBytesLength);

            //Debug.Log(path + " " + textureFormat.ToString() + " " + dwWidth + " " + dwHeight + " " + dwMipMapCount + " " + dxtBytes.Length);

            Texture2D texture = new Texture2D(dwWidth, dwHeight, textureFormat, dwMipMapCount > 1);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply(false, false);
            return texture;
        }
    }

    private static bool fourCCEquals(IList<byte> bytes, string s)
    {
        return bytes[0] == s[0] && bytes[1] == s[1] && bytes[2] == s[2] && bytes[3] == s[3];
    }

}
