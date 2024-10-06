# Automatic Database Migration Script

# Parametri
param(
    [string]$ProjectPath = (Get-Location),
    [string]$StartupProjectName = "LingoShift",
    [string]$DbContextName = "AppDbContext"
)

# Funzione per eseguire i comandi di Entity Framework Core
function Invoke-EfCommand {
    param(
        [string]$Command
    )
    
    $output = & dotnet ef $Command.Split() -s $StartupProjectName -c $DbContextName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Errore nell'esecuzione del comando EF Core: $Command"
        Write-Error $output
        exit 1
    }
    return $output
}

# Cambia la directory corrente nel percorso del progetto
Set-Location $ProjectPath

# Verifica se ci sono migrazioni pendenti
$pendingMigrations = Invoke-EfCommand "migrations list --json" | ConvertFrom-Json
$pendingCount = ($pendingMigrations | Where-Object { -not $_.applied }).Count

if ($pendingCount -gt 0) {
    Write-Host "Ci sono $pendingCount migrazioni pendenti. Applicazione in corso..."

    # Applica le migrazioni pendenti
    Invoke-EfCommand "database update"

    Write-Host "Migrazioni applicate con successo."
} else {
    Write-Host "Nessuna migrazione pendente trovata."
}

# Verifica se ci sono cambiamenti nel modello che richiedono una nuova migrazione
$modelChanges = Invoke-EfCommand "migrations has-changes"

if ($modelChanges -match "Cambiamenti rilevati nel modello.") {
    Write-Host "Rilevati cambiamenti nel modello. Creazione di una nuova migrazione..."

    # Genera un nome per la nuova migrazione basato sulla data e ora corrente
    $migrationName = "AutoMigration_" + (Get-Date -Format "yyyyMMddHHmmss")

    # Crea una nuova migrazione
    Invoke-EfCommand "migrations add $migrationName"

    Write-Host "Nuova migrazione '$migrationName' creata con successo."

    # Applica la nuova migrazione
    Invoke-EfCommand "database update"

    Write-Host "Nuova migrazione applicata con successo."
} else {
    Write-Host "Nessun cambiamento nel modello rilevato. Non è necessaria una nuova migrazione."
}

Write-Host "Processo di migrazione completato."