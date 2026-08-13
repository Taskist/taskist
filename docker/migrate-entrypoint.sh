#!/bin/sh
# Applies EF Core migrations and seeds reference data, then exits.
# Safe to re-run: migrations are tracked, and the seed scripts guard their inserts.

set -e

: "${MSSQL_HOST:=db}"
: "${MSSQL_PORT:=1433}"
: "${MSSQL_USER:=sa}"
: "${MSSQL_DATABASE:=Taskist}"
: "${SEED_DATA:=true}"
: "${SEED_DUMMY_DATA:=false}"

if [ -z "$MSSQL_SA_PASSWORD" ]; then
    echo "ERROR: MSSQL_SA_PASSWORD is not set."
    exit 1
fi

if [ -z "$TASKIST_ConnectionStrings__AppContext" ]; then
    echo "ERROR: TASKIST_ConnectionStrings__AppContext is not set."
    exit 1
fi

SQLCMD="sqlcmd -S ${MSSQL_HOST},${MSSQL_PORT} -U ${MSSQL_USER} -P ${MSSQL_SA_PASSWORD} -C -b"

echo "Waiting for SQL Server at ${MSSQL_HOST}:${MSSQL_PORT}..."

attempt=0
until $SQLCMD -Q "SELECT 1" > /dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ "$attempt" -ge 60 ]; then
        echo "ERROR: SQL Server did not become available in time."
        exit 1
    fi
    sleep 2
done

echo "SQL Server is up."

# the EF bundle needs the database to exist before it can connect
echo "Ensuring database [${MSSQL_DATABASE}] exists..."
$SQLCMD -Q "IF DB_ID('${MSSQL_DATABASE}') IS NULL CREATE DATABASE [${MSSQL_DATABASE}];"

echo "Applying EF Core migrations..."
./efbundle --connection "$TASKIST_ConnectionStrings__AppContext"

if [ "$SEED_DATA" = "true" ]; then
    # 1_defaults and 2_locale_resource are required for a usable install;
    # 3_dummy_data is sample content and stays opt-in.
    for script in ./seed/1_defaults.sql ./seed/2_locale_resource.sql; do
        echo "Seeding $(basename "$script")..."
        $SQLCMD -d "$MSSQL_DATABASE" -i "$script"
    done

    if [ "$SEED_DUMMY_DATA" = "true" ]; then
        echo "Seeding sample data..."
        $SQLCMD -d "$MSSQL_DATABASE" -i ./seed/3_dummy_data.sql
    fi
fi

echo "Database is ready."
