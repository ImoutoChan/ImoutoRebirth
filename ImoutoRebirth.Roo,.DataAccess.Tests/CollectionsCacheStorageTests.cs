using System;
using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;
using Xunit;

namespace ImoutoRebirth.Roo_.DataAccess.Tests
{
    public class CollectionsCacheStorageTests
    {
        [Fact]
        public void MemoryTest()
        {
            var storage = new CollectionsCacheStorage();

            var hashSet = new List<string[]>();

            string path = string.Empty;
            for (int i = 0; i < 1000000; i++)
            {
                path = $"C:\\Users\\pc\\Downloads\\aspnetboilerplate-samples-master\\aspnetboilerplate-samples-master\\IdentityServerDemo\\docker\\{i:D10}.jpg";

                var parts = path.Split("\\").Select(string.Intern).ToArray();

                hashSet.Add(parts);
            }

            //var colleciton = new OverseedColleciton(new Collection(Guid.Empty, "Test"),
            //    new[] {new SourceFolder(default, default, default, default, default, default, default, default)},
            //    new HashSet<string>(),
            //    new DefaultDestinationDirectory());

            //Assert.NotNull(colleciton);
        }

        // hashset 1kk - 350mb / list - 320mb
    }
}
