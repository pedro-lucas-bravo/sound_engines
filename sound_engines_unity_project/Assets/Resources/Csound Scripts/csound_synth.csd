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

    kfreq chnget "freq"
    kQ chnget "Q"
    kfc chnget "fc"
    kGain chnget "gain"
    kRevTime chnget "revtime"

    asig vco2 0.5, kfreq ; sawtooth oscillator generates signal
    afilter moogladder asig, kfc, kQ ; lowpass filter, cutoff 'ifc' and resonance 'iQ'
    arev reverb afilter, 2 ; reverb, 2 seconds

    out arev*kGain ; sum signals, apply gain 'iGain', and output

/*     kfreq init 440
    kQ init 30

    kfreq chnget "freq"
    kQ chnget "q"
    kfc chnget "fc" 
    kGain chnget "gain"

    asig vco2 0.5, kfreq
    aOut lowres asig, kfc, kQ
    outs aOut*kGain */
endin

</CsInstruments>
<CsScore>
; Play Instrument 1 infinitely
i1 0 z
</CsScore>
</CsoundSynthesizer>
