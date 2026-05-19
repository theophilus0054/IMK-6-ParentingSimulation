# 📦 ASSETS REQUIREMENTS & CHECKLIST

## OVERVIEW
Status: **85% Ready** - Only missing audio, animation clips, and blendshape configuration

---

## ✅ EXISTING ASSETS (Already in Project)

### Scripts
```
✅ Assets/Scripts/Baby/
   ├── BabyBehavior.cs
   ├── BabyDisease.cs
   ├── BabyAnimator.cs
   ├── BabyAudioCue.cs
   ├── SymptomDatabase.cs
   └── SymptomDatabaseLoader.cs

✅ Assets/Scripts/Game/
   ├── GameManager.cs
   ├── SaveLoadManager.cs
   ├── Test_Input.cs
   ├── DiagnosisValidator.cs
   └── DiagnosisExample.cs
```

### Data
```
✅ Assets/StreamingAssets/
   └── symptom_database.json (7 symptoms, 2 diseases)

✅ Assets/Object/
   └── Ch31_nonPBR@T-Pose.fbx (3D baby model)
```

### 3D Models
```
✅ Assets/Object/
   ├── box bayi.fbx
   ├── buku.blend1
   ├── kamar.fbx
   ├── laser-thermometer/ (folder)
   ├── mangkok-makan-bayi.fbx
   ├── Oximeter.fbx
   ├── RuanganKopong(DZIKRI).fbx
   └── Ch31_nonPBR@T-Pose.fbx (MAIN baby model)

✅ Assets/Animated Hands/
   └── Models/ (hand animation assets)
```

### Materials & Textures
```
✅ Assets/Materials/
├── Ruangan/ (room materials)
└── Scenes/ (scene materials)

✅ Assets/Texture/
   └── (various textures)
```

---

## ❌ MISSING ASSETS (REQUIRED)

### 1. AUDIO FILES (CRITICAL) ⭐

**Crying Audio (3 files):**
```
Location: Create Assets/Audio/Crying/

File 1: baby_cry_normal.wav
- Description: Normal baby cry
- Duration: 2-4 seconds
- Format: .wav or .mp3
- Volume: Normal
- Status: ❌ MISSING

File 2: baby_cry_hungry.wav  
- Description: Hungry cry (more intense)
- Duration: 2-4 seconds
- Format: .wav or .mp3
- Volume: Louder than normal
- Status: ❌ MISSING

File 3: baby_cry_uncomfortable.wav
- Description: Uncomfortable cry (higher pitch)
- Duration: 2-4 seconds
- Format: .wav or .mp3
- Volume: Normal
- Status: ❌ MISSING
```

**Disease Audio (4 files):**
```
Location: Create Assets/Audio/Disease/

File 1: baby_sneeze.wav
- Description: Baby sneeze (pilek symptom)
- Duration: 1-2 seconds
- Format: .wav or .mp3
- Status: ❌ MISSING

File 2: baby_cough.wav
- Description: Light dry cough
- Duration: 1-3 seconds
- Format: .wav or .mp3
- Status: ❌ MISSING

File 3: baby_cough_phlegm.wav
- Description: Wet cough with phlegm
- Duration: 2-3 seconds
- Format: .wav or .mp3
- Volume: More guttural
- Status: ❌ MISSING

File 4: baby_wheezing.wav
- Description: Wheezing/struggling breath
- Duration: 2-4 seconds
- Format: .wav or .mp3
- Status: ❌ MISSING
```

**Total Audio Files Needed: 7**
**Estimated Size: 500 KB - 5 MB**
**Sourcing Options:**
- [ ] Record yourself
- [ ] Download from free SFX sites (freesound.org, zapsplat.com)
- [ ] Use Unity Asset Store
- [ ] AI generation (text-to-speech tools)

### 2. ANIMATOR SETUP (CRITICAL) ⭐

**Animator Controller:**
```
Location: Create Assets/Animations/BabyAnimator.controller

Contents:
- Base Layer state machine with states:
  ✅ Entry (default)
  ✅ Idle_Sleep (default state)
  ✅ Restless_Moving
  ✅ Crying (optional)
  ✅ Fever (optional)

Parameters:
  ✅ isCrying (bool)
  ✅ isFever (bool)
  ✅ isWheeling (bool)

Transitions:
  ✅ Any → Idle_Sleep
  ✅ Idle_Sleep → Restless_Moving (no condition)
  ✅ Idle_Sleep → Crying (isCrying == true)
  ✅ Idle_Sleep → Fever (isFever == true)

Status: ❌ MISSING (needs manual setup in editor)
```

