$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    # Delete only the database named by ConnectionStrings:DefaultConnection in appsettings.json, with its journal and
    # sidecar files, so other SQLite files in this folder are left alone.
    $connectionString = (Get-Content -LiteralPath appsettings.json -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    $dataSource = ($connectionString -split ';' |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ -match '^(Data ?Source|Filename)\s*=' } |
        Select-Object -First 1) -replace '^[^=]+=\s*', ''
    if (-not $dataSource -or $dataSource -eq ':memory:') {
        throw "No SQLite file is named by ConnectionStrings:DefaultConnection in appsettings.json."
    }

    foreach ($suffix in '', '-journal', '-wal', '-shm') {
        Remove-Item -LiteralPath "$dataSource$suffix" -Force -Confirm:$false -ErrorAction SilentlyContinue
    }

    ./recreate-migrations.ps1
    ./update-database.ps1
}
finally {
    Pop-Location
}
