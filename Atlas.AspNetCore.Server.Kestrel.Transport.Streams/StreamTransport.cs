using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public sealed class StreamTransport : ITransport
    {
        private readonly StreamTransportFactory _transportFactory;
        private readonly IEndPointInformation _endPointInformation;
        private readonly IConnectionHandler _handler;

        private static StreamTransport CurrentStreamTransport;

        internal StreamTransport(StreamTransportFactory transportFactory, IEndPointInformation endPointInformation, IConnectionHandler handler)
        {
            Debug.Assert(transportFactory != null);
            Debug.Assert(endPointInformation != null);
            Debug.Assert(endPointInformation.Type == ListenType.IPEndPoint);
            Debug.Assert(handler != null);

            _transportFactory = transportFactory;
            _endPointInformation = endPointInformation;
            _handler = handler;

            CurrentStreamTransport = this;
        }

        public Task BindAsync()
        {
            return Task.CompletedTask;
        }

        public Task UnbindAsync()
        {
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        public static StreamConnection CreateConnection()
        {
            var connection = new StreamConnection(CurrentStreamTransport, CurrentStreamTransport._handler);
            return connection;
        }

        internal StreamTransportFactory TransportFactory => _transportFactory;
    }
}
