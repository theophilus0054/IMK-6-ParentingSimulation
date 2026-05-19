# 🎮 UNITY EDITOR SETUP GUIDE - Parenting Simulation

## Part 1: HIERARCHY STRUCTURE

### Recommended Scene Hierarchy:
```
Scene
├── GameManager (GameObject)
│   ├── GameManager (script)
│   ├── SaveLoadManager (script)
│   ├── SymptomDatabaseLoader (script)
│   └── DiagnosisValidator (script)
├── Baby (GameObject with 3D Model)
│   ├── BabyBehavior (script)
│   ├── BabyDisease (script)
│   ├── BabyAnimator (script)
│   ├── BabyAudioCue (script)
│   ├── Animator (built-in Unity component)
│   ├── AudioSource (built-in Unity component)
│   ├── SkinnedMeshRenderer (from imported FBX model)
│   └── [3D Model Child Objects from Ch31_nonPBR@T-Pose.fbx]
├── InputTester (GameObject) - Optional for testing
│   └── Test_Input (script)
└── Particles (Folder - Optional)
    ├── CryingTearsVFX (Particle System)
    └── FeverSweatVFX (Particle System)
```

---

## Part 2: GAMEMANAGER SETUP

### Step 1: Create GameManager GameObject
```
Right-click in Hierarchy → Create Empty
Rename to: GameManager
Position: (0, 0, 0)
```

### Step 2: Add Components
```
Inspector → Add Component:
1. GameManager (Script)
2. SaveLoadManager (Script)
3. SymptomDatabaseLoader (Script)
4. DiagnosisValidator (Script)
```

### Step 3: Configure GameManager Inspector

**Inspector Fields to Fill:**

| Field | Value | Type | Notes |
|-------|-------|------|-------|
| Current Phase | Neonatal | Enum | Start phase |
| Current State | Menu | Enum | Initial state |
| Current Day | 1 | int | Start day |
| Neonatal Phase Days | 7 | int | Duration fase neonatal |
| Toddler Phase Days | 7 | int | Duration fase toddler |
| Baby Behavior | [Drag Baby GO] | Reference | Required! |

**How to assign Baby Behavior:**
1. Select GameManager in Hierarchy
2. In Inspector, find "Baby Behavior" field
3. Drag Baby GameObject from Hierarchy → drop to field

### Step 4: Setup DontDestroyOnLoad (Optional)
```
Edit → Preferences → add code ke Awake():
// Sudah di code: if (Instance == null) Instance = this;
```

---

## Part 3: BABY GAMEOBJECT SETUP

### Step 1: Create Baby GameObject
```
Right-click in Hierarchy → 3D Object → Create Empty
Rename to: Baby
Position: (0, 0, 0)
```

### Step 2: Import 3D Model
```
Assets folder → Drag Ch31_nonPBR@T-Pose.fbx ke Baby GameObject
Atau: Assets/Object/Ch31_nonPBR@T-Pose.fbx → drag ke scene
```

### Step 3: Add Required Components
```
Select Baby GameObject in Hierarchy
Inspector → Add Component:
1. BabyBehavior
2. BabyDisease
3. BabyAnimator
4. BabyAudioCue
5. Animator (Built-in - Window → Animation → Animator)
6. AudioSource (Built-in - Add Component → Audio → Audio Source)
```

### Step 4: Configure BabyBehavior Inspector

| Field | Default | Type | Adjustable |
|-------|---------|------|-----------|
| Current State | Normal | Enum | Yes |
| Hunger | 100 | 0-100 | Yes |
| Comfort | 100 | 0-100 | Yes |
| Temperature | 36.5 | 36-41 | Yes |
| thresholdNormal | 40 | 0-100 | Yes |
| feverThreshold | 37.5 | 36-41 | Yes |
| criticalThreshold | 15 | 0-100 | Yes |
| timeScale | 1.0 | float | Yes (untuk slow-mo) |

### Step 5: Configure BabyDisease Inspector

| Field | Default | Type | Notes |
|-------|---------|------|-------|
| diseaseChancePerUpdate | 0.001 | 0-1 | Probability per frame |
| useSymptomDatabase | true | bool | Use JSON database |
| Common Cold Duration | 120 | float | 2 menit |
| Common Cold Min/Max Severity | 30 / 60 | 0-100 | Range severity |
| Pneumonia Duration | 300 | float | 5 menit |
| Pneumonia Min/Max Severity | 60 / 100 | 0-100 | Range severity |
| Pneumonia Chance | 0.3 | 0-1 | Chance to progress |

---

## Part 4: ANIMATOR SETUP ⚠️ CRITICAL

### Step 1: Find Animator Controller
```
Unity Animator Controller: Project/Assets/[cari file .controller]
Atau buat baru: Right-click Assets → Create → Animator Controller
Nama: BabyAnimator
```

