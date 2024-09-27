using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace InfinityPBR.Modules
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Audio Clip Combiner", menuName = "Game Modules/Create/Audio Clip Combiner", order = 99)]
    public class AudioClipCombiner : ScriptableObject
    {
        public string outputName;
        public List<AudioLayer> audioLayers = new List<AudioLayer>();
        [HideInInspector] public bool displayInspector	= false;

        private float rescaleFactor = 32767; //to convert float to Int16

        const int HEADER_SIZE = 44;

        [HideInInspector] public bool showFullEditor = false;
        [HideInInspector] public float longestClipLength = -1f;
        [HideInInspector] public float shortestClipLength = -1f;

        public string PathOutput => $"Assets/Audio Clip Exports/{name}/{outputName}";
        public string PathSample => $"Assets/Audio Clip Exports";
        public string PathExample => $"Assets/Audio Clip Exports/{name}/{outputName}/{outputName}_1.wav";

        [System.Serializable]
        public class AudioLayer
        {
	        [FormerlySerializedAs("clip")] public List<AudioClip> clips = new List<AudioClip>();
	        [HideInInspector] public bool expanded = false;
	        [HideInInspector] public int clipNumber = 0;
	        [HideInInspector] public string name = "No Clips Added";
	        public float volume = 1;
	        public float delay;
	        [HideInInspector] public Int16[] samples;
	        [HideInInspector] public Byte[] bytes;
	        [HideInInspector] public int sampleCount;
	        [HideInInspector] public int channelCount;
	        [HideInInspector] public int delayCount;
	        [HideInInspector] public int onClip = 0;
	        [HideInInspector] public float longestClipLength = -1f;
	        [HideInInspector] public float shortestClipLength = -1f;
	        [HideInInspector] public AudioClipCombiner parent;

	        public bool HasClips()
	        { 
		        if (clips.Count == 0) return false;
		        return clips.Count > 0 && clips.All(x => x != null);
	        }

	        public void GetSamples(int clipNumber, int mostChannels)
	        {
		        
		        samples =  parent.GetSamplesFromClip(clips[clipNumber], volume);
		        
		        delayCount = (int)(delay * clips[clipNumber].frequency * clips[clipNumber].channels);
		        //delayCount = (int)(delay * clips[clipNumber].frequency);

		        sampleCount = delayCount + samples.Length;
		        channelCount = clips[clipNumber].channels;
		        //Debug.Log($"From {clips[clipNumber].name} Samples: {samples.Length} & sampleCount {sampleCount}");
	        }

	        public void UpdateLengths()
	        {
		        longestClipLength = -1f;
		        shortestClipLength = -1f;
		        foreach (AudioClip audioClip in clips)
		        {
			        var length = audioClip.length;
			        longestClipLength = Mathf.Max(length, longestClipLength > 0 ? longestClipLength : length);
			        shortestClipLength = Mathf.Min(length, shortestClipLength > 0 ? shortestClipLength : length);
		        }
	        }

	        public void AddClip(AudioClip newClip)
	        {
		        if (newClip == null) return;
		        if (clips.Contains(newClip)) return;
	            
	            clips.Add(newClip);
	            
	            UpdateLengths();
	        }
        }
        
        public void UpdateLengths()
        {
	        shortestClipLength = ShortestClip();
	        longestClipLength = LongestClip();
        }

        public float ShortestClip()
        {
	        var shortest = -1f;
	        foreach (AudioLayer layer in audioLayers)
		        shortest = Mathf.Max(shortest, layer.shortestClipLength + layer.delay);

	        return shortest;
        }
        
        public float LongestClip()
        {
	        var longest = -1f;
	        foreach (AudioLayer layer in audioLayers)
		        longest = Mathf.Max(longest, layer.longestClipLength + layer.delay);

	        return longest;
        }

        public int TotalExports()
		{
			int totalExports = 1;												// Start at 1...
			for(int n = 0; n < audioLayers.Count; n++)
				totalExports 	*= audioLayers [n].clips.Count;						// Multiply by the number of clips in each layer
			return totalExports;
		}
		
	    public void ExportNow()
	    {
		    Validations();
			// Find total number of exports
			int totalExports = TotalExports();

			if (totalExports > 0) {
				float progressPercent	= 0.0f;
				int clipCount = 0;
#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Exporting Combined Audio Clips", "Clip " + clipCount + " of " + totalExports, progressPercent);
#endif
				string[] combinations;													// Start an array of all combinations
				combinations = new string[totalExports];								// Set the number of entries to the number of exports

				// Reset the onClip value for each layer
				for (int r = 0; r < audioLayers.Count; r++) {
					audioLayers [r].onClip = 0;
				}
					
				for (int l = 0; l < audioLayers.Count; l++) {							// For each layer...
					int exportsLeft = 1;												// Start at 1...
					for (int i = l; i < audioLayers.Count; i++) {							// For each layer left in the list (don't compute those we've already done)
						exportsLeft *= audioLayers [i].clips.Count;					// Find out how many exports are left if it were just those layers
					}

					int entriesPerValue = exportsLeft / audioLayers [l].clips.Count;	// Compute how many entires per value, if the total entries were exportsLeft
					int entryCount = 0;													// Set entryCount to 0

					for (int e = 0; e < combinations.Length; e++) {						// For all combinations
						if (l != 0)														// If this isn't the first layer
							combinations [e] = combinations [e] + ",";					// Append a "," to the String
						combinations [e] = combinations [e] + audioLayers [l].onClip;	// Append the "onClip" value to the string
						entryCount++;													// increase entryCount
						if (entryCount >= entriesPerValue) {							// if we've done all the entires for that "onClip" value...
							audioLayers [l].onClip++;									// increase onClip by 1
							entryCount = 0;												// Reset entryCount
							if (audioLayers [l].onClip >= audioLayers [l].clips.Count)	// if we've also run out of clips for this layer
								audioLayers [l].onClip = 0;								// Reset onClip count
						}
					}
				}

				int number = 0;															// for the file name
				// For each combination, save a .wav file with those clip numbers.
				foreach (var combination in combinations) {
					clipCount++;
					//progressPercent = clipCount / totalExports * 1.0f;
					progressPercent = clipCount / (float)totalExports;
					//Debug.Log ("progressPercent: " + progressPercent);
#if UNITY_EDITOR
					EditorUtility.DisplayProgressBar("Exporting Combined Audio Clips", "Clip " + clipCount + " of " + totalExports, progressPercent);
#endif
					string[] clipsAsString	= combination.Split ("," [0]);
					SaveClip (outputName, number, clipsAsString, audioLayers);
					number++;
				}
#if UNITY_EDITOR
				EditorUtility.ClearProgressBar();
#endif
			} 
			else 
			{
				Debug.Log ("Nothing To Export! (or maybe a layer is missing clips?)");
			}
	    }

	    private int MaxFrequency()
	    {
		    var maxFrequency = -1;
		    foreach (AudioLayer layer in audioLayers)
		    {
			    foreach (AudioClip clip in layer.clips)
			    {
				    maxFrequency = Mathf.Max(maxFrequency, clip.frequency);
			    }
		    }
		    //Debug.Log($"Max Frequency is {maxFrequency}");
		    return maxFrequency;
	    }

	    private int MaxChannels()
	    {
		    var maxChannels = -1;
		    foreach (AudioLayer layer in audioLayers)
		    {
			    foreach (AudioClip clip in layer.clips)
			    {
				    maxChannels = Mathf.Max(maxChannels, clip.channels);
			    }
		    }
		    //Debug.Log($"Max Channels is {maxChannels}");
		    return maxChannels;
	    }
	    
	    public void ExportOneRandom(string randomName)
	    {
		    Validations();

		    // Create an array to store a randomly selected clip from each layer
		    string[] randomClips = new string[audioLayers.Count];

		    // Select a random clip from each layer
		    for (int i = 0; i < audioLayers.Count; i++)
		    {
			    if (audioLayers[i].clips.Count > 0)
			    {
				    int randomIndex = UnityEngine.Random.Range(0, audioLayers[i].clips.Count);
				    randomClips[i] = randomIndex.ToString();
			    }
			    else
			    {
				    Debug.LogWarning($"AudioLayer {i} has no clips. Skipping...");
			    }
		    }

		    // Construct the output file path
		    string outputPath = Path.Combine("Assets", $"{randomName}.wav");

		    // Save a .wav file with the randomly selected clips
		    SaveClip(randomName, -1, randomClips, audioLayers, true);
	    }


		public bool SaveClip(string filename, int exportNumber, string[] clipsAsString, List<AudioLayer> audioLayers, bool isSample = false)
		{
			//Debug.Log ("Doing Export " + exportNumber);
			filename = $"{(filename.Length <= 0 ? "CombinedAudio" : filename)}{(exportNumber < 0 ? "" : "_" + exportNumber)}.wav";

			var filepath = isSample ? $"{PathSample}/{filename}" : $"{PathOutput}/{filename}";

			// Make sure directory exists if user is saving to sub dir.
			Directory.CreateDirectory(Path.GetDirectoryName(filepath));

			using (var fileStream = CreateEmpty(filepath))										// Create an empty file
			{
				int sampleCount = ConvertAndWrite(fileStream, clipsAsString, audioLayers);

				//Debug.Log($"sampleCount: {sampleCount}");
				//	 ClIP NUMBER CHANGE HERE
				WriteHeader(fileStream, sampleCount);
			}
#if UNITY_EDITOR
			AssetDatabase.ImportAsset(filepath);
#endif
			return true; // TODO: return false if there's a failure saving the file
		}

		private int ConvertAndWrite(FileStream fileStream, String[] clipsAsString, List<AudioLayer> audioLayers)
	    {
		    int mostSamples = 0;																// Set this to 0
		    int mostChannels = 1;
	        var dif = MaxFrequency();
			
	        for (int c = 0; c < audioLayers.Count; c++) {										// For each Layer
		        mostChannels = Mathf.Max(mostChannels, audioLayers[c].channelCount);
	        }
	        
	        for (int c = 0; c < audioLayers.Count; c++) {										// For each Layer
				int clipNumber = int.Parse(clipsAsString[c]);									// Get the clip number as an int
				audioLayers[c].GetSamples(clipNumber, mostChannels);											// Run this function from the class
				mostSamples = Mathf.Max(mostSamples, audioLayers[c].channelCount == 1 ? audioLayers[c].sampleCount * 2 : audioLayers[c].sampleCount);// * (mostChannels / audioLayers[c].channelCount));				// Set mostSamples to the greatest one
	        }
	        
	        //Debug.Log($"Most samples: {mostSamples}");
			
	        Int16[] finalSamples = new Int16[mostSamples];										// The exported clip will have the mostSamples
			float[] sampleValues = new float[mostSamples];	

	        for(int i = 0; i < mostSamples; i++)												// for each sample
	        {
	            float sampleValue = 0;															// Set variable for exported clip
	            int sampleCount = 0;															// Set variable

	            foreach (var audioLayer in audioLayers)											// For each layer....
	            {
					if (i > audioLayer.delayCount && i < audioLayer.sampleCount)					// if we are not in the delay range and we are under the samplecount for the clip
	                {
						// Add the value from this layer to the final (sampleValue)
	                    sampleValue += (audioLayer.samples[i - audioLayer.delayCount] / rescaleFactor);
	                    sampleCount++;
	                }
	            }
	            
				sampleValues [i] += sampleValue;
	        }


			float highSample = 0.0f;																			// Variable for the highest sample
			float lowSample = 0.0f;																				// Variable for the lowest sample
			for (int h = 0; h < mostSamples; h++) {																// For each sample...
				highSample = Mathf.Max (highSample, sampleValues [h]);											// Compute the highest sample
				lowSample = Mathf.Min (lowSample, sampleValues [h]);    										// Compute the lowest sample
			}
			float parameter = Mathf.InverseLerp(0.0f, Mathf.Max(highSample, lowSample * -1), 1.0f);				// Find the amount we need to multiply each sample by, based on the most extreme sample (high or low)

			for (int p = 0; p < mostSamples; p++) {																// For each sample...
				sampleValues [p] *= parameter;																	// Multiply the value by the parameter value															// Adjust the volume
			}

			for (int i2 = 0; i2 < mostSamples; i2++) {															// For each sample...
				finalSamples [i2] = (short)(sampleValues[i2] * rescaleFactor);									// Finalize the value
			}
			sampleValues = new float[0];																		// Clear this data

	        Byte[] bytesData = ConvertSamplesToBytes(finalSamples);
	        fileStream.Write(bytesData, 0, bytesData.Length);

	        return mostSamples;
	    }


	    private Byte[] ConvertSamplesToBytes(Int16[] samples)
	    {
		    Byte[] bytesData = new Byte[samples.Length * 2];
	        for (int i = 0; i < samples.Length; i++)
	        {
	            Byte[] byteArr = new Byte[2];
	            byteArr = BitConverter.GetBytes(samples[i]);
	            byteArr.CopyTo(bytesData, i * 2);
	        }
	        return bytesData;
	    }


	    private Int16[] GetSamplesFromClip(AudioClip clip, float volume = 1)
	    {
		    var scaleFrequency = MaxFrequency() / clip.frequency;
		    var scaleChannels = MaxChannels() / clip.channels;
		    var scaleValue = 1;
		    
		    // check if audio is mono and process it differently
		    if (clip.channels == 1)
		    {
			    scaleChannels = 1;
		    }
		    
		    if (scaleFrequency > 1) scaleValue = scaleFrequency;
		    if (scaleChannels > 1 && clip.channels > 1) scaleValue += scaleChannels;
		    
			var samples = new float[clip.samples * clip.channels];
			
			clip.GetData(samples, 0);

			var scaledLength = samples.Length * scaleValue;

			Int16[] intData = new Int16[scaledLength];

	        var scaledIndex = 0;
	        var scaledCounter = 0;
	        for (int i = 0; i < scaledLength; i++)
	        {
		        scaledCounter++;
	            intData[i] = (short)(samples[scaledIndex] * volume * rescaleFactor);

	            if (scaledCounter == scaleValue)
	            {
		            scaledCounter = 0;
		            scaledIndex++;
	            }
	        }
	        return intData;
	    }

	    

	    private void WriteHeader(FileStream fileStream, int sampleCount)
	    {
		   
	        //var frequency = maxFrequency; // clip.frequency
	        //var channelCount = clip.channels;
	        //var channelCount = MaxChannels();
	        
	        fileStream.Seek(0, SeekOrigin.Begin);

	        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
	        fileStream.Write(riff, 0, 4);

	        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
	        fileStream.Write(chunkSize, 0, 4);

	        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
	        fileStream.Write(wave, 0, 4);

	        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
	        fileStream.Write(fmt, 0, 4);

	        Byte[] subChunk1 = BitConverter.GetBytes(16);
	        fileStream.Write(subChunk1, 0, 4);

	        //UInt16 two = 2;
	        UInt16 one = 1;

	        Byte[] audioFormat = BitConverter.GetBytes(one);
	        fileStream.Write(audioFormat, 0, 2);

	        Byte[] numChannels = BitConverter.GetBytes(MaxChannels());
	        fileStream.Write(numChannels, 0, 2);

	        Byte[] sampleRate = BitConverter.GetBytes(MaxFrequency());
	        fileStream.Write(sampleRate, 0, 4);

	        //Debug.Log($"math: {MaxFrequency() * MaxChannels() * 2}, channelCount {MaxChannels()}");
	        Byte[] byteRate = BitConverter.GetBytes(MaxFrequency() * MaxChannels() * 2); // sampleRate * bytesPerSample*number of channels, ie 44100*2*2
	        fileStream.Write(byteRate, 0, 4);

	        UInt16 blockAlign = (ushort)(MaxChannels() * 2);
	        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

	        UInt16 bps = 16;
	        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
	        fileStream.Write(bitsPerSample, 0, 2);

	        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
	        fileStream.Write(datastring, 0, 4);

			//Byte[] subChunk2 = BitConverter.GetBytes(sampleCount * channelCount * 2);
			var multiplier = 44100 / MaxFrequency();
	        Byte[] subChunk2 = BitConverter.GetBytes(sampleCount * MaxChannels() * multiplier);
	        fileStream.Write(subChunk2, 0, 4);

	        //		fileStream.Close();
	    }

	    private FileStream CreateEmpty(string filepath)
	    {
	        var fileStream = new FileStream(filepath, FileMode.Create);
	        byte emptyByte = new byte();

	        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
	        {
	            fileStream.WriteByte(emptyByte);
	        }

	        return fileStream;
	    }

	    public void Validations()
	    {
		    if (String.IsNullOrWhiteSpace(outputName)) outputName = name;
		    outputName = outputName.Trim();
		    outputName = outputName.Replace(" ", "_");
		    foreach (AudioLayer audioLayer in audioLayers)
		    {
			    audioLayer.parent = this;
		    }
	    }
    }
}