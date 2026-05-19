# 📋 IMPLEMENTATION CHECKLIST - Parenting Simulation

## ✅ Overview Status

Semua scripts sudah **implementable** tapi memerlukan setup di Unity Editor.

---

## 📊 Script Analysis

### ✅ BABY SYSTEM (Assets/Scripts/Baby/)

| Script | Status | Complexity | Dependencies | Notes |
|--------|--------|-----------|--------------|-------|
| **BabyBehavior.cs** | ✅ Ready | Medium | GameManager | Main baby state/status manager |
| **BabyDisease.cs** | ✅ Ready | High | SymptomDatabaseLoader | Disease system + gejala |
| **BabyAnimator.cs** | ⚠️ Partial | Medium | Animator, SkinnedMeshRenderer, Particle | Perlu rig & animations |
| **BabyAudioCue.cs** | ⚠️ Partial | Low | AudioSource, AudioClips | Perlu audio files |
| **SymptomDatabase.cs** | ✅ Ready | Low | None | Database scriptable object |
| **SymptomDatabaseLoader.cs** | ✅ Ready | Low | SymptomDatabase | JSON loader |

### ✅ GAME SYSTEM (Assets/Scripts/Game/)

| Script | Status | Complexity | Dependencies | Notes |
|--------|--------|-----------|--------------|-------|
| **GameManager.cs** | ✅ Ready | High | BabyBehavior, SaveLoadManager | Game state/progression |
| **SaveLoadManager.cs** | ✅ Ready | Medium | GameManager, BabyBehavior, BabyDisease | JSON save/load |
| **Test_Input.cs** | ✅ Ready | Low | BabyBehavior, GameManager | Keyboard testing |
| **DiagnosisValidator.cs** | ✅ Ready | Medium | BabyDisease, BabyBehavior | Diagnosis validation |
| **DiagnosisExample.cs** | ✅ Ready | Low | DiagnosisValidator | Example usage |

---

## 🔧 SETUP CHECKLIST - STEP BY STEP

### STEP 1: Scene Setup
- [ ] Create new scene atau setup di existing scene
- [ ] Create empty GameObject: **"GameManager"**
  - [ ] Add component: **GameManager.cs**
  - [ ] Add component: **SaveLoadManager.cs**
  - [ ] Add component: **SymptomDatabaseLoader.cs**
  - [ ] Add component: **DiagnosisValidator.cs**
  - [ ] Make it **DontDestroyOnLoad** (opsional)

### STEP 2: Baby GameObject Setup
- [ ] Create new GameObject: **"Baby"**
- [ ] Add components:
  - [ ] **BabyBehavior.cs** (Main)
  - [ ] **BabyDisease.cs** (Disease system)
  - [ ] **BabyAnimator.cs** (Visuals)
  - [ ] **BabyAudioCue.cs** (Audio)
  - [ ] **Animator** component (dari Unity)
  - [ ] **AudioSource** component (dari Unity)
  - [ ] **SkinnedMeshRenderer** (dari 3D model)

### STEP 3: 3D Model Setup (⚠️ CRITICAL)
- [ ] Import baby 3D model (CH31_nonPBR@T-Pose.fbx atau model lain)
- [ ] Setup model di Baby GameObject:
  - [ ] Add **Animator** component
  - [ ] Assign Animator Controller dengan state machine:
    - [ ] **Idle_Sleep** animation clip
    - [ ] **Restless_Moving** animation clip
    - [ ] Parameters: isCrying, isFever, isWheeling (bool)
  - [ ] Setup **SkinnedMeshRenderer** reference di BabyAnimator
  - [ ] Find blendshape indices untuk:
    - [ ] Crying face (index ?)
    - [ ] Puled/sick face (index ?)
    - [ ] Wheezing face (index ?)
    - [ ] Chest indentation (index ?)

### STEP 4: Visual Effects Setup
- [ ] Create Particle Systems untuk:
  - [ ] **CryingTearsVFX** - Air mata ketika menangis
  - [ ] **FeverSweatVFX** - Keringat saat demam
- [ ] Assign ke BabyAnimator fields di Inspector

### STEP 5: Audio Setup
- [ ] Import audio clips untuk:
  - **Crying Audio:**
    - [ ] Normal cry sound
    - [ ] Hungry cry sound
    - [ ] Uncomfortable cry sound
  - **Disease Audio:**
    - [ ] Sneeze sound
    - [ ] Cough sound
    - [ ] Wet cough sound
    - [ ] Wheezing sound
