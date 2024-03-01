<CsoundSynthesizer>
<CsOptions>
;-n -d -m0d 
;-odac
 ; -m0d
</CsOptions>
<CsInstruments>

sr = 44100  ; Sample rate
ksmps = 32  ; Control signal samples per audio signal sample
nchnls = 2  ; Number of audio channels
0dbfs	=	1 

instr 1  
  kFreq init 440

  kFreq chnget "freq"  

  aOut oscili 0.1, kFreq
  outs aOut
endin

</CsInstruments>
<CsScore>
; Play Instrument 1 infinitely
i1 0 z
</CsScore>
</CsoundSynthesizer>
