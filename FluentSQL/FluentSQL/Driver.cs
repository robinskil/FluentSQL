using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;
using FluentSQL.Mapping.Processors;

namespace FluentSQL
{
    public sealed class Driver
    {
        private readonly FluentSqlOptions _options;
        private readonly IProvider _provider;

        public Driver(FluentSqlOptions options)
        {
            _options = options;
            _provider = options.Provider;
        }

        public IFluentQueryable<T> From<T>() where T : class
        {
            return new FluentQueryable<T>(_options);
        }
    }
}