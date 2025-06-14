#!/bin/bash

# =============================
# Configurações
# =============================
testsFolder="${1:-tests}"
reportTitle="${2:-Cobertura de Testes}"
sonarExclusions="${3:- }"
runNumber="local"
runId=$(date +%s)

# =============================
# Cores
# =============================
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# =============================
# Verifica se há projetos
# =============================
echo "🔍 Procurando projetos de teste em: $testsFolder"
mapfile -t projects < <(find "$testsFolder" -name '*.csproj')

if [ ${#projects[@]} -eq 0 ]; then
    echo "❌ Nenhum projeto de teste encontrado em '$testsFolder'."
    exit 1
fi

# =============================
# Executa testes
# =============================
index=0
for proj in "${projects[@]}"; do
    echo "➡️ Rodando testes com cobertura para: $proj"
    dotnet test "$proj" \
        --verbosity minimal \
        --configuration Debug \
        --collect:"XPlat Code Coverage"
    ((index++))
done

# =============================
# Gera relatório
# =============================
echo -e "${YELLOW}➡️ Gerando relatório HTML e Cobertura...${NC}"

reportgenerator \
    -reports:"**/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reportTypes:"Cobertura;Html;MarkdownSummaryGithub;SonarQube" \
    -title:"$reportTitle" \
    -classfilters:"$sonarExclusions" \
    -tag:"${runNumber}_${runId}"

echo -e "\n✅ Processo concluído. Relatórios gerados em: coveragereport"
