using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Buffers;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public class RequestStream : Stream
    {
        private readonly IPipeWriter _writer;
        public RequestStream(IPipeWriter writer)
        {
            _writer = writer;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("Please use WriteAsync instead");
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var newSeg = new ArraySegment<byte>(buffer, offset, count);
            return _writer.WriteAsync(newSeg);
        }

        public void Complete()
        {
            _writer.Complete();
        }
    }

    public sealed class StreamConnection : IConnectionInformation
    {
        public readonly RequestStream RequestStream;
        public readonly MemoryStream ResponseStream;
        private readonly StreamTransport _transport;
        private readonly IConnectionHandler _connectionHandler;

        private IConnectionContext _connectionContext;
        private IPipeWriter _input;
        private IPipeReader _output;
        private IList<ArraySegment<byte>> _sendBufferList;
        private const int MinAllocBufferSize = 2048;

        internal StreamConnection(StreamTransport transport, IConnectionHandler connectionHandler)
        {
            Debug.Assert(transport != null);

            _transport = transport;
            _connectionHandler = connectionHandler;
            _connectionContext = _connectionHandler.OnConnection(this);

            RequestStream = new RequestStream(_connectionContext.Input);
            ResponseStream = new MemoryStream();
        }

        public async Task StartAsync()
        {
            try
            {
                _input = _connectionContext.Input;
                _output = _connectionContext.Output;

                await DoSend();
            }
            catch (Exception)
            {
                // TODO: Log
            }
        }

        private void SetupSendBuffers(ReadableBuffer buffer)
        {
            Debug.Assert(!buffer.IsEmpty);
            Debug.Assert(!buffer.IsSingleSpan);

            if (_sendBufferList == null)
            {
                _sendBufferList = new List<ArraySegment<byte>>();
            }

            // We should always clear the list after the send
            Debug.Assert(_sendBufferList.Count == 0);

            foreach (var b in buffer)
            {
                _sendBufferList.Add(GetArraySegment(b));
            }
        }

        private async Task DoSend()
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    // Wait for data to write from the pipe producer
                    var result = await _output.ReadAsync();
                    var buffer = result.Buffer;

                    if (result.IsCancelled)
                    {
                        break;
                    }

                    try
                    {
                        if (!buffer.IsEmpty)
                        {
                            if (buffer.IsSingleSpan)
                            {
                                var segment = GetArraySegment(buffer.First);
                                await ResponseStream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                            }
                            else
                            {
                                SetupSendBuffers(buffer);

                                try
                                {
                                    foreach (var segment in _sendBufferList)
                                    {
                                        await ResponseStream.WriteAsync(segment.Array, segment.Offset, segment.Count);
                                    }
                                }
                                finally
                                {
                                    _sendBufferList.Clear();
                                }
                            }
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        _output.Advance(buffer.End);
                    }
                }

                //_socket.Shutdown(SocketShutdown.Send);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                error = null;
            }
            catch (ObjectDisposedException)
            {
                error = null;
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                //Application.OnConnectionClosed(error);
                _output.Complete(error);
            }
        }

        private static ArraySegment<byte> GetArraySegment(Buffer<byte> buffer)
        {
            if (!buffer.TryGetArray(out var segment))
            {
                throw new InvalidOperationException();
            }

            return segment;
        }

        public IPEndPoint RemoteEndPoint => null;

        public IPEndPoint LocalEndPoint => null;

        public PipeFactory PipeFactory => _transport.TransportFactory.PipeFactory;

        public IScheduler InputWriterScheduler => InlineScheduler.Default;

        public IScheduler OutputReaderScheduler => TaskRunScheduler.Default;
    }   
}