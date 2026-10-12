-- Runs once, when the pgdata volume is first created (docker-entrypoint-initdb.d).
-- A read-only role for the agent's MCP server (spec 002 research R7). Local-only password, supplied to the
-- MCP server through the BOOKSTORE_RO_PASSWORD environment variable — never written into settings.json.
CREATE ROLE bookstore_ro LOGIN PASSWORD 'bookstore_ro_local';
GRANT CONNECT ON DATABASE bookstore TO bookstore_ro;
GRANT USAGE ON SCHEMA public TO bookstore_ro;
-- Tables are created later by the API (EnsureCreated), so grant on future tables owned by the app role.
ALTER DEFAULT PRIVILEGES FOR ROLE bookstore IN SCHEMA public GRANT SELECT ON TABLES TO bookstore_ro;
