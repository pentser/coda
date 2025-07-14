# HandLandmarkRecorder Guide

## Overview

The `HandLandmarkRecorder` is a Unity component that captures hand landmark data from MediaPipe's Holistic solution and saves it as JSON files. This system is designed to record hand movements and gestures for later playback or analysis.

## Components

### Data Structures

#### HandLandmark
```csharp
[System.Serializable]
public class HandLandmark
{
    public float x, y, z;
}
```
Represents a single 3D point in normalized coordinates (0-1 range).

#### HandLandmarkFrame
```csharp
[System.Serializable]
public class HandLandmarkFrame
{
    public HandLandmark[] landmarks;
    public float timestamp;
}
```
Represents a single frame of hand data with all landmarks and a timestamp.

#### HandLandmarkVideo
```csharp
[System.Serializable]
public class HandLandmarkVideo
{
    public HandLandmarkFrame[] frames;
    public string handType; // "left", "right", or "both"
    public int totalFrames;
}
```
Represents a complete recording session with multiple frames.

## Setup Instructions

### 1. Add HandLandmarkRecorder to Your Scene

1. Create an empty GameObject in your scene
2. Add the `HandLandmarkRecorder` component to it
3. Configure the settings in the inspector:

#### Recording Settings
- **HolisticGraph**: Reference to the HolisticGraph component (auto-detected if not assigned)
- **Record Left Hand**: Enable recording of left hand landmarks
- **Record Right Hand**: Enable recording of right hand landmarks
- **Auto Save On Quit**: Automatically save data when the application quits
- **Save On Key Press**: Enable saving when pressing the save key
- **Save Key**: Key to press for manual save (default: S)

#### File Settings
- **File Name**: Name of the output JSON file (default: "video_hand_to_json.json")
- **Folder Name**: Folder to save JSON files (default: "json_hand")

### 2. Ensure HolisticGraph is Running

The HandLandmarkRecorder requires a working HolisticGraph component that provides hand landmark data. Make sure:
- HolisticGraph is properly initialized
- Hand tracking is enabled
- The system is receiving hand landmark data

## How It Works

### 1. Initialization Process

```csharp
void Start()
{
    // Find HolisticGraph if not assigned
    if (holisticGraph == null)
    {
        holisticGraph = FindObjectOfType<HolisticGraph>();
    }
    
    // Subscribe to events after initialization
    StartCoroutine(SubscribeToEventsWhenReady());
    
    // Create output folder
    string folderPath = Path.Combine(Application.dataPath, folderName);
    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }
}
```

### 2. Event Subscription

The recorder subscribes to MediaPipe hand landmark events:

```csharp
// Left hand events
holisticGraph.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;

// Right hand events  
holisticGraph.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
```

### 3. Data Recording

When hand landmarks are detected, the system:

1. **Captures Landmarks**: Converts MediaPipe landmarks to custom format
2. **Adds Timestamp**: Records the time since recording started
3. **Stores Frame**: Adds the frame to the appropriate hand's frame list

```csharp
void OnLeftHandLandmarksOutput(object sender, OutputEventArgs<NormalizedLandmarkList> e)
{
    if (e.value == null || !isRecording) return;

    var frame = new HandLandmarkFrame();
    frame.landmarks = e.value.Landmark.Select(l => new HandLandmark 
    { 
        x = l.X, 
        y = l.Y, 
        z = l.Z 
    }).ToArray();
    frame.timestamp = Time.time - startTime;
    
    leftHandFrames.Add(frame);
}
```

### 4. Saving Process

The system saves three types of JSON files:

#### Left Hand Data (`video_hand_to_json_left.json`)
```json
{
    "frames": [
        {
            "landmarks": [
                {"x": 0.5, "y": 0.5, "z": 0.0},
                // ... 21 landmarks for left hand
            ],
            "timestamp": 0.0
        }
    ],
    "handType": "left",
    "totalFrames": 100
}
```

#### Right Hand Data (`video_hand_to_json_right.json`)
```json
{
    "frames": [
        {
            "landmarks": [
                {"x": 0.5, "y": 0.5, "z": 0.0},
                // ... 21 landmarks for right hand
            ],
            "timestamp": 0.0
        }
    ],
    "handType": "right",
    "totalFrames": 100
}
```

#### Combined Data (`video_hand_to_json.json`)
```json
{
    "frames": [
        // All frames from both hands combined
    ],
    "handType": "both",
    "totalFrames": 200
}
```

## Usage Instructions

### Automatic Recording

1. **Start the Scene**: The recorder automatically starts recording when the scene begins
2. **Perform Hand Movements**: Move your hands in front of the camera
3. **Save Data**: Press the save key (default: S) or quit the application

### Manual Control

```csharp
// Get the recorder component
HandLandmarkRecorder recorder = FindObjectOfType<HandLandmarkRecorder>();

// Start recording manually
recorder.StartRecording();

// Stop recording
recorder.StopRecording();

// Save data manually
recorder.SaveToJson();

// Clear recorded data
recorder.ClearRecordedData();

// Get frame counts
int leftFrames = recorder.GetLeftHandFrameCount();
int rightFrames = recorder.GetRightHandFrameCount();
```

