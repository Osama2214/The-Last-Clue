# Infrastructure tests

The SQL model tests inspect the generated schema without a database server. The repository integration tests require SQL Server and create a fresh database for the test class. Set LAST_CLUE_TEST_SQLSERVER to a connection string for a disposable SQL Server instance with permission to create and drop databases, then run the Infrastructure test project. The fixture chooses a unique database name and removes only that database. These tests are skipped when the variable is unset.
