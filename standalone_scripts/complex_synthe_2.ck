// Define global audio components
SawOsc s => LPF filter => JCRev reverb => ADSR e => Gain amp => dac;

// Set initial parameters
440.0 => float freq;     // frequency
0.5 => float Q;          // resonance
1000.0 => float fc;      // cutoff frequency
0.1 => float G;       // gain
2.0 => float revMix;    // reverb time

// Envelope parameters (placeholders for this example)
0.1 => float attack;
0.2 => float decay;
0.7 => float sustain;
0.3 => float release;

// Set processing chain parameters
freq => s.freq;
fc => filter.freq;
Q => filter.Q;
revMix => reverb.mix;
G => amp.gain;

e.set( 10::ms, 8::ms, .5, 500::ms );

// infinite time-loop
while( true )
{
    // choose freq
    Math.random2( 32, 96 ) => Std.mtof => s.freq;

    // key on: begin ATTACK
    // (note: ATTACK automatically transitions to DECAY;
    //        DECAY automatically transitions to SUSTAIN)
    e.keyOn();
    // advance time by 500 ms
    // (note: this is the duration from the
    //        beginning of ATTACK to the end of SUSTAIN)
    500::ms => now;
    // key off; start RELEASE
    e.keyOff();
    // allow the RELEASE to ramp down to 0
    e.releaseTime() => now;

    // advance time by 300 ms (duration until the next sound)
    300::ms => now;
}
