using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Services.MetadataRequest;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands
{
    internal class AddNewFileCommandHandler : ICommandHandler<AddNewFileCommand>
    {
        private readonly IMetadataRequesterProvider _metadataRequesterProvider;
        private readonly MeidoDbContext _context;

        public AddNewFileCommandHandler(IMetadataRequesterProvider metadataRequesterProvider, MeidoDbContext context)
        {
            _metadataRequesterProvider = metadataRequesterProvider;
            _context = context;
        }

        public async Task<Unit> Handle(AddNewFileCommand request, CancellationToken cancellationToken)
        {
            var allMetadataSources = typeof(MetadataSource).GetEnumValues().Cast<MetadataSource>();

            foreach (var metadataSource in allMetadataSources)
            {
                var parsingStatus = ParsingStatus.Create(request.FileId, request.Md5, metadataSource);

                await _context.AddAsync(parsingStatus);

                await _metadataRequesterProvider
                     .Get(metadataSource)
                     .SendRequestCommand(request.FileId, request.Md5);
            }


            return Unit.Value;
        }
    }
}