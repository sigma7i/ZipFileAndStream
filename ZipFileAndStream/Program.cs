using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace ZipFile
{
	class Program
	{
		static void Main(string[] args)
		{
			var files = Directory.GetFiles("TestData", "*.*", SearchOption.TopDirectoryOnly);

			foreach (var file in files)
			{
				var data = File.ReadAllBytes(file);

				var watchDeflat = Stopwatch.StartNew();
				var compressedDeflat = CompressDeflat(data);
				watchDeflat.Stop();

				var watchGZip = Stopwatch.StartNew();
				var compressGZip = CompressGZip(data);
				watchGZip.Stop();

				var watchLZMA = Stopwatch.StartNew();
				var compressLZMA = CompressFileLZMA(data);
				watchLZMA.Stop();

				//Console.WriteLine(file.Remove(0, 9));
				Console.WriteLine("OriginalSize {0}", data.Length / 1024);
				Console.WriteLine("CompressDeflat {0} {1}", compressedDeflat.Length / 1024, watchDeflat.ElapsedMilliseconds);
				Console.WriteLine("compressGZip {0} {1}", compressGZip.Length / 1024, watchGZip.ElapsedMilliseconds);
				Console.WriteLine("compressLZMA {0} {1}", compressLZMA.Length / 1024, watchLZMA.ElapsedMilliseconds);
				Console.WriteLine();
			}


			//var decompressLZMA = DecompressFileLZMA(compressLZMA);
			//File.WriteAllBytes("LZMADecompress.jpg", decompressLZMA);
			Console.ReadKey();
		}

		public static byte[] CompressDeflat(byte[] data)
		{
			using (MemoryStream output = new MemoryStream())
			{
				using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
				{
					dstream.Write(data, 0, data.Length);
				}
				output.Flush();
				return output.ToArray();
			}
		}

		public static byte[] DecompressDeflat(byte[] data)
		{
			using (MemoryStream input = new MemoryStream(data))
			{
				using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
				{
					using (MemoryStream output = new MemoryStream())
					{
						dstream.CopyTo(output);
						return output.ToArray();
					}
				}
			}
		}

		public static byte[] CompressGZip(byte[] data)
		{
			MemoryStream output = new MemoryStream();
			using (GZipStream dstream = new GZipStream(output, CompressionLevel.Optimal))
			{
				dstream.Write(data, 0, data.Length);
			}
			return output.ToArray();
		}

		public static byte[] DecompressGZip(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			using (GZipStream dstream = new GZipStream(input, CompressionMode.Decompress))
			{
				dstream.CopyTo(output);
			}
			return output.ToArray();
		}

		private static byte[] CompressFileLZMA(byte[] data)
		{
			// https://stackoverflow.com/questions/7646328/how-to-use-the-7z-sdk-to-compress-and-decompress-a-file
			SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();

			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			//FileStream output = new FileStream(outFile, FileMode.Create);

			// Write the encoder properties
			coder.WriteCoderProperties(output);

			// Write the decompressed file size.
			output.Write(BitConverter.GetBytes(input.Length), 0, 8);

			// Encode the file.
			coder.Code(input, output, input.Length, -1, null);


			//	output.Flush();
			//output.Close();

			return output.ToArray();
		}

		private static byte[] DecompressFileLZMA(byte[] data)
		{
			SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();

			// Read the decoder properties
			byte[] properties = new byte[5];
			input.Read(properties, 0, 5);

			// Read in the decompress file size.
			byte[] fileLengthBytes = new byte[8];
			input.Read(fileLengthBytes, 0, 8);
			long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

			coder.SetDecoderProperties(properties);
			coder.Code(input, output, input.Length, fileLength, null);
			//output.Flush();
			//output.Close();

			return output.ToArray();
		}
	}
}
