using System.ComponentModel.DataAnnotations;
using Ploch.Data.Model;

namespace Ploch.TemplateApp.Model;

public class Author : IHasId<int>, INamed, IHasDescription, IHasAuditProperties
{
    /// <summary>
    ///     The maximum length of <see cref="Name" />.
    /// </summary>
    public const int NameMaxLength = 128;

    public int Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? ModifiedTime { get; set; }

    public DateTimeOffset? AccessedTime { get; set; }

    public string? CreatedBy { get; set; }

    public string? LastModifiedBy { get; set; }

    public string? LastAccessedBy { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
