﻿[assembly: CollectionBehavior(DisableTestParallelization = true)]
[CollectionDefinition("Sequential Database Tests", DisableParallelization = true)]
public class SequentialDatabaseTestsCollection : ICollectionFixture<TestSetup>
{
    // This class has no code, it's just a marker to define a test collection.
}
