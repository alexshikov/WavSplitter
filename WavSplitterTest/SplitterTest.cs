using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace WavSplitter.Test
{
	[TestFixture]
	public class SplitterTest
	{
		[Test]
		public async Task Rewrite ()
		{
			var input = GetType ().Assembly.GetManifestResourceStream ("WavSplitterTest.Resources.source.wav");

			var limit = 200 * 1024;
			var splitter = new Splitter (input, limit);

			var path = Path.Combine (TestContext.CurrentContext.TestDirectory, "split.wav");
			var output = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
			var hasMore = await splitter.ReadNext (output);

			Assert.IsFalse (hasMore);
		}

		[Test]
		public async Task Copy ()
		{
			var input = GetType ().Assembly.GetManifestResourceStream ("WavSplitterTest.Resources.source.wav");

			var path = Path.Combine (TestContext.CurrentContext.TestDirectory, "copy.wav");
			var output = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);

			var reader = new WavReader (input);
			var writer = new WavWriter (output, reader.Header);

			var fllr = reader.ReadChunkHeader (skipFllr: false);
			writer.SetCurrentChunkId (fllr.ChunkId);

			var bytes = new byte[fllr.ChunkLength];
			await reader.ReadDataChunk (bytes);
			await writer.Write (bytes, 0, bytes.Length);
			writer.Flush ();

			Assert.AreEqual (input.Position, output.Position, "First chunk test");
			Assert.AreEqual (fllr.ChunkLength, writer.LastDataChunkLength);

			var data = reader.ReadChunkHeader (skipFllr: false);
			bytes = new byte[data.ChunkLength];
			await reader.ReadDataChunk (bytes);

			writer.StartNewDataChunk (data.ChunkId);
			await writer.Write (bytes, 0, bytes.Length);
			writer.Flush ();

			Assert.IsFalse (reader.HasMore);
			Assert.AreEqual (input.Position, output.Position, "Second chunk test");
			Assert.AreEqual (data.ChunkLength, writer.LastDataChunkLength);
		}

		[Test]
		public async Task Split ()
		{
			var input = GetType ().Assembly.GetManifestResourceStream ("WavSplitterTest.Resources.source.wav");

			var limit = 35 * 1024;
			var splitter = new Splitter (input, limit);

			var path = Path.Combine (TestContext.CurrentContext.TestDirectory, "part1.wav");
			var output = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
			var hasMore = await splitter.ReadNext (output);

			Assert.IsTrue (hasMore);

			path = Path.Combine (TestContext.CurrentContext.TestDirectory, "part2.wav");
			output = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
			hasMore = await splitter.ReadNext (output);

			Assert.IsFalse (hasMore);
		}
	}
}
