using System;
using System.IO;
namespace WavSplitter.Test
{
	public static class Helper
	{
		public static Stream GetAudioStream ()
		{
			return typeof (Helper).Assembly.GetManifestResourceStream ("WavSplitterTest.Resources.source.wav");
		}
	}
}
