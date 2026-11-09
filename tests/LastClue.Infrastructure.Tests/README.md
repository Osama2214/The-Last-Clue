# Infrastructure tests

Core SQL model tests generate the SQL Server schema and verify the constraints, indexes and relationships that protect room and game state. They run without a database server. Tests that apply migrations and exercise database constraints against a running SQL Server are tracked separately.
