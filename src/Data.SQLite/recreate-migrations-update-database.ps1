$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    # Delete only the database named by ConnectionStrings:DefaultConnection in appsettings.json, with its journal and
    # sidecar files, so other SQLite files in this folder are left alone.
    $connectionString = (Get-Content -LiteralPath appsettings.json -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    # Parsed with ADO.NET connection-string rules, so quoted values (Data Source="my data.db") are unquoted.
    $builder = [System.Data.Common.DbConnectionStringBuilder]@{ ConnectionString = $connectionString }
    $aliases = @(@('Data Source', 'DataSource', 'Filename') | Where-Object { $builder.ContainsKey($_) })
    if ($aliases.Count -gt 1) {
        # Microsoft.Data.Sqlite uses the last alias, so picking one here could delete a different file.
        throw "ConnectionStrings:DefaultConnection names the database more than once ($($aliases -join ', ')); keep one."
    }
    $dataSource = if ($aliases.Count -eq 1) { [string] $builder[$aliases[0]] }
    # Only ReadWriteCreate (the default) can create the replacement database; any other mode (ReadWrite, ReadOnly,
    # Memory, 3, an unknown value) leaves the file alone.
    $mode = if ($builder.ContainsKey('Mode')) { ([string] $builder['Mode']).Trim() } else { 'ReadWriteCreate' }
    if (-not $dataSource -or $dataSource -eq ':memory:') {
        throw "No SQLite file is named by ConnectionStrings:DefaultConnection in appsettings.json."
    }
    if ($mode -ne 'ReadWriteCreate') {
        throw "ConnectionStrings:DefaultConnection uses Mode=$mode; only ReadWriteCreate can recreate the database."
    }
    if ($dataSource -match '^\s*file:' -or $dataSource -like '*|DataDirectory|*') {
        throw "ConnectionStrings:DefaultConnection uses a URI or |DataDirectory| path ('$dataSource'); delete the database manually."
    }

    foreach ($suffix in '', '-journal', '-wal', '-shm') {
        # Missing files are fine; a file that exists but cannot be deleted (locked, no permission) stops the script.
        if (Test-Path -LiteralPath "$dataSource$suffix") {
            Remove-Item -LiteralPath "$dataSource$suffix" -Force -Confirm:$false -ErrorAction Stop
        }
    }

    ./recreate-migrations.ps1
    ./update-database.ps1
}
finally {
    Pop-Location
}
