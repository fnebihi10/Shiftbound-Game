# Bakes a visible, collision-free opening detail layer into the committed scene YAML.
# Unity's Editor bake command may later replace this layer with authored chamfered meshes.
$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent $PSScriptRoot
$scenePath = Join-Path $repository 'UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity'
$scene = [System.IO.File]::ReadAllText($scenePath)
$rootName = 'Visual Benchmark V5 - opening roofs'
if ($scene.Contains("m_Name: $rootName")) { throw 'V5 scene layer already exists; refusing to duplicate it.' }

$rendererMatch = [regex]::Match($scene, '(?ms)^--- !u!23 &\d+\r?\nMeshRenderer:.*?(?=^--- !u!)')
if (-not $rendererMatch.Success) { throw 'Could not find a MeshRenderer template.' }
$rendererTemplate = $rendererMatch.Value
$materials = @{
    stone = '4471a3f63373ff949a674bbae88547a5'
    facade = 'ed1e281e7740ca445ab1c3ff8fc785f6'
    steel = 'a13f18eb69037b742b238b45f5c84c02'
    zinc = 'e6a605fd2498d2649b6ae6320a1df432'
    brass = '334bafd5d24c78e49ae3a0d2df95d5f7'
    moss = 'ee64358a8b9ee774fbab0974f6ac3a55'
    leaf = '95c1e81f8d4451b4988f8bbf3ac280bc'
    leafLight = '85e24e877d4aac24b84bc70477ab3987'
    tar = 'a13f18eb69037b742b238b45f5c84c02'
}
$parts = [System.Collections.Generic.List[object]]::new()
function Add-Part([string]$name, [double]$x, [double]$y, [double]$z,
    [double]$sx, [double]$sy, [double]$sz, [string]$material,
    [int]$mesh = 10202, [bool]$overgrown = $false) {
    $script:parts.Add([pscustomobject]@{
        Name=$name; X=$x; Y=$y; Z=$z; SX=$sx; SY=$sy; SZ=$sz
        Material=$material; Mesh=$mesh; Overgrown=$overgrown
    })
}

# Construction layers read at gameplay camera distance while leaving the center route open.
foreach ($x in @(-4.05, 4.05)) {
    Add-Part 'V5 masonry parapet wall' $x 0.13 2.3 0.23 0.50 6.6 facade
    Add-Part 'V5 stone roof coping' $x 0.43 2.3 0.34 0.11 6.8 stone
    Add-Part 'V5 zinc drip edge' $x -0.25 2.3 0.32 0.065 6.8 zinc
    foreach ($z in @(-0.7, 1.3, 3.3, 5.3)) {
        Add-Part 'V5 parapet pier' $x 0.23 $z 0.32 0.49 0.34 facade
        Add-Part 'V5 parapet cap' $x 0.54 $z 0.40 0.09 0.42 stone
    }
}
Add-Part 'V5 layered start roof fascia' 0 -0.81 6.95 8.35 0.53 0.23 facade
Add-Part 'V5 start roof flashing' 0 -0.34 6.95 8.4 0.08 0.32 zinc
foreach ($x in @(-3.35, -1.65, 0.05, 1.75, 3.45)) {
    Add-Part 'V5 roof edge support bracket' $x -0.69 7.09 0.075 0.38 0.15 steel
}
foreach ($z in @(7.05, 11.95, 16.45)) {
    $width = if ($z -lt 13) { 5.05 } else { 3.05 }
    Add-Part 'V5 landing edge flashing' 0 -0.22 $z $width 0.07 0.13 zinc
}

# Existing HVAC collider silhouettes stay in place. Added housings and feet are visual only.
foreach ($x in @(-2.7, 2.7)) {
    Add-Part 'V5 HVAC raised lid' $x 1.19 2.4 1.49 0.11 1.57 zinc
    foreach ($side in @(-0.47, 0.47)) {
        Add-Part 'V5 HVAC mounting foot' ($x+$side) 0.05 2.4 0.18 0.12 1.12 steel
    }
    for ($i=0; $i -lt 6; $i++) {
        Add-Part 'V5 HVAC cooling blade' $x (0.28+$i*0.11) 1.60 0.78 0.025 0.08 zinc
    }
    Add-Part 'V5 HVAC electrical conduit' $x 0.045 3.47 0.08 0.08 0.78 steel
}
Add-Part 'V5 roof cable tray' -3.42 0.055 4.9 0.23 0.09 2.15 steel
for ($i=0; $i -lt 5; $i++) {
    Add-Part 'V5 cable tray strap' -3.42 0.13 (4.02+$i*0.43) 0.27 0.025 0.05 zinc
}
Add-Part 'V5 roof drain outer frame' 3.35 0.019 5.44 0.70 0.03 0.73 zinc
Add-Part 'V5 roof drain throat' 3.35 0.039 5.44 0.56 0.008 0.58 tar
for ($i=0; $i -lt 5; $i++) {
    Add-Part 'V5 roof drain bar' (3.10+$i*0.125) 0.05 5.44 0.035 0.016 0.55 steel
}
Add-Part 'V5 localized service patch' -3.42 0.012 4.1 0.36 0.014 0.71 tar
Add-Part 'V5 localized drainage patch' 3.24 0.012 4.6 0.37 0.014 0.71 tar

