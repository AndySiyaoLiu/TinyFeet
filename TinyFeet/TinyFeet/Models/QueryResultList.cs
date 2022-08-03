using TinyFeet.Interfaces.Query;

namespace TinyFeet.Models;

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