### Step 2: Setup State Machine
```
Window → Animation → Animator
Buat states:
1. Idle_Sleep (default state)
2. Restless_Moving
3. Crying (opsional)
4. Fever (opsional)

Parameters:
- isCrying (bool)
- isFever (bool)
- isWheeling (bool)
```

### Step 3: Add Transitions
```
Idle_Sleep → Restless_Moving (condition: none)
Idle_Sleep → Crying (condition: isCrying == true)
Idle_Sleep → Fever (condition: isFever == true)
[semua state] → Idle_Sleep (condition: [reset])

Timing: 0.1 - 0.5 detik untuk smooth transition
```

### Step 4: Assign Animator Controller
```
Select Baby GameObject
Animator component:
- Controller: [Drag .controller file ke field ini]
```

### Step 5: Setup SkinnedMeshRenderer Reference
```
Baby GameObject → Inspector
BabyAnimator component:
- Baby Face Renderer: [Find SkinnedMeshRenderer untuk head/face]
- Body Renderer: [Find SkinnedMeshRenderer untuk body]
- Chest Renderer: [Find SkinnedMeshRenderer untuk chest]
```

**How to find SkinnedMeshRenderer:**
```
1. Expand Baby GameObject dalam Hierarchy
2. Cari child objects dengan mesh
3. Select child object
4. Inspector: cari component "SkinnedMeshRenderer"
5. Drag SkinnedMeshRenderer component ke Baby → BabyAnimator fields
```

### Step 6: Find Blendshape Indices ⚠️ IMPORTANT
```
Select SkinnedMeshRenderer component
Inspector → Mesh: click mesh asset
Materials tab → cari Blendshapes atau Shape Keys

Blendshape indices (example, sesuaikan dengan model):
- Crying Blendshape Index: ? (cari index untuk crying face)
- Puled Blendshape Index: ? (sick/pucat face)
- Wheezing Blendshape Index: ? (sesak nafas face)
- Chest Cavity Blendshape Index: ? (dada cekung)

UPDATE BabyAnimator Inspector dengan nilai ini!
```

**How to check blendshape index:**
```
1. Open Blender dengan model .fbx
2. Lihat Shape Keys di Properties panel
3. Count index dari atas (0 = first, 1 = second, dst)
4. Catat di spreadsheet
5. Update di BabyAnimator Inspector
```

---

## Part 5: AUDIO SETUP

### Step 1: Import Audio Files
```
Project → Assets → Audio folder (create if not exist)
Drag audio files (.wav, .mp3) ke folder
Atau: Assets → Import New Asset
```

### Step 2: Create Audio Folder Structure
```
Assets/Audio/
├── Crying/
│   ├── cry_normal.wav
│   ├── cry_hungry.wav
│   └── cry_uncomfortable.wav
├── Disease/
│   ├── sneeze.wav
│   ├── cough.wav
│   ├── cough_phlegm.wav
│   └── wheezing.wav
```

### Step 3: Configure AudioSource Component
```
Select Baby GameObject
AudioSource component:
- Output: Master
- Volume: 0.8
- Spatial Blend: 0 (2D)
```

### Step 4: Assign Audio Clips to BabyAudioCue
```
Select Baby GameObject
BabyAudioCue component:

CRYING AUDIO:
- Normal Cry Clip: [Drag cry_normal.wav]
- Hungry Cry Clip: [Drag cry_hungry.wav]
- Uncomfortable Cry Clip: [Drag cry_uncomfortable.wav]

DISEASE AUDIO:
- Sneeze Clip: [Drag sneeze.wav]
- Cough Clip: [Drag cough.wav]
- Cough With Phlegm Clip: [Drag cough_phlegm.wav]
- Wheeze Clip: [Drag wheeze.wav]

PARAMETERS:
- Cry Volume: 0.8
- Disease Audio Volume: 0.6
- Pitch Variation: 0.1

TIMING:
- Cough Interval: 5 (detik)
- Sneeze Interval: 8 (detik)
- Wheeze Interval: 3 (detik)
```

---

## Part 6: PARTICLE SYSTEMS SETUP

### Step 1: Create Crying Tears VFX
```
Right-click in Hierarchy → Effects → Particle System
Rename: CryingTearsVFX
Parent ke: Baby GameObject
Position: (0, [kepala], 0)

Setup:
- Duration: 2-3 detik
- Loop: unchecked
- Color: Biru pucat (air mata)
- Size: Kecil (0.1-0.5)
- Speed: Turun ke bawah
```

### Step 2: Create Fever Sweat VFX
```
Right-click in Hierarchy → Effects → Particle System
Rename: FeverSweatVFX
Parent ke: Baby GameObject
Position: (0, [seluruh body], 0)

Setup:
- Duration: 2-3 detik
- Loop: unchecked
- Color: Putih/keabu-abuan
- Size: Sangat kecil (0.05-0.2)
- Speed: Floating
```

