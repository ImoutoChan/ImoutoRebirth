namespace ImoutoRebirth.Room.DataAccess.Exceptions
{

    public abstract class EntityNotFoundException : Exception
    {
        protected EntityNotFoundException(string message) 
            : base(message)
        {
            
        }
    }

    public class EntityNotFoundException<T> : EntityNotFoundException
    {
        public EntityNotFoundException(Guid id)
            : base($"Entity {typeof(T).Name} with id {id} was not found")
        {
        }
    }
}