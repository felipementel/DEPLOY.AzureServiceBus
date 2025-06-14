param (
    [string]$testsFolder = "tests",
    [string]$reportTitle = "Cobertura de Testes",
    [string]$sonarExclusions = "",
    [string]$runNumber = "local",
    [string]$runId = (Get-Random)
)

# Define cor amarela para a saída
$YELLOW = "`e[33m"

Write-Host "🔍 Procurando projetos de teste em: $testsFolder"
$projects = Get-ChildItem -Path $testsFolder -Recurse -Filter *.csproj

if ($projects.Count -eq 0) {
    Write-Host "❌ Nenhum projeto de teste encontrado em '$testsFolder'."
    exit 1
}

$index = 0

foreach ($proj in $projects) {
    Write-Host "➡️ Rodando testes com cobertura para: $($proj.FullName)"
    dotnet test $proj.FullName `
        --verbosity Minimal `
        --configuration Debug `
        --collect:"XPlat Code Coverage"
    $index++
}

Write-Host "${YELLOW}➡️ Gerando relatório HTML e Cobertura..."

reportgenerator `
    -reports:"**/TestResults/**/coverage.cobertura.xml" `
    -targetdir:"coveragereport" `
    -reportTypes:"Cobertura;Html;MarkdownSummaryGithub;SonarQube" `
    -title:"$reportTitle" `
    -classfilters:"$sonarExclusions" `
    -tag:"$runNumber_$runId"

Write-Host "`n✅ Processo concluído. Relatórios gerados na pasta: coveragereport"