### Step 3: Assign ke BabyAnimator
```
Select Baby GameObject
BabyAnimator component:
- Crying Tears VFX: [Drag CryingTearsVFX particle system]
- Fever Sweat VFX: [Drag FeverSweatVFX particle system]
```

---

## Part 7: MATERIAL & VISUALS SETUP

### Step 1: Create Normal Skin Material
```
Right-click Assets → Create → Material
Nama: BabySkinNormal
Shader: Standard atau Universal Render Pipeline/Lit
Color: Kulit bayi normal (RGB: 255, 200, 180 atau sesuai model)
```

### Step 2: Create Pale Skin Material (Optional)
```
Duplicate BabySkinNormal → BabySkinPale
Color: Pucat (RGB: 243, 237, 237)
```

### Step 3: Assign ke BabyAnimator
```
Select Baby GameObject
BabyAnimator component:
- Baby Normal Material: [Drag BabySkinNormal]
- Pale Skin Color: (0.95, 0.93, 0.93, 1) [automatic]

Atau buat custom di BabyAnimator code jika perlu
```

---

## Part 8: DATABASE SETUP ✅ ALREADY DONE

### SymptomDatabaseLoader Configuration
```
GameManager GameObject → SymptomDatabaseLoader component:

Fields:
- Json File Name: "symptom_database.json"
- Load From Streaming Assets: true ✅
- Validate On Load: true ✅

File Location: Assets/StreamingAssets/symptom_database.json ✅
```

**Verify JSON is loaded:**
```
Play scene → check Console
Expected: "[SymptomDatabaseLoader] Database berhasil dimuat dari: ..."
```

---

## Part 9: DIAGNOSIS VALIDATOR SETUP

### DiagnosisValidator Configuration
```
GameManager GameObject → DiagnosisValidator component:

VALIDATION SETTINGS:
- False Positive Penalty: 0.1 (10%)
- Missed Symptom Penalty: 0.15 (15%)

ACCURACY THRESHOLDS:
- Perfect Threshold: 90
- Good Threshold: 70
- Fair Threshold: 50
- Poor Threshold: 20

REFERENCES:
- Baby Behavior: [Auto-find, bisa drag manual jika perlu]
- Baby Disease: [Auto-find, bisa drag manual jika perlu]
```

---

## Part 10: TEST INPUT SETUP (Optional)

### Create InputTester GameObject
```
Right-click in Hierarchy → Create Empty
Rename: InputTester
Add Component: Test_Input

Configure:
- Baby Behavior: [Drag Baby GameObject]
- Game Manager: [Drag GameManager GameObject]
```

### Test Keyboard Input
```
Play Scene
Tekan:
- F: Beri makan (hunger = 100)
- D: Ganti popok (comfort = 100)
- Space: Akhir hari (EndDay)

Check Console untuk logs
```

---

## 🔍 VERIFICATION CHECKLIST

### After Setup, Verify:

- [ ] GameManager Scene Initialized
  ```
  Console: "[GameManager] BabyBehavior found"
  ```

- [ ] Baby Components Loaded
  ```
  Console: No errors about missing components
  ```

- [ ] Database Loaded
  ```
  Console: "[SymptomDatabaseLoader] Database berhasil dimuat"
  ```

- [ ] Audio System Ready
  ```
  No warnings about missing AudioSource
  ```

- [ ] Animator Ready
  ```
  Animator Controller assigned
  States: Idle_Sleep, Restless_Moving, etc.
  ```

- [ ] Particle Systems Created
  ```
  CryingTearsVFX and FeverSweatVFX in Hierarchy
  ```

---

## ⚡ TROUBLESHOOTING

### Error: "BabyBehavior tidak ditemukan di scene!"
**Solution:**
1. Check Baby GameObject exists in scene
2. Assign to GameManager → Baby Behavior field
3. Or ensure it has BabyBehavior component

### Error: "SymptomDatabaseLoader tidak ditemukan"
**Solution:**
1. Create SymptomDatabaseLoader component on GameManager
2. Verify symptom_database.json exists in Assets/StreamingAssets/

### Error: "Animator component tidak ditemukan"
**Solution:**
1. Select Baby GameObject
2. Add Component → Animator
3. Assign Animator Controller (.controller file)

### Audio not playing
**Solution:**
1. Check AudioSource component exists
2. Verify audio clips assigned in BabyAudioCue
3. Check volume levels (cryVolume, diseaseAudioVolume)

### Blendshapes not working
**Solution:**
1. Find correct blendshape indices in model
2. Open model in Blender to check Shape Keys
3. Update BabyAnimator Inspector with correct indices
4. Verify SkinnedMeshRenderer assigned

---

## 📝 REFERENCE DOCUMENT CREATED

This setup guide provides complete instructions for Unity Editor configuration.

Keep this document for reference during development!

---

Generated: 19 May 2026
For: IMK-6-ParentingSimulation Project
