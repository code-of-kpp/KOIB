if test "$PLATFORM" == "Arm"; then 
    export GS_OPTIONS=-K10240 
fi 
args=$@ 
QUIET_START_ARGS="-quiet" 
BPC_HOME=/home/Bpc 
cd $BPC_HOME 
if ! test -e WorkData/Log; then 
    mkdir -p WorkData/Log 
fi 
echo -17 >/proc/$$/oom_adj 
rpcstatd_pid=$(ps -A | grep "rpc.statd" | awk '{print $1}') 
echo -17 >/proc/$rpcstatd_pid/oom_adj 
echo 1 > /proc/sys/net/ipv4/tcp_retries2 
RESTART_EXITCODE=2 
exitCode=$RESTART_EXITCODE 
while test "$exitCode" -eq "$RESTART_EXITCODE"; do 
    echo `date '+%d.%m.%Y %H:%M:%S -> started '` $args >> WorkData/Log/exitCode.log 
    export LC_ALL=C 
    lpq -a >> WorkData/Log/exitCode.log 
    cancel -a >> WorkData/Log/exitCode.log 
    lpq -a >> WorkData/Log/exitCode.log 
    export LANG="ru_RU.koi8r" 
    export LC_CTYPE="ru_RU.UTF-8" 
    ./Croc.Bpc.Application.exe $args 1>`date +WorkData/Log/console_%Y%m%d_%H%M%S.log` 2>&1 
    exitCode=$? 
    args="" 
    if ! test -e WorkData/Log; then 
        mkdir -p WorkData/Log 
    fi 


    case "$exitCode" in 
        1)    # exit 
            echo `date '+%d.%m.%Y %H:%M:%S -> exit'` >> WorkData/Log/exitCode.log 
            ;; 


        2)    # restart 
            echo `date '+%d.%m.%Y %H:%M:%S -> restart'` >> WorkData/Log/exitCode.log 
            ;; 


        3)    # reboot 
            echo `date '+%d.%m.%Y %H:%M:%S -> reboot'` >> WorkData/Log/exitCode.log 
            reboot 
            ;; 


        4)    # poweroff 
            echo `date '+%d.%m.%Y %H:%M:%S -> poweoff'` >> WorkData/Log/exitCode.log 
            poweroff 
            ;; 


        5)    # driver restart 
            echo `date '+%d.%m.%Y %H:%M:%S -> restart Gs2'` >> WorkData/Log/exitCode.log 
            /etc/init.d/gs2mgr stop 
            /etc/init.d/gs2mgr start 
            exitCode=$RESTART_EXITCODE 
            ;; 


        134) # mono crashed 
            echo `date '+%d.%m.%Y %H:%M:%S -> TRAP 134'` >> WorkData/Log/exitCode.log 
            exitCode=$RESTART_EXITCODE 
            args=$QUIET_START_ARGS 
            ;; 


        137) # kill -9 
            echo `date '+%d.%m.%Y %H:%M:%S -> kill -9'` >> WorkData/Log/exitCode.log 
            ;; 


          *)    # unexpected exit code 
            echo `date '+%d.%m.%Y %H:%M:%S -> unexpected exit'` $exitCode >> WorkData/Log/exitCode.log 
            exitCode=$RESTART_EXITCODE 
            args=$QUIET_START_ARGS 
    esac 
done
