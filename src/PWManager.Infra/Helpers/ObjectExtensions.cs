using Google.Cloud.Firestore;

namespace PWManager.Infra.Helpers
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts an object of type T to a dictionary, while converting DateTime and Guid fields appropriately.
        /// </summary>
        public static Dictionary<string, object> EntityToDictionary<T>(T entity)
        {
            if (entity == null)
            {
                return null;
            }

            var dictionary = new Dictionary<string, object>();

            foreach (var property in typeof(T).GetProperties())
            {
                object propertyValue = property.GetValue(entity);
                dictionary[property.Name] = ProcessValueForDictionary(propertyValue);
            }

            return dictionary;
        }

        /// <summary>
        /// Converts a dictionary to an object of type T, while converting DateTime and Guid fields appropriately.
        /// </summary>
        public static T DictionaryToEntity<T>(Dictionary<string, object> dictionary) where T : new()
        {
            if (dictionary == null)
            {
                return default;
            }

            var entity = new T();

            foreach (var property in typeof(T).GetProperties())
            {
                if (!dictionary.TryGetValue(property.Name, out var value))
                {
                    continue;
                }

                property.SetValue(entity, ProcessValueForEntity(value, property.PropertyType));
            }

            return entity;
        }

        private static object ProcessValueForDictionary(object value)
        {
            switch (value)
            {
                case DateTime dt:
                    return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                case Guid guid:
                    return guid.ToString();
                default:
                    return value;
            }
        }

        private static object ProcessValueForEntity(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                return ((Timestamp)value).ToDateTime();
            }

            if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                return Guid.Parse(value.ToString());
            }

            return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        }
    }
}
