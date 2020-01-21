namespace TinyFeet.Models
{
    using TinyFeet.Interfaces.Query;

    public class QueryResultList<T> : IQueryResult
    {
        #region Constructor

        public QueryResultList(T[] items)
        {
            this.Items = items;
        }

        #endregion Constructor

        #region Properties

        public T[] Items { get; }

        #endregion Properties
    }
}
