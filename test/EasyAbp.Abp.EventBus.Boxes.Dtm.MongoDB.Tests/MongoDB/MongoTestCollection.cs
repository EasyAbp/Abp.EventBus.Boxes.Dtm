using Xunit;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.MongoDB;

[CollectionDefinition(Name)]
public class MongoTestCollection : ICollectionFixture<MongoDbFixture>
{
    public const string Name = "MongoDB Collection";
}
