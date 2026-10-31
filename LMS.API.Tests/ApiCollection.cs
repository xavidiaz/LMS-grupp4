namespace LMS.API.Tests;

// All test classes share one API and one database, and run one at a time
[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory>;