**Animation Clips:**
```
Location: Create Assets/Animations/

File 1: Idle_Sleep.anim
- Description: Baby sleeping/resting idle loop
- Duration: 2-4 seconds (looping)
- Type: Humanoid rig animation
- Status: ❌ MISSING (can use default)

File 2: Restless_Moving.anim
- Description: Baby moving around restlessly
- Duration: 2-4 seconds (looping)
- Type: Humanoid rig animation
- Status: ❌ MISSING (can use default)

Optional:
- Crying.anim (specific crying pose)
- Fever.anim (fever animation)
```

**How to Create (if not available):**
```
Option 1: Use Mixamo animations
1. Go to mixamo.com
2. Search "baby" or use humanoid
3. Download animations
4. Import to Unity
5. Retarget to Ch31_nonPBR model

Option 2: Manual animation in Blender
1. Open Ch31_nonPBR@T-Pose.fbx
2. Create simple keyframe animations
3. Export as .fbx
4. Import to Unity

Option 3: Use default Unity animations
1. Animator will use built-in humanoid animations
2. Less immersive but functional
```

### 3. MODEL BLENDSHAPES (CRITICAL) ⭐

**Blendshape Configuration:**
```
Model: Ch31_nonPBR@T-Pose.fbx

Required Blendshapes (check in model):
- [ ] Crying face (mouth open, sad expression)
- [ ] Puled/sick face (grimace, uncomfortable)
- [ ] Wheezing face (mouth open struggling)
- [ ] Chest indentation (cavity deformation)

How to Find Blendshapes:
1. Import model to Blender
2. Open Shapekeys tab
3. List all available shapekeys
4. Create missing ones if needed:
   - Crying: Sad mouth + closed eyes
   - Puled: Grimace + tense face
   - Wheezing: Open mouth + strain
   - Chest: Inward deformation of ribcage

Status: ❌ UNKNOWN (need to check model file)
```

**Blendshape Index Mapping:**
```
After identifying blendshapes, map indices:

In BabyAnimator.cs (already in code):
- cryingBlendshapeIndex = ? (find actual index)
- puledBlendshapeIndex = ? (find actual index)
- wheezingBlendshapeIndex = ? (find actual index)
- chestCavityBlendshapeIndex = ? (find actual index)

Update in Inspector or hardcode in script
```

### 4. MATERIALS (MEDIUM PRIORITY)

**Skin Material:**
```
Location: Create Assets/Materials/

File: BabySkinNormal.mat
- Shader: Standard or URP/Lit
- Base Color: RGB(255, 200, 180) - warm skin tone
- Metallic: 0
- Smoothness: 0.8
- Status: ❌ MISSING (can use default white)

File: BabySkinPale.mat (optional)
- Shader: Standard or URP/Lit
- Base Color: RGB(243, 237, 237) - pale/sick
- Metallic: 0
- Smoothness: 0.8
- Status: ❌ MISSING (code will handle)
```

---

## ⚠️ OPTIONAL ASSETS (Nice to Have)

### 1. Particle Effects (Prefabs)
```
These are created in-editor, but can be saved as prefabs:

File 1: CryingTearsVFX.prefab
- Particle system for water droplets
- Color: Light blue
- Direction: Downward
- Lifetime: 2-3 seconds

File 2: FeverSweatVFX.prefab
- Particle system for sweat droplets
- Color: White/translucent
- Direction: Floating
- Lifetime: 2-3 seconds

Status: ⭐ Can create in editor (30 min)
```

### 2. UI Assets (For Future)
```
These are for diagnosis UI (not yet implemented):

File 1: UI_DiagnosisPanel.prefab
File 2: UI_SymptomButton.prefab
File 3: UI_ResultPanel.prefab

Status: ⭐⭐ For next phase
```

### 3. Fonts
```
Already available: TextMesh Pro built-in
Optional: Custom fonts for better look
Status: Not critical
```

---

## 📥 IMPORT INSTRUCTIONS

### Audio Import Settings:
```
Select audio file → Inspector
Audio Importer:
- Audio Format: WAV or MP3
- Sample Rate: 48000 Hz recommended
- Load In Background: Checked
- Streaming: Unchecked
- Compression Format: Vorbis (high quality)
```

### Model Import Settings:
```
Ch31_nonPBR@T-Pose.fbx → Inspector
Model:
- Animation Type: Humanoid
- Avatar Definition: Create From This Model
- Muscle Setup: (auto)
```

