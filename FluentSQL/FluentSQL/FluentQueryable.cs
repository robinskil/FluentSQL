using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentSQL
{
    public class FluentQueryable<T> : IFluentQueryable<T> where T : class
    {
        private readonly IProvider _provider;
        private readonly QueryBuilder _queryBuilder;
        private readonly QueryExecutor _queryExecutor;

        public FluentQueryable(FluentSqlOptions fluentSqlOptions)
        {
            _provider = fluentSqlOptions.Provider;
            _queryBuilder = new QueryBuilder(fluentSqlOptions, typeof(T));
            _queryExecutor = new QueryExecutor(_queryBuilder, fluentSqlOptions.Provider);
        }

        protected FluentQueryable(IProvider provider, QueryBuilder queryBuilder, QueryExecutor queryExecutor)
        {
            _provider = provider;
            _queryBuilder = queryBuilder;
            _queryExecutor = queryExecutor;
        }
        
        public T First()
        {
            return _queryExecutor.ExecuteQueryAndMap<T>().First();
        }

        public List<T> ToList()
        {
            return _queryExecutor.ExecuteQueryAndMap<T>();
        }

        public IFluentQueryable<TOut> Select<TOut>(Expression<Func<T, TOut>> selectExpression) where TOut : class
        {
            _queryBuilder.AddSelectExpression(selectExpression);
            return new FluentQueryable<TOut>(_provider,_queryBuilder,_queryExecutor);
        }

        public IFluentQueryable<T> Where(Expression<Func<T, bool>> whereExpression)
        {
            _queryBuilder.AddWhereExpression(whereExpression);
            return this;
        }

        public IFluentQueryable<T> Select(Expression<Func<T, T>> selectExpression)
        {
            _queryBuilder.AddSelectExpression(selectExpression);
            return this;
        }
    }

    public class FluentJoinedQueryable<T, TInclude>
    {
        
    }
}