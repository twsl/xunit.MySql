using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#else
using JetBrains.Annotations;
#endif

namespace Xunit.MySql.Extensions
{
    /// <summary>
    /// Extensions class for DbContext
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Gets the Entity of {T} from the database by using the provided <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="T">The class of the return value object.</typeparam>
        /// <param name="context">The DbContext the <paramref name="query"/> is executed on.</param>
        /// <param name="query">The MySql query string.</param>
        /// <returns>The Entity of Type {T}.</returns>
        public static IEnumerable<T> GetEntity<T>([NotNull]this DbContext context, string query) where T : new()
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var t = new T();

                var values = new object[reader.FieldCount];
                reader.GetValues(values);

                var properties = t.GetType().GetProperties().ToList();

                for (var i = 0; i < values.Length; i++)
                {
                    var name = reader.GetName(i);
                    if (properties.Count() != 0)
                    {
                        var value = values[i];
                        var property = properties.Where(p => p.Name == name).FirstOrDefault();

                        Type tProp = property.PropertyType;

                        // Nullable properties have to be treated differently, since we use their underlying property to set the value in the object.
                        if (tProp.IsGenericType && tProp.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            // if it's null, just set the value from the reserved word null, and return.
                            if (value == null || value.GetType() == typeof(DBNull))
                            {
                                property.SetValue(t, null, null);
                                continue;
                            }

                            // Get the underlying type property instead of the nullable generic.
                            tProp = new NullableConverter(property.PropertyType).UnderlyingType;
                        }

                        property.SetValue(t, Convert.ChangeType(value, tProp), null);
                    }
                    else
                    {
                        t = values.Length == 1 ? (T)values[i] : (T)(object)values;
                    }
                }

                yield return t;
            }
        }
    }
}
