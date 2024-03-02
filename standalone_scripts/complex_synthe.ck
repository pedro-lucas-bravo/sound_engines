// Synthesizer parameters
0.5 => float Q; // Resonance for the low-pass filter
1000 => float fc; // Cutoff frequency for the low-pass filter
1 => float revMix; // Reverb mix: 0.0 (dry) - 1.0 (wet)
//0.5 => float feedback; // Delay feedback
0.01 => float G; // Gain

// Oscillator
SawOsc osc => LPF filter => PRCRev reverb => Gain gain => dac;

// Set the parameters
fc => filter.freq;
Q => filter.Q;

// Configure reverb mix (mix of original signal and reverb signal)
revMix => reverb.mix;

// Main performance loop
while( true )
{
    // Play the note continuously
    1.0 => osc.gain;
    440 => osc.freq; // Set the frequency to any desired value

    // Apply the gain to the output
    G => gain.gain;

    // Synthesize the sound for this time slice
    100::ms => now;
}
