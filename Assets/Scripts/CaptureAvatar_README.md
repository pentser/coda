# CaptureAvatar Script - Avatar Movement Recording

This script allows you to record avatar movement, hand gestures, finger landmarks, and facial expressions, then save the data as a JSON file.

## Setup Instructions

1. **Add the Script to a GameObject:**
   - Create an empty GameObject in your scene or use an existing one
   - Add the `CaptureAvatar` component to the GameObject
   - Make sure your scene has a GameObject with the "Solution" tag that contains the `HolisticSolution` component

2. **Configure Recording Settings (Optional):**
   - **Capture Rate**: Set the frames per second for recording (default: 30 FPS)
   - **File Name**: Set the base name for saved files (default: "avatar_capture")
   - **Show Recording Status**: Toggle the on-screen recording status display

## How to Use

### Recording Avatar Movement

1. **Start Recording**: Press the `R` key to begin recording avatar movement
   - The status display will show "RECORDING..." in red
   - Frame count will update in real-time

2. **Stop Recording**: Press the `R` key again to stop recording
   - The status will show the number of captured frames and duration

3. **Save Recording**: Press the `S` key to save the captured data to a JSON file
   - Files are saved in the `Assets/captureAvatar/` folder with a timestamp
   - Format: `avatar_capture_YYYYMMDD_HHMMSS.json`
   - The folder is automatically created if it doesn't exist

### What Gets Recorded

The script captures the following data every frame:

#### Pose Data
- Neck, chest, and hip rotations
- Hip position
- Shoulder, elbow, and hand positions (both sides)
- Upper and lower leg rotations (both sides)

#### Hand Data (Both Hands)
- Wrist rotation
- All finger joint rotations (thumb, index, middle, ring, pinky)
- Individual finger segments (PIP, DIP, TIP joints)

#### Face Data
- Mouth open amount
- Eye iris positions (left and right)
- Eye open amount (left and right)

## JSON File Structure

The saved JSON file contains:
```json
{
  "captureDate": "2024-01-15 14:30:25",
  "captureStartTime": 123.45,
  "totalDuration": 5.67,
  "frameCount": 170,
  "frames": [
    {
      "pose": { /* pose data */ },
      "rightHand": { /* right hand data */ },
      "leftHand": { /* left hand data */ },
      "face": { /* face data */ }
    }
    // ... more frames
  ]
}
```

## Troubleshooting

- **"Could not find HolisticSolution" Error**: Make sure your scene has a GameObject tagged as "Solution" with the HolisticSolution component
- **No Data Captured**: Ensure the motion capture system is running and detecting movement
- **File Save Errors**: Check that Unity has write permissions to the Assets/captureAvatar folder

## Tips

- Higher capture rates (60+ FPS) will create larger files but smoother playback
- Test with short recordings first to verify everything is working
- The on-screen status helps monitor recording progress
- Files are automatically timestamped to prevent overwriting 