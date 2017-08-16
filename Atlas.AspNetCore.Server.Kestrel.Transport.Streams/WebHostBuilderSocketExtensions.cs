using Atlas.AspNetCore.Server.Kestrel.Transport.Streams;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderSocketExtensions
    {
        /// <summary>
        /// Specify an additional transport to be used by Kestrel alongside the current transport.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseAdditionalTransportFactory<T>(this IWebHostBuilder hostBuilder)
            where T : ITransportFactory
        {
            return hostBuilder.ConfigureServices(services =>
            {
                Type iTransportFactoryImplementationType = null;
                foreach (var s in services)
                {
                    if (s.ServiceType == typeof(ITransportFactory))
                    {
                        iTransportFactoryImplementationType = s.ImplementationType;
                        break;
                    }
                }

                if (iTransportFactoryImplementationType != null)
                {
                    services.AddSingleton(iTransportFactoryImplementationType, iTransportFactoryImplementationType);
                    services.AddSingleton(typeof(T), typeof(T));

                    var type = typeof(TransportFactoryAggregator<,>).MakeGenericType(iTransportFactoryImplementationType, typeof(T));
                    services.AddSingleton(typeof(ITransportFactory), type);
                }
            });
        }

        /// <summary>
        /// Adds StreamTransports as an additional transport to Kestrel alongside the current transport.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseStreamTransport(this IWebHostBuilder hostBuilder)
        {
            StreamTransportFactory.PairingToken = hostBuilder.GetSetting("TOKEN");
            return hostBuilder.UseAdditionalTransportFactory<StreamTransportFactory>();
        }
    }
}