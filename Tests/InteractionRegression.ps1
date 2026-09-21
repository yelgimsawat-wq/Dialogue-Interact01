$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$paths = @(
    'Tests/InteractionRegression.cs',
    'Assets/Gantarino/Interact/Scripts/InteractableObject.cs',
    'Assets/Gantarino/Interact/Scripts/IInteractable.cs',
    'Assets/Ping/QuestSystem.cs',
    'Assets/Ping/QuestSystem/QuestManager.cs',
    'Assets/Ping/QuestSystem/QuestSet.cs',
    'Assets/Ping/QuestSystem/QuestAcceptResult.cs',
    'Assets/Ping/QuestSystem/QuestInstance.cs',
    'Assets/Ping/QuestSystem/QuestObjective_Child.cs',
    'Assets/Ping/QuestSystem/QuestProgression.cs'
) | ForEach-Object { Join-Path $root $_ }
Add-Type -Path $paths -CompilerOptions @('/nowarn:0649')
[InteractionRegression]::Run()
Write-Output 'PASS: legacy migration, combined actions, quest acceptance, duplicate prevention, event targets, completion, and missing system'
