using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class TagEntity : EntityBase
    {
        private static readonly string _synonymsSeparator = ":.:";

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool HasValue { get; set; }

        /// <summary>
        /// Separator: :.:
        /// </summary>
        public string Synonyms { get; set; }

        [NotMapped]
        public IReadOnlyCollection<string> SynonymsArray
        {
            get => Synonyms.Split(new [] {_synonymsSeparator}, StringSplitOptions.RemoveEmptyEntries);
            set => Synonyms = string.Join(_synonymsSeparator, value);
        }

        public int Count { get; set; }

        public TagTypeEntity Type { get; set; }

        public IReadOnlyCollection<FileTagEntity> FileTags { get; set; }
    }
}