namespace App.Business.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SwaggerExcludeAttribute : Attribute
{
}