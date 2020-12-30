﻿using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.CmsKit.Tags;
using Tag = Volo.CmsKit.Tags.Tag;

namespace Volo.CmsKit.MongoDB.Tags
{
    public class MongoTagRepository : MongoDbRepository<ICmsKitMongoDbContext, Volo.CmsKit.Tags.Tag, Guid>, ITagRepository
    {
        public MongoTagRepository(IMongoDbContextProvider<ICmsKitMongoDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public Task<bool> AnyAsync(
            [NotNull] string entityType,
            [NotNull] string name,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            return GetMongoQueryable()
                    .AnyAsync(x =>
                            x.EntityType == entityType &&
                            x.Name == name &&
                            x.TenantId == tenantId,
                        cancellationToken);
        }

        public Task<Tag> GetAsync(
            [NotNull] string entityType,
            [NotNull] string name,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            return GetAsync(x =>
                x.EntityType == entityType &&
                x.Name == name &&
                x.TenantId == tenantId,
               cancellationToken: cancellationToken);
        }

        public Task<Tag> FindAsync(
            [NotNull] string entityType,
            [NotNull] string name,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            return FindAsync(x =>
                x.EntityType == entityType &&
                x.Name == name &&
                x.TenantId == tenantId,
               cancellationToken: cancellationToken);
        }

        public virtual async Task<List<Tag>> GetAllRelatedTagsAsync(
            [NotNull] string entityType,
            [NotNull] string entityId,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            var entityTagIds = await DbContext.EntityTags.AsQueryable()
                .Where(q => q.EntityId == entityId && q.TenantId == tenantId)
                .Select(q => q.TagId)
                .ToListAsync(cancellationToken: GetCancellationToken(cancellationToken));

            var query = GetMongoQueryable()
                            .Where(x => 
                                x.EntityType == entityType &&
                                x.TenantId == tenantId &&
                                entityTagIds.Contains(x.Id));

            var result = await query.ToListAsync(cancellationToken: GetCancellationToken(cancellationToken));
            return result;
        }
    }
}
