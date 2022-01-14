namespace ImoutoRebirth.Lilin.WebApi.Responses
{
    public class TagTypeResponse
    {
        public Guid Id { get; }

        public string Name { get; }

        public int Color { get; }

        public TagTypeResponse(Guid id, string name, int color)
        {

            Id = id;
            Name = name;
            Color = color;
        }
    }
}