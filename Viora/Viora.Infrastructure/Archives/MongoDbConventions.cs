using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Infrastructure.Archives;

internal static class MongoDbConventions
{
    private static bool _registered;

    public static void RegisterConventions()
    {
        if (_registered) return;
        _registered = true;

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonClassMap.RegisterClassMap<Entity>(cm =>
        {
            cm.AutoMap();
            cm.UnmapMember(c => c.DomainEvents);
        });

        BsonClassMap.RegisterClassMap<Template>(cm =>
        {
            cm.AutoMap();
            cm.MapField("_versions");
        });

        BsonClassMap.RegisterClassMap<TemplateVersion>(cm =>
        {
            cm.AutoMap();
            cm.MapField("_fields");
        });

        BsonClassMap.RegisterClassMap<TemplateField>(cm =>
        {
            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<Record>(cm =>
        {
            cm.AutoMap();
            cm.MapField("_values");
            cm.MapField("_attachments");
        });

        BsonClassMap.RegisterClassMap<Folder>(cm =>
        {
            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<Archive>(cm =>
        {
            cm.AutoMap();
        });
    }
}
