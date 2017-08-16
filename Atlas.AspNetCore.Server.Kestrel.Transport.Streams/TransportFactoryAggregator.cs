using Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public sealed class TransportFactoryAggregator<A, B> : ITransportFactory
        where A : ITransportFactory
        where B : ITransportFactory
    {
        private readonly PipeFactory _pipeFactory = new PipeFactory();

        private ITransportFactory FactoryA;
        private ITransportFactory FactoryB;

        public TransportFactoryAggregator(A factoryA, B factoryB)
        {
            this.FactoryA = factoryA;
            this.FactoryB = factoryB;
        }

        public ITransport Create(IEndPointInformation endPointInformation, IConnectionHandler handler)
        {
            return new TransportAggregator(this.FactoryA.Create(endPointInformation, handler), this.FactoryB.Create(endPointInformation, handler));
        }

        internal PipeFactory PipeFactory => _pipeFactory;
    }

}
