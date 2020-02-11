using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class UnbindTagRequest
    {
        [Required]
        public FileTagRequest FileTag { get; set; } = default!;
    }
}