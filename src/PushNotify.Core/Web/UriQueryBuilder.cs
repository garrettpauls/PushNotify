using System;
using System.Text;

namespace PushNotify.Core.Web
{
    public sealed class UriQueryBuilder
    {
        private readonly StringBuilder mUri;
        private bool mHasParameters;

        public UriQueryBuilder(string baseUri)
        {
            // for now we assume the base uri doesn't have any parameters,
            // TODO: should check and handle existing params properly.
            mUri = new StringBuilder(baseUri);
            mHasParameters = false;
        }

        public UriQueryBuilder AddParameter(string key, string value)
        {
            if(mHasParameters)
            {
                mUri.Append("&");
            }
            else
            {
                mUri.Append("?");
            }

            mUri.Append(Uri.EscapeUriString(key));
            mUri.Append("=");
            mUri.Append(Uri.EscapeUriString(value));

            mHasParameters = true;
            return this;
        }

        public Uri ToUri()
        {
            return new Uri(mUri.ToString());
        }
    }
}
