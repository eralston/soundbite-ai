using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Masticore.Storage
{
    /// <summary>
    /// Extensions for handling streams
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts the given string to a stream
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Stream ToStream(this string val)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(val);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }

        /// <summary>
        /// Converts the given stream to a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ToText(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }

        /// <summary>
        /// Converts the given stream to a Base64 encoded string
        /// TODO: Figure out how to do this safelty for arbitraty streams and not just MemoryStreams
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ToBase64String(this MemoryStream stream)
        {
            byte[] bytes = stream.ToArray();
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts the given string to a Base64 encoded string, ready for an img tag
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ToJpgString(this MemoryStream stream)
        {
            return $"data:image/jpeg;base64, {stream.ToBase64String()}";
        }

        /// <summary>
        /// Runs an action against a stream and afterwards returns the stream to its original position.
        /// </summary>
        /// <param name="stream">Stream to return to original position after processing.</param>
        /// <param name="seekPos">Seek position to which the stream is set before running the action.  Use <c>null</c> to use current position.</param>
        /// <param name="asyncAction">Action to perform (presumably on the stream)</param>
        /// <param name="throwIfCannotSeek">Flag indicating whether an exception should be thrown in the stream does not support seeking.</param>
        /// <exception cref="Exception">Thrown if the <paramref name="throwIfCannotSeek"/> flag is true and the stream does not support seeking.</exception>
        public static Task SeekAndReturn(this Stream stream, long? seekPos, Func<Task> asyncAction, bool throwIfCannotSeek = true)
        {
            Task result;
            if (stream.CanSeek)
            {
                // Store off the original position
                long originalPos = stream.Position;

                // Update the current position
                if (seekPos != null)
                {
                    stream.Position = seekPos.Value;
                }

                // Make sure to reset the stream position even if there is an error
                try
                {
                    result = asyncAction?.Invoke();
                }
                finally
                {
                    stream.Position = originalPos;
                }
            }
            else
            {
                if (throwIfCannotSeek)
                {
                    throw new Exception("Cannot return stream to original seek position.");
                }

                // Do not allow the process to continue if the requested seek position does not match
                if (seekPos != null && seekPos.Value != stream.Position)
                {
                    throw new Exception("Stream does not support seeking and the current position does not match the requested position.");
                }

                result = asyncAction();
            }

            return result;
        }

        /// <summary>
        /// Runs an action against a stream and afterwards returns the stream to its original position.
        /// </summary>
        /// <param name="stream">Stream to return to original position after processing.</param>
        /// <param name="seekPos">Seek position to which the stream is set before running the action.  Use <c>null</c> to use current position.</param>
        /// <param name="action">Action to perform (presumably on the stream)</param>
        /// <param name="throwIfCannotSeek">Flag indicating whether an exception should be thrown in the stream does not support seeking.</param>
        /// <exception cref="Exception">Thrown if the <paramref name="throwIfCannotSeek"/> flag is true and the stream does not support seeking.</exception>
        public static void SeekAndReturn(this Stream stream, long? seekPos, Action action, bool throwIfCannotSeek = true)
        {
            if (stream.CanSeek)
            {
                // Store off the original position
                long originalPos = stream.Position;

                // Update the current position
                if (seekPos != null)
                {
                    stream.Position = seekPos.Value;
                }

                // Make sure to reset the stream position even if there is an error
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    stream.Position = originalPos;
                }
            }
            else
            {
                if (throwIfCannotSeek)
                {
                    throw new Exception("Cannot return stream to original seek position.");
                }

                // Do not allow the process to continue if the requested seek position does not match
                if (seekPos != null && seekPos.Value != stream.Position)
                {
                    throw new Exception("Stream does not support seeking and the current position does not match the requested position.");
                }

                action?.Invoke();
            }
        }

        /// <summary>
        /// Copy between streams in 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="chunkSize"></param>
        public static void CopyToInChunks(this Stream source, Stream destination, int chunkSize = 4096)
        {
            byte[] readBuffer = new byte[chunkSize];
            int bytesRead;

            // Iteratively read the stream and write to the ffprobe input
            while ((bytesRead = source.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                destination.Write(readBuffer, 0, chunkSize);
            }
        }
    }
}
