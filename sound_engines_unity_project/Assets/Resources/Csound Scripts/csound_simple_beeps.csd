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
  ; Generate a new frequency between 300 and 1000 Hz whenever kTrig is 1
  kFreq randh 700, 10
  ; Generate the sine wave with the changing frequency
  aOut oscili 1.0, 300 + kFreq

  ; Output the sine wave to left and right channels
  outs aOut, aOut
endin

</CsInstruments>
<CsScore>
; Play Instrument 1 infinitely
i1 0 z
</CsScore>
</CsoundSynthesizer>
