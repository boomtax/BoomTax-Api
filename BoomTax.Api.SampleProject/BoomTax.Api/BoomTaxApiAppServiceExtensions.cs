using System;
using System.Net.Http;
using Microsoft.Azure.AppService;

namespace BoomTax.Api.SampleProject
{
    public static class BoomTaxApiAppServiceExtensions
    {
        public static BoomTaxApi CreateBoomTaxApi(this IAppServiceClient client)
        {
            return new BoomTaxApi(client.CreateHandler());
        }

        public static BoomTaxApi CreateBoomTaxApi(this IAppServiceClient client, params DelegatingHandler[] handlers)
        {
            return new BoomTaxApi(client.CreateHandler(handlers));
        }

        public static BoomTaxApi CreateBoomTaxApi(this IAppServiceClient client, Uri uri, params DelegatingHandler[] handlers)
        {
            return new BoomTaxApi(uri, client.CreateHandler(handlers));
        }

        public static BoomTaxApi CreateBoomTaxApi(this IAppServiceClient client, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
        {
            return new BoomTaxApi(rootHandler, client.CreateHandler(handlers));
        }
    }
}
