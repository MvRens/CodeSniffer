using LiteDB;

namespace CodeSniffer.Repository.LiteDB
{
    internal static class BsonExtensions
    {
        public static T ToObject<T>(this BsonDocument value, BsonMapper? mapper = null)
        {
            var mapperInstance = mapper ?? BsonMapper.Global;
            return mapperInstance.ToObject<T>(value);
        }


        public static T[] ToArray<T>(this BsonArray array, BsonMapper? mapper = null)
        {
            if (array.Count == 0)
                return Array.Empty<T>();

            var mapperInstance = mapper ?? BsonMapper.Global;

            return array
                .Select(i => (T)mapperInstance.Deserialize(typeof(T), i))
                .ToArray();
        }
    }
}
