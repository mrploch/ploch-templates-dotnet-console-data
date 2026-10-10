$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    # Remove only this project's database (the one named in appsettings.json) and its journal and sidecar files.
    Remove-Item template-app.db, template-app.db-journal, template-app.db-wal, template-app.db-shm -Force -Confirm:$false -ErrorAction SilentlyContinue
    ./recreate-migrations.ps1
    ./update-database.ps1
}
finally {
    Pop-Location
}
