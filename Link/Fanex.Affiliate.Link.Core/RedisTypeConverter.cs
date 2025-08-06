using StackExchange.Redis;

namespace Fanex.Affiliate.Link.Core;

public static class RedisTypeConverter
{
    private static MessagePackSerializer serializer = new MessagePackSerializer();

    public static RedisValue ToRedisValue(object value)
    {
        if (typeof(byte[]) == value.GetType())
        {
            return (byte[])value;
        }

        return serializer.Serialize(value);
    }

    public static T ToObject<T>(RedisValue value)
    {
        var binary = (byte[])value;
        if (typeof(T) == typeof(byte[]))
        {
            return (T)Convert.ChangeType(binary, typeof(T));
        }

        return serializer.Deserialize<T>(binary);
    }
}
