loss=$(ping -c 3 -i 0.2 -q -w 2 $2 | grep loss | sed -n -e 's/.* \([0-9]\+\)%.*/\1/p') 
if test "$loss" -eq "0"; then 
    echo 0 
    exit 
fi 
ifdown eth0 
sleep $1 
ifup eth0 
sleep $1 
loss=$(ping -c 3 -i 0.2 -q -w 2 $2 | grep loss | sed -n -e 's/.* \([0-9]\+\)%.*/\1/p') 
if test "$loss" -eq "0"; then 
    echo 1 
else 
    echo 2 
fi