- [ ] Assign ke BabyAudioCue fields di Inspector
- [ ] Adjust volume levels (cryVolume, diseaseAudioVolume)

### STEP 6: Material Setup
- [ ] Create material untuk baby skin normal
- [ ] Material harus mendukung color change untuk pale skin effect
- [ ] Assign ke BabyAnimator.babyNormalMaterial di Inspector

### STEP 7: GameManager Setup - Inspector Fields
```
BABY SYSTEM
- Baby Behavior: [Drag Baby GameObject]
- Neonatal Phase Days: 7
- Toddler Phase Days: 7

UI MANAGER (future)
- UI Manager: [To be assigned later]
```

### STEP 8: SymptomDatabaseLoader Setup
- [ ] SymptomDatabaseLoader harus auto-load JSON dari:
  - **Path:** Assets/StreamingAssets/symptom_database.json ✅ (sudah ada)
  - **Atau:** Application.persistentDataPath
- [ ] Verify JSON valid di console

### STEP 9: DiagnosisValidator Setup
- [ ] Assign di Inspector atau auto-find akan handle
- [ ] Configure thresholds (opsional):
  - perfectThreshold: 90
  - goodThreshold: 70
  - fairThreshold: 50
  - poorThreshold: 20

### STEP 10: Testing Input Setup
- [ ] Attach **Test_Input.cs** ke GameObject atau UI button
- [ ] Test keyboard input:
  - [ ] **F** = Beri makan
  - [ ] **D** = Ganti popok
  - [ ] **Space** = Akhir hari

---

## ⚠️ CRITICAL REQUIREMENTS

### Audio Files REQUIRED:
```
✅ dapat ditemukan di Assets/TextMesh Pro/Samples or project
❌ atau dibuat/download sendiri:
  - baby_cry_normal.wav
  - baby_cry_hungry.wav
  - baby_cry_uncomfortable.wav
  - baby_sneeze.wav
  - baby_cough.wav
  - baby_cough_phlegm.wav
  - baby_wheezing.wav
```

### Animation Clips REQUIRED:
```
❌ Harus dibuat/import:
  - Idle_Sleep (atau name sesuai Animator Controller)
  - Restless_Moving (untuk lapar/uncomfortable)
```

### 3D Model Blendshapes REQUIRED:
```
Model harus memiliki blendshapes:
1. Crying expression (index ?)
2. Puled/sick expression (index ?)
3. Wheezing expression (index ?)
4. Chest cavity indentation (index ?)

Atau setup di Blender terlebih dahulu
```

---

## 📋 CONFIGURATION EXAMPLES

### BabyBehavior Inspector
```
Current State: Normal
Baby Status Parameters:
- Hunger: 100
- Comfort: 100
- Temperature: 36.5
Thresholds:
- thresholdNormal: 40
- feverThreshold: 37.5
- criticalThreshold: 15
Decay Multipliers:
- timeScale: 1.0
```

### BabyDisease Inspector
```
Disease Parameters:
- diseaseChancePerUpdate: 0.001
Common Cold Properties:
- commonColdDuration: 120 (2 menit)
- commonColdMinSeverity: 30
- commonColdMaxSeverity: 60
Pneumonia Properties:
- pneumoniaDuration: 300 (5 menit)
- pneumoniaMinSeverity: 60
- pneumoniaMaxSeverity: 100
- pneumoniaChance: 0.3
Database Settings:
- useSymptomDatabase: true
```

### BabyAnimator Inspector
```
Cryingテア VFX: [Particle System]
Fever Sweat VFX: [Particle System]
Baby Face Renderer: [SkinnedMeshRenderer]
- Crying Blendshape Index: [?]
- Puled Blendshape Index: [?]
- Wheezing Blendshape Index: [?]
Body Renderer: [SkinnedMeshRenderer]
Chest Renderer: [SkinnedMeshRenderer]
- Chest Cavity Blendshape Index: 0
Baby Normal Material: [Material]
Pale Skin Color: (0.95, 0.93, 0.93, 1)
```

