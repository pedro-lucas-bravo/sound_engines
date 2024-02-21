SinOsc s => dac;
Math.random2(440,2000) => int freq;
freq => s.freq;
0.01 => s.gain;

while(true) 
{
    //{freqId} => s.freq;
    1::samp => now;
}