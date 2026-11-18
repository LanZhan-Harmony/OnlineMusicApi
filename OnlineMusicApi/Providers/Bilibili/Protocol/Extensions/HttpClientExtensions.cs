using System.Net.Http.Headers;
using System.Text;

namespace OnlineMusicApi.Internal.Providers.Bilibili.Protocol.Extensions;

internal static class HttpClientExtensions
{
    extension(HttpClient client)
    {
        public Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string? url,
            IEnumerable<KeyValuePair<string, string>>? queries,
            IEnumerable<KeyValuePair<string, string>>? headers,
            string? content,
            string? contentType
        ) =>
            client.SendAsync(
                method,
                url,
                queries,
                headers,
                content is null ? null : Encoding.UTF8.GetBytes(content),
                contentType
            );

        public Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string? url,
            IEnumerable<KeyValuePair<string, string>>? queries,
            IEnumerable<KeyValuePair<string, string>>? headers,
            byte[]? content,
            string? contentType
        )
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(method);

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            UriBuilder uriBuilder = new(url);
            if (queries is not null)
            {
                var query = queries.ToQueryString();
                if (!string.IsNullOrEmpty(query))
                {
                    if (string.IsNullOrEmpty(uriBuilder.Query))
                    {
                        uriBuilder.Query = query;
                    }
                    else
                    {
                        uriBuilder.Query += "&" + query;
                    }
                }
            }

            HttpRequestMessage request = new(method, uriBuilder.Uri);
            if (content is not null)
            {
                request.Content = new ByteArrayContent(content);
                if (!string.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                }
            }

            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return client.SendAsync(request);
        }
    }
}
