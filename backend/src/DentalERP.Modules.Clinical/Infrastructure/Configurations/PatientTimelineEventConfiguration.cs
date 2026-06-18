using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class PatientTimelineEventConfiguration : IEntityTypeConfiguration<PatientTimelineEvent>
{
    public void Configure(EntityTypeBuilder<PatientTimelineEvent> builder)
    {
        builder.ToTable("patient_timeline");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description");
        builder.Property(e => e.ActorId).HasColumnName("actor_id");
        builder.Property(e => e.ActorName).HasColumnName("actor_name").HasMaxLength(100);
        builder.Property(e => e.LinkedEntityType).HasColumnName("linked_entity_type").HasMaxLength(50);
        builder.Property(e => e.LinkedEntityId).HasColumnName("linked_entity_id");
        builder.Property(e => e.Metadata).HasColumnName("metadata");
        builder.Property(e => e.EventAt).HasColumnName("event_at").IsRequired();
        builder.Property(e => e.IsVisibleToDoctor).HasColumnName("is_visible_to_doctor").IsRequired();
        builder.Property(e => e.IsVisibleToPatient).HasColumnName("is_visible_to_patient").IsRequired();
        builder.Property(e => e.EventCategory).HasColumnName("event_category").HasMaxLength(20).IsRequired();

        // Append-only: no updates or deletes
        builder.HasIndex(e => new { e.PatientId, e.EventAt }).HasDatabaseName("ix_timeline_patient");
        builder.HasIndex(e => e.EventType).HasDatabaseName("ix_timeline_event_type");
        builder.HasIndex(e => new { e.PatientId, e.EventCategory }).HasDatabaseName("ix_timeline_category");
    }
}
