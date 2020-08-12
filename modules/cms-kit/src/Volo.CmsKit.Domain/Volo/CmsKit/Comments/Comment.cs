﻿using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Volo.CmsKit.Comments
{
    public class Comment: Entity<Guid>, IAggregateRoot<Guid>, IHasCreationTime, IMustHaveCreator, IMultiTenant
    {
        public virtual Guid? TenantId { get; protected set; }

        public virtual string EntityType { get; protected set; }

        public virtual string EntityId { get; protected set; }

        public virtual string Text { get; protected set; }

        public virtual Guid? RepliedCommentId { get; protected set; }

        public virtual Guid CreatorId { get; set; }

        public virtual DateTime CreationTime { get; set; }

        protected Comment()
        {

        }

        public Comment(
            Guid id,
            [NotNull] string entityType,
            [NotNull] string entityId,
            [NotNull] string text,
            Guid? repliedCommentId,
            Guid creatorId,
            Guid? tenantId = null)
            : base(id)
        {
            EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType), CommentConsts.EntityTypeLength);
            EntityId = Check.NotNullOrWhiteSpace(entityId, nameof(entityId), CommentConsts.EntityIdLength);
            RepliedCommentId = repliedCommentId;
            CreatorId = creatorId;
            TenantId = tenantId;

            SetText(text);
        }

        public virtual void SetText(string text)
        {
            Text = Check.NotNullOrWhiteSpace(text, nameof(text), CommentConsts.MaxTextLength);
        }
    }
}
