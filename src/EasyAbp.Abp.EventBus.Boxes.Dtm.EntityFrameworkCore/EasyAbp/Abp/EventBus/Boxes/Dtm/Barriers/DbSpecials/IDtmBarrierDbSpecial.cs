using System.Diagnostics.CodeAnalysis;
using DtmCommon;
using EasyAbp.Abp.EventBus.Boxes.Dtm.Options;

namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers.DbSpecials;

public interface IDtmBarrierDbSpecial : IDbSpecial
{
    string GetCreateBarrierTableSql(AbpDtmEventBoxesOptions options);
    
    string GetInsertIgnoreTemplate([NotNull] string tableName);
    
    string GetQueryPreparedSql([NotNull] string tableName);
}