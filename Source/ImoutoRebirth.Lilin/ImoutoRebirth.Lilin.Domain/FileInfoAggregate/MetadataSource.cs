﻿namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

// todo compare with old ids
//public enum Source
//{
//    [EnumMember] Virtual = -1, // 0xFFFFFFFF
//    [EnumMember] User = 0,
//    [EnumMember] Sankaku = 1,
//    [EnumMember] Yandere = 2,
//    [EnumMember] Danbooru = 3,
//}
public enum MetadataSource
{
    Yandere = 0,
    Danbooru = 1,
    Sankaku = 2,
    Manual = 3,
    Gelbooru = 4,
    Rule34 = 5,
}
