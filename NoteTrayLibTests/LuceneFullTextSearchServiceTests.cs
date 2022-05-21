using NoteTrayLib.Services;
using NUnit.Framework;

namespace NoteTrayLibTests;

[TestFixture]
public class LuceneFullTextSearchServiceTests
{
    private LuceneFullTextSearchService _searchService = new LuceneFullTextSearchService(new FileTrackerService(new SQLiteDatabaseService("testdb")), "searchindex");
    
    [Test]
    public void BuildIndexTest()
    {
        _searchService.BuildIndex(@"C:\Users\Taylor\Dropbox\org\wiki", true);
        Assert.Pass();
    }
    
    [Test]
    public void SearchIndexTest()
    {
        _searchService.Search("Clockwork");
        Assert.Pass();
    }

    [Test]
    public void IndexFileTest()
    {
        _searchService.IndexFile(@"C:\Users\Taylor\Dropbox\org\wiki\good_movies.org");
        _searchService.Search("pest");
        Assert.Pass();
    }

    [Test]
    public void RemoveFileTest()
    {
        _searchService.RemoveFile(@"C:\Users\Taylor\Dropbox\org\wiki\good_movies.org");
        _searchService.Search("pest");
        Assert.Pass();
    }
}