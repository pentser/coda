# Avatar Playback System

The Avatar Playback System allows you to load and replay captured avatar movement data that was recorded using the CaptureAvatar script.

## Setup

1. **Add the Script to Your Scene**:
   - Create an empty GameObject in your scene
   - Add the `AvatarPlayback` component to it
   - Make sure your scene has a HolisticSolution component with the 'Solution' tag

2. **Configure Settings** (optional):
   - `Auto Load File`: Automatically loads the default file on startup
   - `Default File Name`: The JSON file to load automatically (must be in Assets/captureAvatar/)
   - `Show Playback Status`: Shows playback information on screen
   - `Playback Speed`: Default speed multiplier for playback (1.0 = normal speed)

## How to Use

### Loading Capture Data

**Method 1: Automatic Loading**
- If `Auto Load File` is enabled, the script will automatically load the specified default file on startup
- The default file should be located in `Assets/captureAvatar/`

**Method 2: Manual Loading**
- Press **L** key to open a file browser
- Navigate to your capture files (usually in `Assets/captureAvatar/`)
- Select the JSON file you want to load

### Playback Controls

| Key | Action |
|-----|--------|
| **P** | Play/Pause playback |
| **O** | Stop playback |
| **L** | Load capture file |
| **+** / **-** | Increase/Decrease playback speed |

### Playback Process

1. **Load a capture file** using the L key or automatic loading
2. **Press P** to start playback - the avatar will begin performing the captured movements
3. **Press P again** to pause/resume playback
4. **Press O** to stop playback completely
5. Use **+/-** keys to control playback speed (0.1x to 3.0x)

## What It Does

The AvatarPlayback script:

1. **Loads JSON capture data** created by the CaptureAvatar script
2. **Parses the frame data** including:
   - Body pose (neck, chest, hips positions and rotations)
   - Hand movements (detailed finger joint rotations)
   - Facial expressions (mouth open, eye movements)
3. **Applies the movements** to the avatar in real-time by feeding data into the HolisticSolution system
4. **Maintains timing** using the original capture timestamps
5. **Provides visual feedback** showing playback progress and controls

## Technical Details

### Data Format
The script loads JSON files with the following structure:
```json
{
  "captureDate": "2025-07-17 14:58:33",
  "totalDuration": 9.54,
  "frameCount": 220,
  "frames": [
    {
      "pose": { ... },
      "rightHand": { ... },
      "leftHand": { ... },
      "face": { ... }
    }
  ]
}
```

### Integration
- Works seamlessly with the existing HolisticSolution system
- Uses the same avatar animation pipeline as live motion capture
- Respects bone settings and configuration options
- Compatible with VRM avatar models

## Troubleshooting

**"Could not find HolisticSolution in scene"**
- Ensure your scene has a GameObject with HolisticSolution component
- Make sure the GameObject has the 'Solution' tag

**"No capture data loaded"**
- Check that the JSON file exists in the specified location
- Use the L key to manually browse and load a file
- Verify the JSON file was created by the CaptureAvatar script

**Playback appears choppy or fast**
- Adjust the playback speed using +/- keys
- Check that your target framerate matches the capture framerate
- Ensure the avatar model is properly set up and visible

**Avatar doesn't move during playback**
- Verify that bone settings are enabled for the body parts you want to animate
- Check that the avatar model is visible and active
- Ensure the HolisticSolution is not paused

## File Locations

- **Capture files**: `Assets/captureAvatar/`
- **Script**: `Assets/Scripts/AvatarPlayback.cs`
- **Dependencies**: CaptureAvatar.cs (for data structures)

## Integration with Existing Workflow

1. **Record movements** using CaptureAvatar (R key to record, S key to save)
2. **Load and replay** using AvatarPlayback (L key to load, P key to play)
3. **Fine-tune timing** using speed controls (+/- keys)
4. **Use for demos, testing, or content creation**

The system is designed to work alongside the existing motion capture pipeline, allowing you to switch between live capture and playback seamlessly. 