### Animation Clip Import:
```
Animation files → Inspector
Rig:
- Animation Type: Humanoid
- Muscle Setup: Copy From Avatar
```

---

## 🎯 QUICK START OPTIONS

### Option A: Minimal Setup (15 min)
```
Skip: Audio, Custom animations, Blendshapes
Use: Default materials, built-in animations
Result: Game works, minimal visuals
```

### Option B: Basic Setup (2-3 hours)
```
Include: Find basic audio online
Include: Use simple animations (Mixamo)
Skip: Custom blendshapes
Result: Functional game with audio
```

### Option C: Full Polish (5-6 hours)
```
Include: Professional audio
Include: Custom animations
Include: Blendshape implementation
Include: Material setup
Result: Polished, immersive experience
```

---

## 📋 SETUP CHECKLIST (Copy This)

### Phase 1: Audio Assets
```
[ ] Create Assets/Audio/ folder
[ ] Create Assets/Audio/Crying/ subfolder
[ ] Create Assets/Audio/Disease/ subfolder
[ ] Import baby_cry_normal.wav
[ ] Import baby_cry_hungry.wav
[ ] Import baby_cry_uncomfortable.wav
[ ] Import baby_sneeze.wav
[ ] Import baby_cough.wav
[ ] Import baby_cough_phlegm.wav
[ ] Import baby_wheezing.wav
```

### Phase 2: Animation
```
[ ] Create Assets/Animations/ folder
[ ] Create BabyAnimator.controller in editor
[ ] Add states to state machine
[ ] Add parameters (isCrying, isFever, isWheeling)
[ ] Create transitions
[ ] Find or import animation clips
[ ] Assign to controller
```

### Phase 3: Blendshapes
```
[ ] Open Ch31_nonPBR@T-Pose.fbx in Blender
[ ] List all existing blendshapes
[ ] Create missing blendshapes if needed
[ ] Export model with updated blendshapes
[ ] Re-import to Unity
[ ] Update BabyAnimator script with indices
```

### Phase 4: Materials
```
[ ] Create BabySkinNormal.mat
[ ] Create BabySkinPale.mat (optional)
[ ] Assign to SkinnedMeshRenderer
[ ] Test color changes
```

### Phase 5: Particle Effects
```
[ ] Create CryingTearsVFX particle system
[ ] Create FeverSweatVFX particle system
[ ] Save as prefabs
[ ] Test in scene
```

---

## 💡 TROUBLESHOOTING ASSETS

### Issue: Audio not playing
```
Check:
1. Audio format (WAV/MP3)
2. Sample rate (44100+ Hz)
3. AudioSource component enabled
4. Volume not muted
5. Clip assigned to field
```

### Issue: Blendshapes not working
```
Check:
1. Correct blendshape indices
2. SkinnedMeshRenderer assigned
3. Index within mesh range
4. Values 0-100 range
```

### Issue: Animations not playing
```
Check:
1. Animator Controller assigned
2. States created correctly
3. Parameters named exactly (case-sensitive)
4. Transitions set up
5. Default state set to Idle_Sleep
```

---

## 📞 RESOURCES

### Free Audio Sources:
```
- freesound.org (CC licensed)
- zapsplat.com (free)
- pixabay.com/sounds
- freepd.com
- BBC Sound Library
```

### Free Animation Sources:
```
- mixamo.com (free Adobe account)
- animgif.com
- CGTrader
- Sketchfab
```

### Blender Help:
```
- blender.org/manual (shapekeys)
- YouTube: "Blender Shape Keys Tutorial"
- Blender Discord community
```

---

## 📊 ASSET SUMMARY TABLE

| Asset Type | Quantity | Status | Priority | Est. Time |
|-----------|----------|--------|----------|-----------|
| Audio Clips | 7 | ❌ | ⭐⭐⭐ | 1-2 hrs |
| Animations | 2-4 | ❌ | ⭐⭐⭐ | 1-3 hrs |
| Blendshapes | 4 | ❌ | ⭐⭐⭐ | 1-2 hrs |
| Materials | 2 | ❌ | ⭐⭐ | 30 min |
| Particle FX | 2 | ❌ | ⭐ | 30 min |
| 3D Models | 1 | ✅ | - | - |
| Scripts | 9 | ✅ | - | - |
| Database | 1 | ✅ | - | - |

**Total Missing Time: 5-8 hours** (to full polish)

---

Last Updated: 19 May 2026
For: IMK-6-ParentingSimulation
