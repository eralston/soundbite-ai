using Masticore.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Masticore.Ad
{
    /// <summary>
    /// Builds up and executes a request to MS Graph
    /// </summary>
    public class GraphRequest
    {
        protected IGraphClient Client { get; }

        public string BaseUrl { get; }
        public string Select { get; set; }
        public string Filter { get; set; }
        public int? Top { get; set; }
        public int? Skip { get; set; }
        public string SkipToken { get; set; }

        protected bool HasQueryString { get; set; } = false;

        public GraphRequest(IGraphClient client, string baseUrl)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        protected void Append(StringBuilder builder, string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            Append(builder, name, value.Value.ToString());
        }

        protected void Append(StringBuilder builder, string name, string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            AppendConjunction(builder);
            builder.Append(name);
            builder.Append("=");
            builder.Append(value);
        }

        protected void AppendConjunction(StringBuilder builder)
        {
            if (HasQueryString)
            {
                builder.Append("&");
            }
            else
            {
                builder.Append("?");
                HasQueryString = true;
            }
        }

        /// <summary>
        /// AND the given filter onto the request <see cref="Filter"/> field, but only if it's non-empty
        /// </summary>
        /// <param name="newFilter"></param>
        public void AndFilter(string newFilter)
        {
            newFilter = newFilter?.Trim();
            if (string.IsNullOrEmpty(newFilter))
            {
                return;
            }

            if (string.IsNullOrEmpty(Filter))
            {
                Filter = newFilter;
            }
            else
            {
                Filter = $"{Filter} and {newFilter}";
            }
        }

        public string Url()
        {
            HasQueryString = false;
            StringBuilder builder = new StringBuilder(BaseUrl);
            Append(builder, "$select", Select);
            Append(builder, "$filter", Filter);
            Append(builder, "$top", Top);
            Append(builder, "$skip", Skip);
            Append(builder, "$skipToken", SkipToken);
            return builder.ToString();
        }

        public async Task<TResponse> Get<TResponse>(IndexPageRequest page = null)
        {
            if (page != null)
            {
                Top = page.Take;
                Skip = page.Skip;
                Filter = page.Filter;
            }

            string url = Url();
            TResponse data = await Client.Get<TResponse>(url);
            return data;
        }

        public async Task<TResponse> Get<TResponse>(TokenPageRequest page)
        {
            if (page != null)
            {
                Top = page.Take;
                SkipToken = page.SkipToken;
                if (Filter == null)
                {
                    Filter = page.Filter;
                }
            }

            string url = Url();
            TResponse data = await Client.Get<TResponse>(url);
            return data;
        }
    }
}
