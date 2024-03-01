<CsoundSynthesizer>
<CsOptions>
-odac  
</CsOptions>
<CsInstruments>
sr = 44100
ksmps = 32
nchnls = 2
0dbfs  = 1

instr 1

idur = p3
iamp = p4
ifreq = p5
iQ = p6
ifc = p7
irevMix = p8
iGain = p9

asig vco2 iamp, ifreq ; sawtooth oscillator generates signal
afilter lowpass2 asig, ifc, iQ ; lowpass filter, cutoff 'ifc' and resonance 'iQ'
arev reverb afilter, 2 ; reverb, dry/wet 'irevMix'


out arev*iGain ; sum signals, apply gain 'iGain', and output

endin 

</CsInstruments>
<CsScore>
f1 0 16384 10 1
i 1 0 5 0.5 440 0.5 800 0.5 0.1 ; test the instrument
e
</CsScore>
</CsoundSynthesizer>
