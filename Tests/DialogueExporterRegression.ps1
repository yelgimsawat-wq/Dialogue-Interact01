$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$paths = @(
    'Tests/DialogueExporterRegression.cs',
    'Assets/Dialogue/DialogueInGame/DialogueGraphExporter.cs',
    'Assets/Dialogue/DialogueInGame/DialogueGraphExporter.SingleChoice.cs',
    'Assets/Dialogue/DialogueInGame/DialogueGraphExporter.MultipleChoice.cs',
    'Assets/Dialogue/DialogueInGame/DialogLine.cs',
    'Assets/Dialogue/DialogueInGame/DialogueContainer.cs',
    'Assets/Dialogue/Data/Save/DSChoiceSaveData.cs'
) | ForEach-Object { Join-Path $root $_ }
Add-Type -Path $paths
[ExporterRegression]::Run()
Write-Output 'PASS: mixed/multiple-only graphs, edited/deleted choices, terminal choices, and Next with duplicate names in either node order'
