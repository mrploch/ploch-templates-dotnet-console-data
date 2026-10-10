using System.ComponentModel.DataAnnotations;
using Ploch.Data.Model;

namespace Ploch.TemplateApp.Model;

public class Article : IHasId<int>, IHasTitle, IHasDescription, IHasContents, IHasAuditProperties,
                       IHasCategories<ArticleCategory>, IHasTags<ArticleTag>
{
    /// <summary>
    ///     The maximum length of <see cref="Title" />.
    /// </summary>
    public const int TitleMaxLength = 256;

    public int Id { get; set; }

    [Required]
    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public string? Contents { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? ModifiedTime { get; set; }

    public DateTimeOffset? AccessedTime { get; set; }

    public string? CreatedBy { get; set; }

    public string? LastModifiedBy { get; set; }

    public string? LastAccessedBy { get; set; }

    public int? AuthorId { get; set; }

    public virtual Author? Author { get; set; }

    public virtual ICollection<ArticleCategory>? Categories { get; set; } = new List<ArticleCategory>();

    public virtual ICollection<ArticleTag> Tags { get; set; } = new List<ArticleTag>();

    public virtual ICollection<ArticleProperty> Properties { get; set; } = new List<ArticleProperty>();
}
