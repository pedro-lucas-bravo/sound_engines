<CsoundSynthesizer>
<CsOptions>
; Select audio/midi flags here according to platform
-odac    ;;;realtime audio out
;-iadc    ;;;uncomment -iadc if realtime audio input is needed too
; For Non-realtime ouput leave only the line below:
; -o oscils.wav -W ;;; for file output any platform
</CsOptions>
<CsInstruments>

sr = 44100  ; Sample rate
ksmps = 32  ; Control signal samples per audio signal sample
nchnls = 1  ; Number of audio channels
0dbfs	=	1 

instr 1
  seed 0
  iFreq random 220, 2000
  aOut oscili 1.0, iFreq
  outs aOut
endin

</CsInstruments>
<CsScore>
; Play Instrument 1 infinitely
i1 0 z
</CsScore>
</CsoundSynthesizer>