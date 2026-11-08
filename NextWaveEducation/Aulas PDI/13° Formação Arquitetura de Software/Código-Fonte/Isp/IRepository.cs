namespace SolidPrinciples.Isp
{
    public interface IRepository<T> 
    {
        void Add(T data);
        void Update(T data);
        void Delete(Guid id);
        IEnumerable<T> GetAll(Guid id);
        T GetById(Guid id);
    }
}