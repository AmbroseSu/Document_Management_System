using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

public class OptionsResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization);
        foreach (var prop in props)
        {
            prop.PropertyName = prop.PropertyName.ToUpperInvariant();
        }
        return props;
    }
}