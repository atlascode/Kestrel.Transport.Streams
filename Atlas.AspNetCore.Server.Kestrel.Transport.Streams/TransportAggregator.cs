using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System.Threading.Tasks;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public class TransportAggregator : ITransport
    {
        private ITransport TransportA;
        private ITransport TransportB;
        public TransportAggregator(ITransport A, ITransport B)
        {
            this.TransportA = A;
            this.TransportB = B;
        }

        public async Task BindAsync()
        {
            await Task.WhenAll(this.TransportA.BindAsync(), this.TransportB.BindAsync());
        }

        public async Task StopAsync()
        {
            await Task.WhenAll(this.TransportA.StopAsync(), this.TransportB.StopAsync());
        }

        public async Task UnbindAsync()
        {
            await Task.WhenAll(this.TransportA.UnbindAsync(), this.TransportB.UnbindAsync());
        }
    }
}
