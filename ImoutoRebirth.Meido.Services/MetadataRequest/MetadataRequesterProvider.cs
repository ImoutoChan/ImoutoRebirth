using System;
using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.Services.MetadataRequest
{
    public class MetadataRequesterProvider : IMetadataRequesterProvider
    {
        private readonly IEnumerable<IMetadataRequester> _requesters;

        public MetadataRequesterProvider(IEnumerable<IMetadataRequester> requesters)
        {
            _requesters = requesters;
        }

        public IMetadataRequester Get(MetadataSource source)
        {
            var requester = _requesters.FirstOrDefault(x => x.Source == source);

            if (requester == null)
                throw new NotImplementedException($"Metadata requester for source {source} was not found.");

            return requester;
        }
    }
}