using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WavSplitter
{
	public class WavChunkHeader
	{
		public const int Size = 8;

		public string ChunkId { get; private set; }
		public int ChunkLength { get; private set; }

		readonly static string[] supportedChunks =
		{
			WavConst.Data,
			WavConst.Fllr,
			WavConst.List
		};

		public bool ShouldSkip
		{
			get { return ChunkId != WavConst.Data; }
		}

		public void Read (BinaryReader reader)
		{
			ChunkId = Encoding.UTF8.GetString (reader.ReadBytes (4), 0, 4);
			ValidateDataChunkId (ChunkId);

			ChunkLength = reader.ReadInt32 ();
		}

		private static void ValidateDataChunkId (string chunkId)
		{
			if (!supportedChunks.Contains (chunkId))
			{
				throw new NotSupportedException ("Invalid data chunk id: " + chunkId);
			}
		}
	}
}
