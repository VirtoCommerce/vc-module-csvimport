using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogCsvImportModule.Tests
{
    public static class TestUtils
    {
        public static Mock<IQueryable<TEntity>> CreateQuerableMock<TEntity>(List<TEntity> set) where TEntity : Entity
        {
            IQueryable<TEntity> queryableData = set.AsQueryable();
            Mock<IQueryable<TEntity>> mock = new Mock<IQueryable<TEntity>>();

            mock.As<IQueryable<TEntity>>().Setup(x => x.Provider).Returns(queryableData.Provider);
            mock.As<IQueryable<TEntity>>().Setup(x => x.Expression).Returns(queryableData.Expression);
            mock.As<IQueryable<TEntity>>().Setup(x => x.ElementType).Returns(queryableData.ElementType);
            mock.As<IQueryable<TEntity>>().Setup(x => x.GetEnumerator()).Returns(queryableData.GetEnumerator());

            return mock;
        }

        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
