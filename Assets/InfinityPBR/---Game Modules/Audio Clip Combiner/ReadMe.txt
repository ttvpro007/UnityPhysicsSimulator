AUDIO CLIP COMBINER

** WATCH THE VIDEO HERE: https://youtu.be/YVuMQ7I05GA

This module will export combined AudioClip .wav files given any number of input values. It uses Scriptable Objects to create and store the data.

1. Right-click in a project folder you'd like to create an Audio Clip Combiner file, and chooose "Create/Game Modules/Create/Audio Clip Combiner"
2. Feel free to change the "Output Name". This is part of exported .wav files, example "OutputName_1.wav".
3. Add as many Audio Layers as you'd like. You must have at least two layers, with at least one AudioClip each, to export.
4. When you have finished populating your object, click the green Export button to export all of the combined .wav files.


Each Layer can have as many AudioClips as you'd like. An exported .wav file will include one AudioClip from each layer, and every possible combination of clips will be exported. This means that if you have three Audio Layers, each with 5 Audio Clips, the system will export 125 .wav files (5 x 5 x 5).

Each Audio Layer has settings for volume, and delay. Add a 0.5 delay, and this Audio Layer will not play until 0.5 seconds into the combined clip, allowing you to space out sounds as you'd like.

When exporting, existing files will be overwritten. While this is convenient for quickly testing out settings, you can change the Output Name to avoid this.

