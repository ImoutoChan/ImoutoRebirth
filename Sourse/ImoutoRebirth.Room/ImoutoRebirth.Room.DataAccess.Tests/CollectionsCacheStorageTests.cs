using ImoutoRebirth.Room.DataAccess.Cache;
using Xunit;

namespace ImoutoRebirth.Room.DataAccess.Tests
{
    public class CollectionsCacheStorageTests
    {
        private const string Format = "C:\\Users\\pc\\Downloads\\aspnetboilerplate-samples-master\\aspnetboilerplate-samples-master\\IdentityServerDemo\\docker\\{0:D10}.jpg";

        [Fact(Skip = "OneTime")]
        public void MemoryTest()
        {
            //var storage = new CollectionsCacheStorage();

            var hashSet = new List<string>();

            var path = string.Empty;
            for (int i = 0; i < 10000000; i++)
            {
                path = $"C:\\Users\\pc\\Downloads\\aspnetboilerplate-samples-master\\aspnetboilerplate-samples-master\\IdentityServerDemo\\docker\\{i:D10}.jpg";

                var parts = path;//.Split("\\").Select(string.Intern).ToArray();

                hashSet.Add(parts);
            }

            //var colleciton = new OversawCollection(new Collection(Guid.Empty, "Test"),
            //    new[] {new SourceFolder(default, default, default, default, default, default, default, default)},
            //    new HashSet<string>(),
            //    new DefaultDestinationFolder());

            //Assert.NotNull(colleciton);
        }


        [Fact(Skip = "OneTime")]
        public void BloomMemoryTest()
        {

            var bloom = new BloomFilter<string>(200000);

            Parallel.For(0, 200_000, i =>
            {
                var path = string.Format(Format, i);

                var parts = path;//.Split("\\").Select(string.Intern).ToArray();

                bloom.Add(parts);
            });

            var random = new Random();

            int misses = 0,
                successes = 0;

            for (int i = 0; i < 1_000_000; i++)
            {
                var generated = random.Next(0, 1_000_000);

                var result = bloom.Contains(string.Format(Format, generated));

                if (result)
                {
                    if (generated < 200_000)
                    {
                        successes++;
                    }
                    else
                    {
                        misses++;
                    }
                }
                else
                {
                    if (generated < 200_000)
                    {
                        misses++;
                    }
                    else
                    {
                        successes++;
                    }
                }
            }
        }

        // hashset 1kk - 350mb / list - 320mb
    }
}
