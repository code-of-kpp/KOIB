export LC_ALL=C 
APPS_LIST=control/stop.lst 
PARTITIONS_LIST=control/partitions.lst 
CLEANUP_LIST=control/cleanup.lst 
CHMOD_LIST=control/chmod.lst 
DATE_STAMP=`date +%Y%m%d_%H%m%S` 
BACKUP_FILE=backup_$DATE_STAMP.tar 
OUT_PIPE=/tmp/out_$DATE_STAMP.pipe 
ERR_PIPE=/tmp/err_$DATE_STAMP.pipe 
OUT_FILE=install_$DATE_STAMP.log 
ERR_FILE=error_$DATE_STAMP.log 
if test -e $OUT_PIPE; then 
    rm $OUT_PIPE 
fi 
if test -e $ERR_PIPE; then 
    rm $ERR_PIPE 
fi 
mkfifo $OUT_PIPE $ERR_PIPE 
exec 3>&1 4>&1 
tee $OUT_FILE < $OUT_PIPE >&3 & 
pid_out=$! 
exec  1>$OUT_PIPE 
tee $ERR_FILE < $ERR_PIPE >&4 & 
pid_err=$! 
exec  2>$ERR_PIPE 
log() { 
 echo "`date` : $@ " 
} 
run_scripts() { 
 log "run $1 scripts" 
 if test -d $1/ ; then 
    for file in `ls -1 $1/*` 
    do 
       if test -f $file ; then 
        log "exec $file" 
        . $file 
    fi 
    done 
 fi 
} 
log "starting installation" 
if test -e $APPS_LIST; then 
    while read app; 
    do 
        app=`echo "$app" | tr -d '\n\r'` 
        if [ -n "${app// /}" ]; then 
            if ps -A | grep "$app" > /dev/null; then 
                log "killing $app" 
                pkill -9 "$app" 
            fi 
        fi 
    done < $APPS_LIST 
    GS2IsLoaded=`lsmod | grep GS2Driver` 
    if [ -n "$GS2IsLoaded" ]; then 
        log  "unloading GS2Driver" 
        rmmod GS2Driver 
    else 
        log  "GS2Driver has not been loaded" 
    fi     
fi 
if test -e $PARTITIONS_LIST; then 
    log "remount file systems" 
    while read x; 
    do 
        x=`echo "$x" | tr -d '\n\r'` 
        if [ -n "${x// /}" ]; then 
            ROOT_NAME=${x##*:} 
            mount -o remount,rw $ROOT_NAME 
        fi 
    done < $PARTITIONS_LIST 
fi 
if test -e $CLEANUP_LIST; then 
    log "removing old files" 
    while read file; 
    do 
        file=`echo "$file" | tr -d '\n\r'` 
        if [ -n "${file// /}" ]; then 
            if test -e $file; then 
                log "removing $file" 
                tar uvPf $BACKUP_FILE $file 
                if test -d $file; then 
                    rm -rf $file 
                else 
                    rm -f $file 
                fi 
            fi 
        fi 
    done < $CLEANUP_LIST 
    if test -e $BACKUP_FILE; then 
        gzip $BACKUP_FILE 
    fi 
fi 
run_scripts "pre" 
if test -e $PARTITIONS_LIST; then 
    while read x; 
    do 
        x=`echo "$x" | tr -d '\n\r'` 
        if [ -n "${x// /}" ]; then 
            PART_NAME=${x%%:*} 
            ROOT_NAME=${x##*:} 
            ARCH_NAME= 
            for suffix in .tgz .tar.gz .tar.bz2 
            do 
                if test -e $PART_NAME$suffix; then 
                    ARCH_NAME=$PART_NAME$suffix 
                    break 
                fi 
            done 
            if [ -n "$ARCH_NAME" ]; then 
                log "updating $PART_NAME ($ROOT_NAME) partition" 
                umask 0033 
                case "$ARCH_NAME" in 
                        *.bz2) tar -C $ROOT_NAME --no-same-permissions --no-same-owner -jxvf $ARCH_NAME ;; 
                        *)     tar -C $ROOT_NAME --no-same-permissions --no-same-owner -zxvf $ARCH_NAME ;; 
                esac 
            fi 
        fi 
    done < $PARTITIONS_LIST 
fi 
if test -e $CHMOD_LIST; then 
    log "chmoding files" 
    while read file; 
    do 
        file=`echo "$file" | tr -d '\n\r'` 
        if [ -n "${file// /}" ]; then 
            FILE_NAME=${file%%:*} 
            FILE_ATTR=${file##*:} 
            if test -e $FILE_NAME; then 
                log "chmod $FILE_ATTR $FILE_NAME" 
                chmod $FILE_ATTR $FILE_NAME 
            fi 
        fi 
    done < $CHMOD_LIST 
fi 
log "finalization" 
run_scripts "post" 
ldconfig 
log "all done" 
exec 1>&3 3>&- 2>&4 4>&- 
wait $pid_out 
wait $pid_err 
rm $OUT_PIPE $ERR_PIPE
