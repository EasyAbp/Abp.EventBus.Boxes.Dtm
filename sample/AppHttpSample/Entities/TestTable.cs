using Volo.Abp.Domain.Entities;

namespace AppHttpSample.Entities;

public class TestTable: AggregateRoot<Guid>
{
    private TestTable()
    {
        
    }
    public TestTable(Guid id,string name)
    {
        this.Id = id;
        this.Name = name;
    }
    public object[] GetKeys()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; private set; }
}