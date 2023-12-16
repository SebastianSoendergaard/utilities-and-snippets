// See https://aka.ms/new-console-template for more information
using Clone;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Object cloning");

var item = new Dto
{
    Number = 123,
    Text = "abc",
    SecretText = new SensitiveString("def"),
    DateTime = DateTime.Now,
    SubDtoA = new SubDtoA { N = new SensitiveInt(85), T = "ertghjnm" },
    SubDtoB = new SubDtoB(85, new SensitiveString("ertghjnm")),
    SubList = new[]
    {
        new SubDtoA
        {
            N =new SensitiveInt(53),
            T = "dfghj"
        },
        new SubDtoA
        {
            N = new SensitiveInt(75),
            T = "ojhgf"
        }
    },
    SubDic = new Dictionary<string, SubDtoB>
    {
        { "A", new SubDtoB(56, new SensitiveString("gasdf")) },
        { "B", new SubDtoB(58, new SensitiveString("hyer")) }
    }
};

var clone = item; // Clone(item);

bool isEqual = clone == item;

var serializeOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters =
    {
        new SensitiveValueConverter() 
        //new IgnoreTypeInJsonConverter<object>()
    }
};

Console.WriteLine($"item == clone: {isEqual}");
Console.WriteLine($"item: {JsonSerializer.Serialize(item, serializeOptions)}");
Console.WriteLine($"item: {JsonSerializer.Serialize(item)}");
Console.WriteLine($"clone: {JsonSerializer.Serialize(clone)}");

Console.ReadKey();

//object? Clone<T>(T item)
//{
//    if (item == null)
//    {
//        return null;
//    }

//    Type type = item.GetType();
//    var clone = new ExpandoObject();

//    foreach (var property in type.GetProperties())
//    {
//        var value = GetPropertyNameAndValue(property);
//        clone.TryAdd(value.Name, value.Value);
//    }

//    return clone;
//}

//(string Name, object? Value) GetPropertyNameAndValue(PropertyInfo property)
//{
//    Type attributeType = typeof(MaskAttribute);

//    if (property.CustomAttributes.Select(a => a.AttributeType.FullName == attributeType.FullName).Any())
//    {
//        return (property.Name, "***masked***");
//    }

//    var propertyValue = property.GetValue(item);
//    if (propertyValue == null)
//    {
//        return (property.Name, null);
//    }

//    var propertyType = property.PropertyType;
//    if (!propertyType.IsClass)
//    {
//        return (property.Name, propertyValue);
//    }

//    if (propertyType == typeof(string))
//    {
//        return (property.Name, propertyValue);
//    }

//    if (typeof(IEnumerable<object>).IsAssignableFrom(propertyType))
//    {
//        return (property.Name, ((IEnumerable<object>)propertyValue).Select(x => Clone(x)));
//    }

//    var propertyClone = Clone(propertyValue);
//    return (property.Name, propertyClone);
//}

//public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
//{
//    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException();
//    }

//    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException();
//    }

//    //public override void Write(Utf8JsonWriter writer, DateTimeOffset dateTimeValue, JsonSerializerOptions options)
//    //{
//    //    writer.WriteStringValue(dateTimeValue.ToString(
//    //        "MM/dd/yyyy", CultureInfo.InvariantCulture));
//    //}
//}

public class SensitiveValueConverter : JsonConverter<object>
{
    private static Type sensitiveType = typeof(ISensitiveValue);

    public override bool CanConvert(Type typeToConvert)
    {
        var canConvert = typeToConvert.GetInterfaces().Any(i => i == sensitiveType);
        return canConvert;
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Does not support serializing");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var sensitiveValue = (ISensitiveValue)value;
        var maskedValue = sensitiveValue.ToMaskedValue();



        //writer.WriteStartObject();

        JsonSerializer.Serialize(writer, maskedValue, options);

        //writer.WriteEndObject();

        //writer.WriteStringValue(sensitiveValue.ToMaskedValue());
    }
}


//public class IgnoreTypeInJsonConverter<G> : JsonConverter<G> where G : class
//{
//    private bool _ignore = false;

//    public override bool CanConvert(Type typeToConvert)
//    {
//        var canConvert = !_ignore;
//        _ignore = false;
//        return canConvert;

//        //Type attributeType = typeof(MaskAttribute);
//        //if (typeToConvert.CustomAttributes.Where(a => a.AttributeType.FullName == attributeType.FullName).Any())
//        //{
//        //    return true;
//        //}
//        //return false;
//        //return true; // typeof(G) == typeToConvert;//.IsAssignableFrom(typeToConvert);
//    }

//    public override G? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException("Does not support serializing");
//    }

//    public override void Write(Utf8JsonWriter writer, G value, JsonSerializerOptions options)
//    {
//        //writer.WriteStringValue("***masked***");
//        Type attributeType = typeof(MaskAttribute);
//        Type valueType = value.GetType();

//        if (valueType.CustomAttributes.Where(a => a.AttributeType.FullName == attributeType.FullName).Any())
//        {
//            writer.WriteStringValue("***masked***");
//        }
//        else
//        {
//            _ignore = true;
//            JsonSerializer.Serialize(writer, value, options);
//        }
//    }

//}


