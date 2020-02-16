using System;

namespace ImoutoRebirth.Navigator.Services.Tags.Model
{
    internal class TagType
    {
        public TagType(Guid id, string title, int color)
        {
            Id = id;
            Title = title;
            Color = color;
        }

        public Guid Id { get; }

        public string Title { get; }

        public int Color { get; }
    }
}