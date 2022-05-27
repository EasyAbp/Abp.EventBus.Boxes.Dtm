namespace EasyAbp.Abp.EventBus.Boxes.Dtm.Barriers
{
    [global::MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class DtmBarrierDocument
    {
        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("trans_type")]
        public string TransType { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("gid")]
        public string GId { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("branch_id")]
        public string BranchId { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("op")]
        public string Op { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("barrier_id")]
        public string BarrierId { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElement("reason")]
        public string Reason { get; set; }
    }
}