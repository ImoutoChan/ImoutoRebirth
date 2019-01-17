using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Arachne.Service.Commands
{
    public class FileTag : IFileTag
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string[] Synonyms { get; set; }
    }
}