### BabyAudioCue Inspector
```
Crying Audio:
- Normal Cry Clip: [Audio]
- Hungry Cry Clip: [Audio]
- Uncomfortable Cry Clip: [Audio]
Disease Audio:
- Sneeze Clip: [Audio]
- Cough Clip: [Audio]
- Cough With Phlegm Clip: [Audio]
- Wheezing Clip: [Audio]
Audio Parameters:
- Cry Volume: 0.8
- Disease Audio Volume: 0.6
- Pitch Variation: 0.1
Timing:
- Cough Interval: 5
- Sneeze Interval: 8
- Wheezing Interval: 3
```

---

## ✅ IMPLEMENTABLE STATUS

### Immediately Ready (No Asset Required):
- ✅ GameManager.cs - Manage game flow
- ✅ SaveLoadManager.cs - Save/load game
- ✅ BabyBehavior.cs - Baby stats
- ✅ BabyDisease.cs - Disease system
- ✅ SymptomDatabase.cs - Database
- ✅ SymptomDatabaseLoader.cs - JSON loader
- ✅ DiagnosisValidator.cs - Diagnosis validation
- ✅ Test_Input.cs - Testing

### Needs Assets (Audio/Animation/Models):
- ⚠️ BabyAnimator.cs - Needs animations + blendshapes
- ⚠️ BabyAudioCue.cs - Needs audio clips

---

## 🔍 DEPENDENCY DIAGRAM

```
GameManager (Singleton)
├── SaveLoadManager (Singleton)
├── BabyBehavior
│   ├── BabyAnimator (needs Animator, SkinnedMeshRenderer, Particle)
│   ├── BabyAudioCue (needs AudioSource, AudioClips)
│   └── BabyDisease
│       ├── SymptomDatabaseLoader (Singleton)
│       └── SymptomDatabase
└── DiagnosisValidator
    ├── BabyDisease
    └── BabyBehavior

SymptomDatabaseLoader (Singleton)
└── Loads: Assets/StreamingAssets/symptom_database.json ✅
```

---

## 🚀 QUICK START (MINIMAL SETUP)

Untuk testing cepat tanpa audio/animation:

1. **Create GameManager GameObject**
   - Add: GameManager, SaveLoadManager, SymptomDatabaseLoader, DiagnosisValidator

2. **Create Baby GameObject**
   - Add: BabyBehavior, BabyDisease
   - Add: Animator (minimal setup)
   - Add: AudioSource (akan diabaikan jika kosong)

3. **Assign References in Inspector**
   - GameManager → Baby Behavior: drag Baby GameObject
   - BabyBehavior → BabyDisease: auto-find saat runtime

4. **Start Game**
   - Script akan auto-find missing components
   - Will fallback gracefully jika audio/animations kosong

---

## ⚡ DEBUGGING TIPS

Jika ada error, check console untuk:
- `[GameManager] BabyBehavior tidak ditemukan di scene!`
  → Solution: Assign atau ensure Baby GameObject ada

- `[BabyDisease] SymptomDatabaseLoader tidak ditemukan`
  → Solution: Create GameObject dengan SymptomDatabaseLoader

- `[SymptomDatabaseLoader] File JSON tidak ditemukan`
  → Solution: Check Assets/StreamingAssets/symptom_database.json exists

- `[BabyAnimator] BabyAnimator component tidak ditemukan!`
  → Solution: Atach Animator component ke Baby GameObject

- `[BabyAudioCue] AudioSource tidak ditemukan!`
  → Solution: Attach AudioSource component ke Baby GameObject

---

## 📝 NEXT STEPS

1. ✅ Create GameManager hierarchy
2. ✅ Create Baby GameObject dengan basic components
3. ⚠️ **Import/Create Audio Files** (blocking)
4. ⚠️ **Setup Animator Controller** (blocking)
5. ⚠️ **Find/Create Blendshapes** dalam model 3D (blocking)
6. ✅ Create Particle Systems
7. ✅ Test dengan Test_Input.cs
8. Create UI Manager untuk diagnosis interface (future)

---

## 📦 Assets Checklist

### Existing ✅
- ✅ symptom_database.json (StreamingAssets)
- ✅ Ch31_nonPBR@T-Pose.fbx (3D model)

### Missing ❌
- ❌ Audio files (7x) - Custom needed
- ❌ Animation clips - Custom needed
- ❌ Particle system prefabs - Create in editor
- ❌ UI Canvas & buttons - Create later

---

Generated: 19 May 2026
Last Updated: Implementation Checklist Complete
