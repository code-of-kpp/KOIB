speexdec --force-wb $1 - | aplay --buffer-size 300000000 -t wav -f S16_LE -c 1 -r 22050 -
