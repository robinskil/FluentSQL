using System.Collections.Generic;
using System.Data.Common;

namespace FluentSQL
{
    public interface IExpressionResolver
    {
        string GetSqlExpression();
        void ValidateExpression();
        IReadOnlyList<DbParameter> GetParameters();
    }
}