using System.IO;
using System.IO.Compression;
using zlib;

namespace TmxCSharp.Loader
{
    internal static class Decompression
    {
        private static void CopyStream(Stream input, Stream output)
        {
            const int bufferSize = 4096;

            input.CopyTo(output, bufferSize);

            output.Flush();
        }

        public static byte[] Decompress(string compression, byte[] compressedData)
        {
            byte[] decompressedData;

            switch (compression)
            {
                case "zlib":
                    DecompressZLibData(compressedData, out decompressedData);
                    break;
                case "gzip":
                    DecompressGZipData(compressedData, out decompressedData);
                    break;
                default:
                    if (string.IsNullOrEmpty(compression))
                    {
                        decompressedData = compressedData;
                    }
                    else
                    {
                        throw new InvalidDataException("Unsupported compression (expected zlib or gzip)");
                    }
                    break;
            }

            return decompressedData;
        }

        private static void DecompressZLibData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
                {
                    using (Stream inMemoryStream = new MemoryStream(inData))
                    {
                        CopyStream(inMemoryStream, outZStream);
                        outZStream.finish();
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }

        private static void DecompressGZipData(byte[] inData, out byte[] outData)
        {
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                using (GZipStream outGStream = new GZipStream(inMemoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outMemoryStream = new MemoryStream())
                    {
                        CopyStream(outGStream, outMemoryStream);
                        outData = outMemoryStream.ToArray();
                    }
                }
            }
        }
    }
}
