﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities;

public class CollectionFileEntity : EntityBase
{
    [ForeignKey(nameof(Collection))]
    public Guid CollectionId { get; set; }

    [Required]
    public required string Path { get; set; }

    [Required]
    [StringLength(32)]
    public required string Md5 { get; set; }

    public long Size { get; set; }

    public string? OriginalPath { get; set; }

    public bool IsRemoved { get; set; }

    public CollectionEntity? Collection { get; set; }
}
