using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public class SourceActualizer : ISourceActualizer
    {
        private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;

        public SourceActualizer(ISourceActualizingStateRepository sourceActualizingStateRepository)
        {
            _sourceActualizingStateRepository = sourceActualizingStateRepository;
        }

        public async Task RequestActualization()
        {
            var states = await _sourceActualizingStateRepository.GetAll();

            foreach (var state in states)
            {
                state.RequestActualization();
            }
        }
    }
}