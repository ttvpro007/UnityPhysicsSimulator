using System.Reflection;

public static class ReflectionHelper
{
    /// <summary>
    /// Retrieves the value of a private field from an object by its field name using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the field.</typeparam>
    /// <param name="obj">The object instance to retrieve the field value from.</param>
    /// <param name="fieldName">The name of the private field.</param>
    /// <returns>The string representation of the field's value or an error message if not found.</returns>
    public static string GetPrivateFieldValue<T>(T obj, string fieldName)
    {
        // Retrieve the field using reflection
        FieldInfo fieldInfo = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        // Check if the field was found
        if (fieldInfo != null)
        {
            // Get the value of the field
            object fieldValue = fieldInfo.GetValue(obj);
            // Return the value as a string
            return fieldValue?.ToString() ?? "null";
        }

        // Return error message if field not found
        return "Field not found";
    }


    public static bool CastProjectile<T>(Projectile projectile, out T result) where T : Projectile
    {
        result = projectile as T;
        return result != null;
    }
}
