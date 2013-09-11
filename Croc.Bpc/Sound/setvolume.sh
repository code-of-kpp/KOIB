MAX=$(amixer sget Master | grep "Limits" | awk '{print $5}') 
let value=($MAX*$1) 
let value=$(awk -v val=$value 'BEGIN{ printf"%0.f\n", val/100}') 
amixer sset Master $value > /dev/null
