using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Mabs.AudioCpp.Utils;

/// <summary>
/// Resolver that adds per-property ShouldSerialize hooks honoring <see cref="JsonIgnoreAttribute"/>
/// and the global ignore condition, plus <c>ShouldSerialize{PropertyName}()</c> methods on the
/// model type. Names follow the CLR property name from the attribute provider, so
/// <c>[JsonPropertyName]</c>-renamed properties are looked up by their CLR name.
/// </summary>
internal sealed class ShouldSerializeJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private static bool IsIgnored(object? value, JsonIgnoreCondition ignoreCondition)
    {
        if (value is null)
        {
            return ignoreCondition == JsonIgnoreCondition.WhenWritingNull;
        }

        if (value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType())))
        {
            return ignoreCondition == JsonIgnoreCondition.WhenWritingDefault;
        }

        return false;
    }

    private static Func<object, bool>? MakeShouldSerializeWrapper(Type type, string propertyName)
    {
        MethodInfo? method = type.GetMethod($"ShouldSerialize{propertyName}", []);
        if (method == null || method.ReturnType != typeof(bool))
        {
            return null;
        }

        return MakeShouldSerializeFnFromMethod(method);
    }

    private static Func<object, bool> MakeShouldSerializeFnFromMethod(MethodInfo method)
    {
        Expression<Func<object, bool>> expression = obj => (bool)method.Invoke(obj, new object?[0])!;
        return expression.Compile();
    }

    private static T? GetCustomAttributeIncludeInterfaces<T>(Type type, string propertyName)
        where T : Attribute
    {
        if (type.GetProperty(propertyName) is { } property)
        {
            return property.GetCustomAttribute<T>(true);
        }

        foreach (Type interfaceType in type.GetInterfaces())
        {
            if (!(interfaceType.GetProperty(propertyName) is { } interfaceProperty))
            {
                continue;
            }

            return interfaceProperty.GetCustomAttribute<T>(true);
        }

        return null;
    }

    private static Func<object, object?, bool> MakeShouldSerializeFn(Type type, string propertyName, JsonIgnoreCondition ignoreCondition)
    {
        JsonIgnoreAttribute? jsonIgnore = GetCustomAttributeIncludeInterfaces<JsonIgnoreAttribute>(type, propertyName);
        if (jsonIgnore != null)
        {
            if (jsonIgnore.Condition == JsonIgnoreCondition.Always)
            {
                return (obj, val) => false;
            }

            ignoreCondition = jsonIgnore.Condition;
        }

        Func<object, bool>? shouldSerializeFn = MakeShouldSerializeWrapper(type, propertyName);
        if (shouldSerializeFn == null)
        {
            return (obj, val) => !IsIgnored(val, ignoreCondition);
        }

        // STJ codegen skips its own DefaultIgnoreCondition check once a delegate is set, so the
        // delegate must apply the condition itself: it is the whole decision.
        return (obj, val) => !IsIgnored(val, ignoreCondition) && shouldSerializeFn(obj);
    }

    /// <inheritdoc/>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            // If the type is not an object, we don't need to modify its properties.
            return typeInfo;
        }

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            MemberInfo? memberInfo = property.AttributeProvider as MemberInfo;
            property.ShouldSerialize = MakeShouldSerializeFn(type, memberInfo?.Name ?? property.Name, options.DefaultIgnoreCondition);
        }

        return typeInfo;
    }
}