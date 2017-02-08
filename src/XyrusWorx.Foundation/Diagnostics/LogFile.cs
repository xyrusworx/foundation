using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace XyrusWorx.Diagnostics
{
	[PublicAPI]
	public class LogFile : LogWriter
	{
		private readonly string mPath;
		private readonly Stream mStream;

		public LogFile([NotNull] string path, bool append = true)
		{
			if (path.NormalizeNull() == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			mPath = path;

			if (!append)
			{
				using (File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
				{
					// just to truncate
				}
			}
		}
		public LogFile([NotNull] Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanWrite)
			{
				throw new ArgumentException("The stream must be writable.", nameof(stream));
			}

			mStream = stream;
		}

		protected sealed override void DispatchOverride(LogMessage[] messages)
		{
			Stream adHocStream = null;

			try
			{
				if (mStream == null)
				{
					adHocStream = File.Open(mPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				}

				foreach (var message in messages)
				{
					if (message == null)
					{
						continue;
					}

					WriteOverride(mStream ?? adHocStream, message);
				}
			}
			finally
			{
				adHocStream?.Dispose();
			}
		}
		protected sealed override void CleanupDispatcherOverride()
		{
			mStream?.Dispose();
		}

		protected virtual void WriteOverride([NotNull] Stream stream, [NotNull] LogMessage message)
		{
			var data = $"{message.ToString(120)}{Environment.NewLine}";
			var bytes = Encoding.UTF8.GetBytes(data);

			stream.Write(bytes, 0, bytes.Length);
		}
	}
}