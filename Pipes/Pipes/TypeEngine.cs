using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Pipes
{
    public static class TypeEngine
    {
        public const string InterfacePrefix = "I";
        public static Type GenerateInterfaceTypeFromContract(Contract contract,ModuleBuilder moduleBuilder)
        {
            var typeBuilder = moduleBuilder.DefineType(InterfacePrefix+contract.Name, TypeAttributes.Interface);
            foreach (var property in contract.Properties)
            {
                typeBuilder.DefineProperty(property.Name,PropertyAttributes.HasDefault,property.Type.PropertyTypeToClrType(),Type.EmptyTypes);
            }
            typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            return typeBuilder.CreateType();
        }

        public static Type GenerateObjectTypeFromContract(Contract contract, ModuleBuilder moduleBuilder,
            Type contractInterfaceType)
        {
            var typeBuilder = moduleBuilder.DefineType(contract.Name,TypeAttributes.Class|TypeAttributes.Public);
            foreach (var property in contract.Properties)
            {
                typeBuilder.DefineProperty(property.Name,PropertyAttributes.HasDefault,property.Type.PropertyTypeToClrType(),Type.EmptyTypes);
            }
            typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            return typeBuilder.CreateType();
        }
        
        public static Type PropertyTypeToClrType(this PropertyType propertyType)
        {
            switch (propertyType)
            {
                case PropertyType.String:
                    return typeof(string);
                case PropertyType.Int64:
                    return typeof(long);
                case PropertyType.Float64:
                    return typeof(double);
                case PropertyType.Guid:
                    return typeof(Guid);
                case PropertyType.Boolean:
                    return typeof(bool);
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }
    }
}