namespace ImoutoRebirth.Room.DataAccess.Models.Abstract
{
    public class ModelBase
    {
        public Guid Id { get; }

        public ModelBase(Guid id)
        {
            Id = id;
        }
    }
}