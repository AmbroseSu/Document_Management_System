using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SignRequestResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        // Chuyển 'FileData' thành 'file_data'
        if (propertyName == "FileData") return "file_data";
        // Chuyển 'Options' thành 'options'
        if (propertyName == "Options") return "options";
        return propertyName;
    }
}