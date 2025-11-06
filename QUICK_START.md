# 🚀 Quick Start - Advanced XR Chemistry Lab

## ✅ What's New

1. **Multi-Beaker System** - Grab different beakers with same gesture
2. **Fixed Size** - Beaker ALWAYS stays (5, 5, 5)
3. **Bottom UI Panel** - All buttons in one line
4. **Smart Selection** - Auto-selects nearest beaker

---

## 📦 Setup (5 Minutes)

### Step 1: Add Script
1. Create GameObject named "XR_Lab_Manager"
2. Add Component → `AdvancedXRChemistryLab`

### Step 2: Configure
```
Inspector:
├── Beakers: Add your beaker GameObjects
├── Water Particles Prefab: Assign prefab
└── FIXED_BEAKER_SCALE: (5, 5, 5)
```

### Step 3: Test
1. Press Play
2. Show hand to camera
3. Make fist (✊) to grab
4. Open hand (✋) to tilt
5. Pinch (👌) to refill

---

## 🎮 Controls

| Gesture | Action |
|---------|--------|
| ✊ Closed Fist | Grab & move nearest beaker |
| ✋ Open Hand | Tilt beaker to pour |
| 👌 Pinch | Refill beaker |

### UI Buttons (Bottom)
- **⟲ RESET ALL** - Reset all beakers
- **💧 REFILL ALL** - Refill all beakers
- **🗑 CLEAR ALL** - Empty all beakers
- **❓ HELP** - Show controls
- **👁 HIDE UI** - Toggle UI

---

## 🔒 Size Lock Guarantee

```csharp
// Size is ALWAYS (5, 5, 5)
// Enforced in:
- Start()
- Update() (every frame)
- LateUpdate() (final check)
- Before/after every gesture
```

**Result:** Size NEVER changes! ✅

---

## 📊 Multi-Beaker Usage

1. Add multiple beakers to scene
2. Add all to Inspector list
3. Make grab gesture (✊)
4. System grabs nearest beaker
5. Move hand to another beaker
6. Release and grab again
7. Now controlling different beaker

---

## 🐛 Troubleshooting

**Size changing?**
- Check FIXED_BEAKER_SCALE = (5, 5, 5)
- Click RESET ALL button

**Can't grab?**
- Increase Grab Detection Radius to 2.0
- Ensure beakers in Inspector list

**UI not showing?**
- Press HIDE UI button (might be hidden)
- Check script is enabled

---

## 📁 Files Created

1. `AdvancedXRChemistryLab.cs` - Main script
2. `ADVANCED_XR_LAB_GUIDE.md` - Full guide
3. `MIGRATION_GUIDE.md` - Migration steps
4. `QUICK_START.md` - This file

---

## ✅ Success Checklist

- [ ] Script added to scene
- [ ] Beakers in Inspector list
- [ ] Water particles assigned
- [ ] Tested grab gesture
- [ ] Verified size stays (5, 5, 5)
- [ ] Tested all UI buttons
- [ ] Multiple beakers working

---

**Done! Your advanced XR lab is ready! 🎉**