# Uneven, rooted growth follows parapets, the drain and the first alternate bridge.
$spots = @(
    @(-3.71,0.12,0.16), @(-3.76,0.10,2.21), @(-3.67,0.12,5.34),
    @(3.73,0.11,0.94), @(3.69,0.12,3.31), @(3.34,0.09,5.50),
    @(-2.13,0.09,9.13), @(2.10,0.10,11.18), @(-1.32,0.08,14.20),
    @(1.24,0.10,15.48)
)
for ($i=0; $i -lt $spots.Count; $i++) {
    $p=$spots[$i]
    $scale=0.40+($i%4)*0.09
    Add-Part 'V5 rooted leaf mound' $p[0] $p[1] $p[2] $scale 0.20 ($scale*0.76) $(if($i%3 -eq 0){'leafLight'}else{'leaf'}) 10207 $true
    Add-Part 'V5 tapered plant stem' $p[0] 0.16 $p[2] 0.055 (0.29+($i%3)*0.05) 0.055 leaf 10206 $true
}
foreach ($x in @(-3.79,3.79)) {
    for ($i=0; $i -lt 5; $i++) {
        Add-Part 'V5 irregular parapet moss' $x 0.014 (-0.50+$i*1.45+($i%2)*0.22) (0.19+($i%3)*0.08) 0.018 0.38 moss 10207 $true
    }
}
Add-Part 'V5 drain moss accumulation' 3.35 0.022 4.91 0.39 0.02 0.32 moss 10207 $true

$rootObject = [long]3000000000
$rootTransform = $rootObject + 1
$childRefs = [System.Text.StringBuilder]::new()
$growthRefs = [System.Text.StringBuilder]::new()
$documents = [System.Text.StringBuilder]::new()
$culture = [System.Globalization.CultureInfo]::InvariantCulture
function Number([double]$n) { return $n.ToString('0.########', $culture) }
for ($i=0; $i -lt $parts.Count; $i++) {
    $p=$parts[$i]
    $id=[long]3000000010 + [long]$i*4
    $transform=$id+1; $renderer=$id+2; $filter=$id+3
    [void]$childRefs.Append("  - {fileID: $transform}`r`n")
    if ($p.Overgrown) { [void]$growthRefs.Append("  - {fileID: $id}`r`n") }
    $active=if($p.Overgrown){0}else{1}
    $go=@"
--- !u!1 &$id
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $transform}
  - component: {fileID: $renderer}
  - component: {fileID: $filter}
  m_Layer: 0
  m_Name: $($p.Name)
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: $active
--- !u!4 &$transform
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $id}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: $(Number $p.X), y: $(Number $p.Y), z: $(Number $p.Z)}
  m_LocalScale: {x: $(Number $p.SX), y: $(Number $p.SY), z: $(Number $p.SZ)}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: $rootTransform}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"@
    $render=[regex]::Replace($rendererTemplate, '^--- !u!23 &\d+', "--- !u!23 &$renderer", 'Multiline')
    $render=[regex]::Replace($render, 'm_GameObject: \{fileID: \d+\}', "m_GameObject: {fileID: $id}")
    $render=[regex]::Replace($render, '(?m)^  - \{fileID: 2100000, guid: [a-f0-9]+, type: 2\}', "  - {fileID: 2100000, guid: $($materials[$p.Material]), type: 2}")
    $render=[regex]::Replace($render, 'm_CastShadows: \d+', 'm_CastShadows: 0')
    $render=[regex]::Replace($render, 'm_MotionVectors: \d+', 'm_MotionVectors: 0')
    $meshDoc="--- !u!33 &$filter`r`nMeshFilter:`r`n  m_ObjectHideFlags: 0`r`n  m_CorrespondingSourceObject: {fileID: 0}`r`n  m_PrefabInstance: {fileID: 0}`r`n  m_PrefabAsset: {fileID: 0}`r`n  m_GameObject: {fileID: $id}`r`n  m_Mesh: {fileID: $($p.Mesh), guid: 0000000000000000e000000000000000, type: 0}`r`n"
    [void]$documents.Append($go.TrimStart("`r","`n") + "`r`n" + $render.TrimEnd("`r","`n") + "`r`n" + $meshDoc)
}
$root=@"
--- !u!1 &$rootObject
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $rootTransform}
  m_Layer: 0
  m_Name: $rootName
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$rootTransform
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $rootObject}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
$($childRefs.ToString().TrimEnd("`r","`n"))
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"@
$sceneRoots = [regex]::Match($scene, '(?ms)^--- !u!1660057539 &9223372036854775807\r?\nSceneRoots:.*\z')
if (-not $sceneRoots.Success) { throw 'SceneRoots block missing.' }
$scene = $scene.Insert($sceneRoots.Index, $root.TrimStart("`r","`n") + "`r`n" + $documents.ToString())
$scene = $scene.TrimEnd("`r","`n") + "`r`n  - {fileID: $rootTransform}`r`n"
$growthList = [regex]::Match($scene, '(?m)^  overgrownOnly:\r?\n(?:  - \{fileID: \d+\}\r?\n)+')
if (-not $growthList.Success) { throw 'RooftopWorldArt growth list missing.' }
$scene = $scene.Insert($growthList.Index + $growthList.Length, $growthRefs.ToString())
$scene = [regex]::Replace($scene, '(?<!\r)\n', "`r`n")
[System.IO.File]::WriteAllText($scenePath, $scene, [System.Text.UTF8Encoding]::new($false))
Write-Output "Baked $($parts.Count) non-colliding V5 visual pieces into GoldenRooftops.unity."
