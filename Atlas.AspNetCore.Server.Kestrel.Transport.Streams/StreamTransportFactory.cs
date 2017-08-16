using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public sealed class StreamTransportFactory : ITransportFactory
    {
        private readonly PipeFactory _pipeFactory = new PipeFactory();

        public static string PairingToken;

        public StreamTransportFactory()
        {
        }

        public ITransport Create(IEndPointInformation endPointInformation, IConnectionHandler handler)
        {
            if (endPointInformation == null)
            {
                throw new ArgumentNullException(nameof(endPointInformation));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return new StreamTransport(this, endPointInformation, handler);
        }

        internal PipeFactory PipeFactory => _pipeFactory;
    }
}