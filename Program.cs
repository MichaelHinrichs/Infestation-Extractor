//Written for the Infestation series.
//Infestation: Battle Royale https://store.steampowered.com/app/1240290
//Infestation: Survivor Stories https://store.steampowered.com/app/226700
//Infestation: The New Z https://store.steampowered.com/app/555570
using System.IO;
using System.IO.Compression;

namespace Infestation_Extractor
{
    class Program
    {
        public static BinaryReader br;
        static void Main(string[] args)
        {
            br = new(File.OpenRead(args[0] + "\\WZ_00.bin"));
            br.BaseStream.Position = 26;

            Stream fileTable = new MemoryStream();
            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)(br.BaseStream.Length - 26))), CompressionMode.Decompress))
                ds.CopyTo(fileTable);

            br = new(fileTable);
            br.BaseStream.Position = 0;
            System.Collections.Generic.List<Subfile> subfiles = new();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                subfiles.Add(new());
                br.ReadBytes(14);
            }

            foreach (Subfile file in subfiles)
            {
                if (file.container < 9)
                    br = new(File.OpenRead(args[0] + "\\WZ_0" + (file.container + 1) + ".bin"));
                else
                    br = new(File.OpenRead(args[0] + "\\WZ_" + (file.container + 1) + ".bin"));

                br.BaseStream.Position = file.start;
                Directory.CreateDirectory(args[0] + "\\" + Path.GetDirectoryName(file.name));

                if (file.isCompressed == 2)
                {
                    using FileStream fs = File.Create(args[0] + "\\" + file.name);
                    br.ReadInt16();
                    using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(file.sizeCompressed)), CompressionMode.Decompress))
                        ds.CopyTo(fs);
                    fs.Close();
                }
                else
                {
                    BinaryWriter bw = new(File.Create(args[0] + "\\" + file.name));
                    bw.Write(br.ReadBytes(file.sizeUncompressed));
                    bw.Close();
                }
            }
        }

        class Subfile
        {
            public string name = new string(br.ReadChars(260)).TrimEnd('\0');
            public byte isCompressed = br.ReadByte();
            public byte container = br.ReadByte();
            public int start = br.ReadInt32() + 4;
            public int sizeUncompressed = br.ReadInt32();
            public int sizeCompressed = br.ReadInt32();
            float checksum = br.ReadSingle();
            public int unknown = br.ReadInt32();
        }
    }
}
