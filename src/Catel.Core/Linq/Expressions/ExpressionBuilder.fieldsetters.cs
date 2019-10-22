﻿namespace Catel.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Catel.Reflection;

    public static partial class ExpressionBuilder
    {
        public static Expression<Action<T, TField>> CreateFieldSetter<T, TField>(string fieldName)
        {
            Argument.IsNotNullOrWhitespace(() => fieldName);

            var field = typeof(T).GetFieldEx(fieldName);
            return field is null ? null : CreateFieldSetter<T, TField>(field);
        }

        public static Expression<Action<T, TField>> CreateFieldSetter<T, TField>(FieldInfo fieldInfo)
        {
            Argument.IsNotNull(() => fieldInfo);

            return CreateFieldSetterExpression<T, TField>(fieldInfo);
        }

        public static Expression<Action<T, object>> CreateFieldSetter<T>(string fieldName)
        {
            Argument.IsNotNullOrWhitespace(() => fieldName);

            var field = typeof(T).GetFieldEx(fieldName);
            return field is null ? null : CreateFieldSetter<T>(field);
        }

        public static Expression<Action<T, object>> CreateFieldSetter<T>(FieldInfo fieldInfo)
        {
            Argument.IsNotNull(() => fieldInfo);

            return CreateFieldSetterExpression<T, object>(fieldInfo);
        }

        public static IReadOnlyDictionary<string, Expression<Action<T, object>>> CreateFieldSetters<T>()
        {
            var fieldSetters = new Dictionary<string, Expression<Action<T, object>>>(StringComparer.OrdinalIgnoreCase);
            var fields = typeof(T).GetFieldsEx();

            foreach (var field in fields)
            {
                var fieldSetter = CreateFieldSetter<T>(field);
                if (fieldSetter is null)
                {
                    continue;
                }

                fieldSetters[field.Name] = fieldSetter;
            }

            return new ReadOnlyDictionary<string, Expression<Action<T, object>>>(fieldSetters);
        }

        public static IReadOnlyDictionary<string, Expression<Action<T, TField>>> CreateFieldSetters<T, TField>()
        {
            var fieldSetters = new Dictionary<string, Expression<Action<T, TField>>>(StringComparer.OrdinalIgnoreCase);
            var properties = typeof(T).GetFieldsEx().Where(w => w.FieldType == typeof(TField));

            foreach (var field in properties)
            {
                var fieldSetter = CreateFieldSetter<T, TField>(field);
                if (fieldSetter is null)
                {
                    continue;
                }

                fieldSetters[field.Name] = fieldSetter;
            }

            return new ReadOnlyDictionary<string, Expression<Action<T, TField>>>(fieldSetters);
        }

        private static Expression<Action<T, TField>> CreateFieldSetterExpression<T, TField>(FieldInfo fieldInfo)
        {
            var targetType = fieldInfo.DeclaringType;
            if (targetType is null)
            {
                return null;
            }

            var target = Expression.Parameter(targetType, "target");
            var parameter = Expression.Parameter(typeof(TField), "field");

            var fieldExp = Expression.Field(target, fieldInfo);
            var body = Expression.Assign(fieldExp, Expression.Convert(parameter, fieldInfo.FieldType));

            var lambda = Expression.Lambda<Action<T, TField>>(body, target, parameter);
            return lambda;
        }
    }
}