rm error* 
chmod +x install.sh 
./install.sh 
size=$(du error* | awk '{print $1}') 
case "$size" in 
    0) 
        echo "SETUP OK" 
        reboot 
        ;; 


    *) 
        echo "SETUP FAILED!" 
        ;; 
esac
