using System.Collections.Generic;
using FluentSQL.Mapping;

namespace FluentSQL
{
    public interface ISelectExpressionResolver : IExpressionResolver
    {
        List<TypeMapping> GenerateMappings();
    }
}