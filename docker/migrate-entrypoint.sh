#!/bin/sh
# Applies EF Core migrations, then the idempotent seed scripts in numeric order.
# 01_schema.sql is skipped: the migration bundle already creates the schema.
set -eu

: "${MSSQL_HOST:?MSSQL_HOST is required}"
: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
MSSQL_DATABASE="${MSSQL_DATABASE:-Taskist}"
SEED_DATA="${SEED_DATA:-true}"
SEED_DUMMY_DATA="${SEED_DUMMY_DATA:-false}"

CONNECTION="Server=${MSSQL_HOST},1433;Database=${MSSQL_DATABASE};User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;"

echo "Applying migrations to ${MSSQL_DATABASE}..."
./efbundle --connection "$CONNECTION"

run_script() {
    echo "Running $(basename "$1")..."
    sqlcmd -S "${MSSQL_HOST},1433" -U sa -P "$MSSQL_SA_PASSWORD" -d "$MSSQL_DATABASE" \
        -C -b -I -f 65001 -i "$1"
}

if [ "$SEED_DATA" = "true" ]; then
    for script in ./seed/02_*.sql ./seed/03_*.sql; do
        run_script "$script"
    done
else
    echo "SEED_DATA is not true; skipping reference data."
fi

if [ "$SEED_DUMMY_DATA" = "true" ]; then
    for script in ./seed/04_*.sql; do
        run_script "$script"
    done
fi

echo "Database is ready."
