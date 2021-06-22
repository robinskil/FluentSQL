using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentSQL
{
    public interface IFluentQueryable<T>
    {
        T First();
        List<T> ToList();
        IFluentQueryable<TOut> Select<TOut>(Expression<Func<T,TOut>> selectExpression) where TOut : class;
        IFluentQueryable<T> Select(Expression<Func<T, T>> selectExpression);
    }
}