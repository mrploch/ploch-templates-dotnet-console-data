using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ploch.TemplateApp.Model;

namespace Ploch.TemplateApp.Data.Configurations;

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        // The Author-Article relationship is configured from the Article side in ArticleConfiguration.
        // Basic constraints (Name length / required) are expressed via data annotations on the entity.
    }
}