### Key Controls

- **S Key**: Save current recording to JSON files
- **P Key**: Alternative save key
- **Application Quit**: Auto-save if enabled

## Output Files

### File Locations

All JSON files are saved to: `Assets/json_hand/`

### Generated Files

1. **`video_hand_to_json.json`** - Combined data from both hands
2. **`video_hand_to_json_left.json`** - Left hand data only
3. **`video_hand_to_json_right.json`** - Right hand data only

### File Structure

Each JSON file contains:
- **frames**: Array of hand landmark frames
- **handType**: Type of hand data ("left", "right", or "both")
- **totalFrames**: Total number of recorded frames

### Landmark Structure

Each frame contains 21 landmarks per hand (MediaPipe hand model):
- **Landmarks 0-4**: Thumb
- **Landmarks 5-8**: Index finger
- **Landmarks 9-12**: Middle finger
- **Landmarks 13-16**: Ring finger
- **Landmarks 17-20**: Pinky finger

## Troubleshooting

### Common Issues

#### 1. No JSON Files Generated
- **Check**: Ensure HolisticGraph is running and detecting hands
- **Solution**: Verify hand tracking is enabled in HolisticGraph settings

#### 2. Empty JSON Files
- **Check**: Look for "No data to save" messages in console
- **Solution**: Ensure hands are visible to the camera during recording

#### 3. Missing Folder
- **Check**: Verify the `json_hand` folder exists in Assets
- **Solution**: The recorder should create the folder automatically

#### 4. Recording Not Starting
- **Check**: Look for "HolisticGraph not found" errors
- **Solution**: Ensure HolisticGraph component is in the scene

### Debug Information

Enable debug logging to see:
- Recording start/stop messages
- Frame count updates (every 30 frames)
- Save operation details
- Error messages

## Integration with Animation System

The generated JSON files can be used with the JSON Animation Importer:

1. **Load JSON Files**: Use the JSONAnimationImporter to load the recorded data
2. **Play Animations**: Replay the recorded hand movements on VRM avatars
3. **Custom Animations**: Use the data for custom animation sequences

## Performance Considerations

### Memory Usage
- Each frame stores 21 landmarks per hand
- Long recordings can consume significant memory
- Consider clearing data periodically for long sessions

### File Size
- Typical recording: ~1KB per second at 30fps
- 1-minute recording: ~60KB per hand
- Use compression for large datasets

### Optimization Tips
- Disable recording when not needed
- Clear data periodically for long sessions
- Use appropriate save intervals

## Advanced Usage

### Custom File Names
```csharp
recorder.fileName = "my_custom_recording.json";
```

### Custom Save Locations
```csharp
recorder.folderName = "custom_folder";
```

### Selective Recording
```csharp
// Record only left hand
recorder.recordLeftHand = true;
recorder.recordRightHand = false;

// Record only right hand
recorder.recordLeftHand = false;
recorder.recordRightHand = true;
```

### Real-time Monitoring
```csharp
void Update()
{
    HandLandmarkRecorder recorder = FindObjectOfType<HandLandmarkRecorder>();
    if (recorder != null)
    {
        int leftFrames = recorder.GetLeftHandFrameCount();
        int rightFrames = recorder.GetRightHandFrameCount();
        Debug.Log($"Recording: {leftFrames} left, {rightFrames} right frames");
    }
}
```

## File Format Specification

### JSON Schema
```json
{
    "frames": [
        {
            "landmarks": [
                {
                    "x": 0.5,
                    "y": 0.5,
                    "z": 0.0
                }
            ],
            "timestamp": 0.0
        }
    ],
    "handType": "left|right|both",
    "totalFrames": 100
}
```

### Coordinate System
- **X**: Horizontal position (0 = left, 1 = right)
- **Y**: Vertical position (0 = bottom, 1 = top)
- **Z**: Depth position (0 = near, 1 = far)
- **Timestamp**: Seconds since recording started

### MediaPipe Hand Landmarks
The system uses MediaPipe's 21-point hand model:
- **0**: Wrist
- **1-4**: Thumb (CMC, MCP, IP, tip)
- **5-8**: Index finger (MCP, PIP, DIP, tip)
- **9-12**: Middle finger (MCP, PIP, DIP, tip)
- **13-16**: Ring finger (MCP, PIP, DIP, tip)
- **17-20**: Pinky finger (MCP, PIP, DIP, tip)

## Conclusion

The HandLandmarkRecorder provides a complete solution for capturing hand movement data in Unity. By following this guide, you can:

1. Set up hand landmark recording
2. Capture hand movements and gestures
3. Generate JSON files for animation playback
4. Integrate with the VRM animation system

The recorded data can be used for gesture recognition, animation playback, or analysis of hand movements in VR/AR applications. 