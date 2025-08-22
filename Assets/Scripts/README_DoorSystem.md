# Door Interaction System Setup Guide

This system provides a complete door interaction solution for Unity with smooth animations, input system integration, and UI prompts.

## Scripts Overview

### 1. **Door.cs** - Core Door Functionality
- Handles door opening/closing animations
- Configurable open/close angles and animation duration
- Audio support for open/close sounds
- Smooth animation with customizable curves

### 2. **Interactable.cs** - Base Interaction Detection
- Detects when player is within interaction range
- Manages UI prompts and visual feedback
- Configurable interaction range and settings

### 3. **DoorInteractable.cs** - Door-Specific Interaction
- Combines Door and Interactable functionality
- Optional auto-close feature
- Handles interaction events

### 4. **InteractionManager.cs** - Input System Integration
- Manages all interactable objects
- Integrates with Unity's new Input System
- Fallback to legacy input system
- Handles UI prompts and interaction logic

### 5. **InteractionPromptUI.cs** - UI Management
- Manages interaction prompt display
- Smooth fade animations
- Customizable styling

## Setup Instructions

### Step 1: Create a Door GameObject

1. **Create the door structure:**
   - Create an empty GameObject named "Door"
   - Add a 3D model or primitive (cube) as a child for the door visual
   - Position the door model so it rotates around the correct pivot point

2. **Add the Door script:**
   - Select the Door GameObject
   - Add Component → Scripts → Door
   - Configure settings:
     - **Open Angle**: How far the door opens (e.g., 90 degrees)
     - **Close Angle**: Starting/closed position (usually 0)
     - **Animation Duration**: How long the animation takes
     - **Animation Curve**: Easing for smooth movement

3. **Set up the pivot:**
   - Create an empty child GameObject at the door's hinge point
   - Assign this as the "Door Pivot" in the Door script
   - The door will rotate around this point

### Step 2: Add Interaction Components

1. **Add Interactable component:**
   - Select the Door GameObject
   - Add Component → Scripts → Interactable
   - Configure:
     - **Interaction Range**: How close the player needs to be
     - **Interaction Prompt**: Text to display (e.g., "Press E to open")

2. **Add DoorInteractable component:**
   - This automatically requires both Door and Interactable
   - Configure auto-close settings if desired

3. **Add Collider:**
   - Ensure the door has a Collider component
   - This is required for interaction detection

### Step 3: Set up the Player

1. **Add InteractionManager:**
   - Select your Player GameObject
   - Add Component → Scripts → InteractionManager
   - Configure:
     - **Max Interaction Distance**: Maximum range for finding interactables
     - **Interactable Layer**: Layer mask for interactable objects

2. **Ensure PlayerInput component:**
   - Your player should have a PlayerInput component
   - This should reference your Input Actions asset
   - The "Interact" action should be configured (you already have this!)

### Step 4: Create UI for Interaction Prompts

1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Set to "Screen Space - Overlay"

2. **Create Interaction Prompt:**
   - Right-click Canvas → UI → Panel
   - Add TextMeshPro Text as child
   - Add Image component for background
   - Add CanvasGroup component for fade effects

3. **Configure InteractionPromptUI:**
   - Add the InteractionPromptUI script to the panel
   - Assign the TextMeshPro and Image components
   - Customize colors and animation settings

4. **Connect to InteractionManager:**
   - Select your Player GameObject
   - In the InteractionManager component, assign:
     - **Interaction Prompt UI**: Your UI panel
     - **Prompt Text**: Your TextMeshPro component

### Step 5: Configure Input System

Your Input Actions asset already has an "Interact" action configured with a "Hold" interaction. This is perfect for door interactions!

## Usage

### Basic Door Setup
1. Create a door GameObject with the Door script
2. Add Interactable and DoorInteractable components
3. Ensure the player has InteractionManager
4. Press E (or your configured input) near the door to interact

### Customization Options

#### Door Animation
- Adjust open/close angles for different door types
- Modify animation duration for faster/slower movement
- Customize animation curves for different movement styles

#### Interaction Range
- Set different ranges for different interactables
- Use layers to organize interactable objects
- Adjust player detection distance

#### UI Styling
- Customize prompt text and colors
- Add fade animations
- Modify background and text appearance

#### Audio
- Add AudioSource to doors
- Assign open/close sound clips
- Audio will play automatically during interactions

## Advanced Features

### Auto-Close Doors
- Enable auto-close in DoorInteractable
- Set delay before automatic closing
- Useful for doors that should close behind the player

### Multiple Door Types
- Create different door prefabs with different settings
- Use inheritance to create specialized door types
- Share common functionality through base classes

### Event System
- Subscribe to door state changes
- Create custom behaviors based on door status
- Integrate with other game systems

## Troubleshooting

### Door Not Rotating
- Check that Door Pivot is assigned correctly
- Ensure the door model is a child of the pivot
- Verify rotation values are appropriate

### Interaction Not Working
- Check that all required components are added
- Verify the player has InteractionManager
- Ensure interactable objects have colliders
- Check layer masks and interaction ranges

### UI Not Showing
- Verify Canvas is set to Screen Space - Overlay
- Check that InteractionPromptUI is assigned in InteractionManager
- Ensure TextMeshPro components are properly assigned

### Input Not Responding
- Verify PlayerInput component is on the player
- Check that Input Actions asset is assigned
- Ensure "Interact" action is properly configured

## Example Scene Setup

1. **Door GameObject:**
   - Door script (open: 90°, close: 0°, duration: 1s)
   - Interactable component (range: 3m)
   - DoorInteractable component (auto-close: true, delay: 3s)
   - Box Collider
   - Door model as child

2. **Player GameObject:**
   - Movement script (your existing script)
   - InteractionManager
   - PlayerInput component
   - Camera as child

3. **UI Canvas:**
   - InteractionPromptUI script
   - TextMeshPro text
   - Background image
   - CanvasGroup

4. **Input Actions:**
   - Interact action with Hold interaction
   - Bound to E key or gamepad button

This system provides a solid foundation for interactive doors and can be easily extended for other interactable objects like chests, switches, or NPCs!
