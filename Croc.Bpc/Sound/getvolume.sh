MAX=$(amixer sget Master | grep "Limits" | awk '{print $5}') 
value=$(amixer sget Master | grep "Mono:" | awk '{print $3}') 
let value=$(awk -v val=$value -v max=$MAX 'BEGIN{ printf"%0.f\n", val*100/max}') 
echo $value
