using App.Business.Attributes;

namespace Project.Application.Helpers;

public class SwaggerExcludeFormDataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content == null)
            return;

        foreach (var content in operation.RequestBody.Content.Values)
        {
            var schema = content.Schema;
            if (schema == null)
                continue;

            var targetSchema = schema;

            if (schema.Reference != null &&
                context.SchemaRepository.Schemas.TryGetValue(schema.Reference.Id, out var referencedSchema))
            {
                targetSchema = referencedSchema;
            }

            if (targetSchema.Properties == null || targetSchema.Properties.Count == 0)
                continue;

            var parameterTypes = context.MethodInfo.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            foreach (var type in parameterTypes)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (!ShouldIgnore(property))
                        continue;

                    var jsonName = GetJsonName(property);

                    targetSchema.Properties.Remove(property.Name);
                    targetSchema.Properties.Remove(jsonName);

                    targetSchema.Required?.Remove(property.Name);
                    targetSchema.Required?.Remove(jsonName);
                }
            }
        }
    }

    private static bool ShouldIgnore(PropertyInfo property) =>
        Attribute.IsDefined(property, typeof(SwaggerExcludeAttribute)) ||
        Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)) ||
        Attribute.IsDefined(property, typeof(BindNeverAttribute));

    private static string GetJsonName(PropertyInfo property)
    {
        var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
        if (!string.IsNullOrWhiteSpace(jsonName))
            return jsonName!;

        return property.Name.Length == 1
            ? property.Name.ToLowerInvariant()
            : char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
    }
}