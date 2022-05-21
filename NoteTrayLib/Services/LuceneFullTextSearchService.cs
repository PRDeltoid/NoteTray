using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NoteTrayLib.Models;
using NoteTrayLib.Utilities;
using Serilog;
using Directory = Lucene.Net.Store.Directory;

namespace NoteTrayLib.Services;

public class LuceneFullTextSearchService : IFullTextSearchService
{
    const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
    
    private readonly IndexWriter _writer;
    private readonly Analyzer _standardAnalyzer;
    private readonly QueryParser _queryParser;

    public event EventHandler<SearchResultEventArgs> SearchResultsAvailable; 

    public LuceneFullTextSearchService(FileTrackerService fileTracker, string indexName)
    {
        fileTracker.FileTracked += FileTrackerOnFileTracked;
        fileTracker.FileRemoved += FileTrackerOnFileRemoved;
        // Compatibility version
        string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
        Directory indexDir = FSDirectory.Open(indexPath);

        // Create an analyzer to process the text 
        _standardAnalyzer = new StandardAnalyzer(luceneVersion);

        // Create an index writer
        IndexWriterConfig indexConfig = new IndexWriterConfig(luceneVersion, _standardAnalyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND,
        };
        _writer = new IndexWriter(indexDir, indexConfig);
        
        // Standard query parser to make parsing user input into queries simpler
        _queryParser = new QueryParser(luceneVersion, "content", _standardAnalyzer);
    }

    public void BuildIndex(string searchPath, bool flushIndex)
    {
        if (flushIndex)
        {
            // Since this builds a new index, we should delete the old index data
            _writer.DeleteAll();
        }
        
        // Recursively index searchPath and all subdirectories
        IndexDirectory(searchPath, true);

        // Flush and commit the index data to the directory
        _writer.Commit();
    }

    public IEnumerable<SearchResultModel> Search(string text)
    {
        // Parse the user's query text
        Query query = _queryParser.Parse(text);
        
        Log.Information("Search query: {query}", text);
        
        // Search
        using DirectoryReader reader = _writer.GetReader(applyAllDeletes: true);
        IndexSearcher searcher = new IndexSearcher(reader);
        TopDocs topDocs = searcher.Search(query, n: 20);

        List<SearchResultModel> results = new List<SearchResultModel>();
        
        // Show results
        foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
        {
            // Find the actual document from the score doc data
            Document resultDoc = searcher.Doc(scoreDoc.Doc);
            // Extract the important info from the index
            string name = resultDoc.Get("name");
            string fullPath = Path.Combine(resultDoc.Get("path", name));
            
            // parse it as a search result
            SearchResultModel result = new SearchResultModel()
            {
                FullPath = fullPath,
                Name = name,
                SearchScore = scoreDoc.Score
            };
            
            results.Add(result);
        }

        Log.Information($"Matching results: {topDocs.TotalHits}");
        
        SearchResultsAvailable?.Invoke(this, new SearchResultEventArgs(text, results));
        return results;
    }
    
    public void IndexFile(string filePath)
    {
        // A crude upsert where the document is replaced if it already exists 
        // Otherwise it is created normally
        BooleanQuery query = ConstructKeyQuery(filePath);
        _writer.DeleteDocuments(query);
        Document doc = ConstructDocument(filePath);
        _writer.AddDocument(doc);
    }

    public void RemoveFile(string filePath)
    {
        BooleanQuery query = ConstructKeyQuery(filePath);
        _writer.DeleteDocuments(query);
    }
    
    private void IndexDirectory(string directoryPath, bool recursive)
    {
        foreach (NoteListItem file in DirectoryUtilities.GetChildFiles(directoryPath))
        {
            IndexFile(file.FullPath);
        }

        if (recursive)
        {
            foreach (NoteListItem directory in DirectoryUtilities.GetChildDirectories(directoryPath))
            {
                IndexDirectory(directory.FullPath, true);
            }
        }
    } 
    
    private void FileTrackerOnFileRemoved(object sender, string e)
    {
        Log.Debug("Removing file from index: {filepath}", e);
        // Unindex the removed file
        RemoveFile(e);
    }

    private void FileTrackerOnFileTracked(object sender, TrackedFileModel e)
    {
        Log.Debug("Indexing file: {filepath}", e.FullPath);
        IndexFile(e.FullPath);
    }
    
    private static BooleanQuery ConstructKeyQuery(string filePath)
    {
        return new BooleanQuery
        {
            { new TermQuery(new Term("path", Path.GetDirectoryName(filePath))), Occur.MUST },
            { new TermQuery(new Term("name", Path.GetFileName(filePath))), Occur.MUST }
        }; 
    }

    private static Document ConstructDocument(string filePath)
    {
        Document doc = new Document();

        StringField docPath = new StringField("path",  Path.GetDirectoryName(filePath), Field.Store.YES);
        doc.Add(docPath);
        
        StringField docTitle = new StringField("name",  Path.GetFileName(filePath), Field.Store.YES);
        doc.Add(docTitle);
        
        StreamReader reader = File.OpenText(filePath);
        TextField docText = new TextField("content", reader); 
        doc.Add(docText);
        return doc;
    }
}