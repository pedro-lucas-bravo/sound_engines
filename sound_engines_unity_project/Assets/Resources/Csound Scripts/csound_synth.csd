<CsoundSynthesizer>
<CsOptions>
;-odac
 ; -m0d
</CsOptions>
<CsInstruments>

;sr = 44100  ; Sample rate
ksmps = 32  ; Control signal samples per audio signal sample
nchnls = 1  ; Number of audio channels
0dbfs	=	1

instr 1
    kfreq init 440 ; initial frequency
    kQ init 0.5 ; initial resonance
    kfc init 1000 ; initial cutoff frequency
    kGain init 0.1 ; initial gain
    kRevTime init 2 ; initial reverb time

    iAttack = p4
    iDecay = p5
    iSustain = p6
    iRelease = p7

    kfreq chnget "freq"
    kQ chnget "Q"
    kfc chnget "fc"
    kGain chnget "gain"
    kRevTime chnget "revtime"

    asig vco2 1.0, kfreq ; sawtooth oscillator generates signal
    afilter lowres asig, kfc, kQ ; lowpass filter, cutoff 'ifc' and resonance 'iQ'; looks lile lowres is more efficient than moogladder
    arev reverb afilter, 2 ; reverb, 2 seconds

    iAmp init 1.0 ; initial amplitude

    ; Steps through segments while a gate from midikey2 in the score file is set to turnon 
    kEnv linsegr 0, iAttack, iAmp, iDecay, iSustain, -1, iSustain, iRelease, 0   ; -1 means hold until turnoff

    out arev*kEnv*kGain ; sum signals, apply gain 'iGain', and output
endin

</CsInstruments>
<CsScore>
; Play Instrument 1 infinitely
;i1 0 z
</CsScore>
</CsoundSynthesizer>
