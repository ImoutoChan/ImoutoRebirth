using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Room.WebApi.Requests
{
    public class CollectionCreateRequest
    {
        [Required]
        public string Name { get; set; } = default!;
    }
}