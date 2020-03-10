using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LZOWrapper;

namespace FableAnniversaryHelperUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            uint HPNT = (5 * 4);
            uint HDMY = (14 * 4);


            using (BinaryReader buffer = new BinaryReader(File.Open(args[0], FileMode.Open)))
            {
                LZO lzo = new LZO();

                //Grab string header
                var stringbytes = new List<char>();
                while (buffer.PeekChar() != 0x00)
                {
                    stringbytes.Add(buffer.ReadChar());
                }
                buffer.ReadChar(); //null string termination not handled by ReadString()

                //Skeleton and Origin
                Byte[] header = buffer.ReadBytes(41);
                UInt16 NumberHPNTs = buffer.ReadUInt16();
                UInt16 NumberHDMYs = buffer.ReadUInt16();
                UInt32 HLPR_Size = buffer.ReadUInt32();
                Byte[] pad1 = buffer.ReadBytes(2);

                Byte[] helper_points = new Byte[HPNT * NumberHPNTs];
                Byte[] helper_dummies = new Byte[HDMY * NumberHDMYs];
                Byte[] helpers = new Byte[HLPR_Size];

                UInt16 compressed_point_size = buffer.ReadUInt16();
                if (compressed_point_size > 0)
                {
                    Byte[] compressed = buffer.ReadBytes(compressed_point_size);
                    Byte[] runoff = buffer.ReadBytes(3);
                    helper_points = lzo.Decompress(compressed, compressed_point_size, helper_points, (int)(HPNT * NumberHPNTs));
                    System.Buffer.BlockCopy(runoff, 0, helper_points, helper_points.Length - 3, 3);
                }
                else { helper_points = buffer.ReadBytes((int)(HPNT * NumberHPNTs)); }

                UInt16 compressed_dummy_size = buffer.ReadUInt16();
                if (compressed_dummy_size > 0)
                {
                    Byte[] compressed = buffer.ReadBytes(compressed_dummy_size);
                    Byte[] runoff = buffer.ReadBytes(3);
                    helper_dummies = lzo.Decompress(compressed, compressed_dummy_size, helper_dummies, (int)(HDMY * NumberHDMYs));
                    System.Buffer.BlockCopy(runoff, 0, helper_dummies, helper_dummies.Length - 3, 3);
                }
                else { helper_dummies = buffer.ReadBytes((int)(HDMY * NumberHDMYs)); }

                UInt16 compressed_helper_size = buffer.ReadUInt16();
                if (compressed_helper_size > 0)
                {
                    Byte[] compressed = buffer.ReadBytes(compressed_helper_size);
                    Byte[] runoff = buffer.ReadBytes(3);
                    helpers = lzo.Decompress(compressed, compressed_helper_size, new byte[HLPR_Size], (int)HLPR_Size);
                    System.Buffer.BlockCopy(runoff, 0, helpers, helpers.Length - 3, 3);
                }
                else { helpers = buffer.ReadBytes((int)HLPR_Size); }


                UInt32 NumberMaterials = buffer.ReadUInt32();
                UInt32 NumberSubMeshes = buffer.ReadUInt32();
                UInt32 NumberBones = buffer.ReadUInt32();
                UInt32 SizeOfBoneIndex = buffer.ReadUInt32();
                byte[] pad2 = buffer.ReadBytes(1);
                UInt16 unk1 = buffer.ReadUInt16();
                UInt16 unk2 = buffer.ReadUInt16();
                Byte[] identitymatrix = buffer.ReadBytes(12 * 4); //matrix, 12 floats

                for (var i = 1; i < NumberBones; i++) { }
                for (var i = 1; i < NumberMaterials; i++) { }
                for (var i = 1; i < NumberSubMeshes; i++) { }


                string newfile = Path.ChangeExtension(args[0], null) + ".unpacked.BIN";
                (new FileInfo(newfile)).Directory.Create();
                using (BinaryWriter b = new BinaryWriter(File.Open(newfile, FileMode.Create)))
                {
                    //Leave it uncompressed
                    compressed_point_size = 0;
                    compressed_dummy_size = 0;
                    compressed_helper_size = 0;
                    //None of this is needed
                    NumberMaterials = 0;
                    NumberSubMeshes = 0;
                    NumberBones = 0;
                    unk1 = 0;
                    unk2 = 0;

                    //Write a new file...
                    b.Write(stringbytes.ToArray());
                    b.Write(new Byte[] { 0x00 });
                    b.Write(header);
                    b.Write(NumberHPNTs);
                    b.Write(NumberHDMYs);
                    b.Write(HLPR_Size);
                    b.Write(pad1);
                    b.Write(compressed_point_size);
                    b.Write(helper_points);
                    b.Write(compressed_dummy_size);
                    b.Write(helper_dummies);
                    b.Write(compressed_helper_size);
                    b.Write(helpers);
                    b.Write(NumberMaterials);
                    b.Write(NumberSubMeshes);
                    b.Write(NumberBones);
                    b.Write(SizeOfBoneIndex);
                    b.Write(pad2);
                    b.Write(unk1);
                    b.Write(unk2);
                    b.Write(identitymatrix);
                    //Nothing after this is needed or read by the game...
                }
            }
        }
    }

    class Material
    {
        uint ID { get; set; }
        string Name { get; set; }
        object data { get; set; }
    }
}
