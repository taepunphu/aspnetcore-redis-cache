using System;
using System.IO;
using System.Threading.Tasks;
using Hyperion;
using Microsoft.Extensions.Caching.Distributed;

namespace aspnetcore_redis.helpers
{
    [Obsolete]
    public static class IDistributedCacheExtension
    {
        public static async Task SetObjectAsync<T>(this IDistributedCache cache,
        string id, T value, double lifespan = 14400,
        bool sliding = false) where T : class
        {
            //Serialization
            var serializer = new Serializer(new SerializerOptions().WithPreserveObjectReferences(true));
            await using var mem = new MemoryStream();
            serializer.Serialize(value, mem);
            var towrite = mem.ToArray();
            if (sliding)
            {
                await cache.SetAsync(id, towrite, new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromSeconds(lifespan)
                });
            }
            else
            {
                await cache.SetAsync(id, towrite, new DistributedCacheEntryOptions()
                {
                    //บวกเวลาจากปัจจุบัน
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lifespan)
                });
            }
        }

        [Obsolete]
        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string id) where T : class
        {
            var bytes = await cache.GetAsync(id);
            if (bytes == null) return default(T);
            var serializer = new Serializer(new SerializerOptions().WithPreserveObjectReferences(true));
            await using var mem = new MemoryStream(bytes);
            var value = serializer.Deserialize<T>(mem);
            return value;
        }
    }
}