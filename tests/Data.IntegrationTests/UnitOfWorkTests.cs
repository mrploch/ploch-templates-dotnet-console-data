using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.TemplateApp.Data;
using Ploch.TemplateApp.Model;

namespace Ploch.TemplateApp.Data.IntegrationTests;

public class UnitOfWorkTests : GenericRepositoryDataIntegrationTest<TemplateAppDbContext>
{
    [Fact]
    public async Task CommitAsync_should_persist_changes_across_multiple_repositories()
    {
        var unitOfWork = CreateUnitOfWork();

        var authorRepo = unitOfWork.Repository<Author, int>();
        var articleRepo = unitOfWork.Repository<Article, int>();

        var author = new Author { Name = "Test Author" };
        await authorRepo.AddAsync(author, TestContext.Current.CancellationToken);

        var article = new Article { Title = "Test Article", Author = author };
        await articleRepo.AddAsync(article, TestContext.Current.CancellationToken);

        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Verify both rows landed in the database via a fresh root DbContext, not the writer's context.
        await using var verifyContext = CreateRootDbContext();
        var savedAuthor = await verifyContext.Authors.FindAsync([author.Id], TestContext.Current.CancellationToken);
        var savedArticle = await verifyContext.Articles.FindAsync([article.Id], TestContext.Current.CancellationToken);

        savedAuthor.Should().NotBeNull();
        savedArticle.Should().NotBeNull();
        savedArticle!.AuthorId.Should().Be(savedAuthor!.Id);
    }

    [Fact]
    public async Task CommitAsync_should_update_audit_modified_time_on_update()
    {
        var unitOfWork = CreateUnitOfWork();
        var articleRepo = unitOfWork.Repository<Article, int>();

        var article = new Article { Title = "Original Title" };
        await articleRepo.AddAsync(article, TestContext.Current.CancellationToken);
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        var createdTime = article.CreatedTime;
        createdTime.Should().NotBeNull();

        await Task.Delay(50, TestContext.Current.CancellationToken);

        article.Title = "Updated Title";
        await articleRepo.UpdateAsync(article, TestContext.Current.CancellationToken);
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Confirm the audit timestamp was advanced and persisted by reloading from the database.
        await using var verifyContext = CreateRootDbContext();
        var saved = await verifyContext.Articles.FindAsync([article.Id], TestContext.Current.CancellationToken);

        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Updated Title");
        saved.ModifiedTime.Should().NotBeNull();
        saved.ModifiedTime.Should().BeOnOrAfter(createdTime!.Value);
    }

    [Fact]
    public async Task DeleteAsync_should_remove_entity()
    {
        var unitOfWork = CreateUnitOfWork();
        var tagRepo = unitOfWork.Repository<ArticleTag, int>();

        var tag = new ArticleTag { Name = "Temporary Tag" };
        await tagRepo.AddAsync(tag, TestContext.Current.CancellationToken);
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        var tagId = tag.Id;

        await tagRepo.DeleteAsync(tag, TestContext.Current.CancellationToken);
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Verify removal via a fresh root DbContext rather than the repository under test.
        await using var verifyContext = CreateRootDbContext();
        var deleted = await verifyContext.ArticleTags.FindAsync([tagId], TestContext.Current.CancellationToken);
        deleted.Should().BeNull();
    }
}
