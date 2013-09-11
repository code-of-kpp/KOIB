FIRST=yes    # debian/rules sets this to 'yes' when creating hwclockfirst.sh 
HWCLOCKPARS= 
hwclocksh() 
{ 
    [ ! -x /sbin/hwclock ] && return 0 
    . /etc/default/rcS 
    . /lib/lsb/init-functions 
    verbose_log_action_msg() { [ "$VERBOSE" = no ] || log_action_msg "$@"; } 
    [ "$GMT" = "-u" ] && UTC="yes" 
    case "$UTC" in 
       no|"")    GMT="--localtime" 
        UTC="" 
        if [ "X$FIRST" = "Xyes" ] && [ ! -r /etc/localtime ]; then 
            if [ -z "$TZ" ]; then 
            log_action_msg "System clock was not updated at this time" 
            return 1 
            fi 
        fi 
        ;; 
       yes)    GMT="--utc" 
        UTC="--utc" 
        ;; 
       *)    log_action_msg "Unknown UTC setting: \"$UTC\""; return 1 ;; 
    esac 
    case "$BADYEAR" in 
       no|"")    BADYEAR="" ;; 
       yes)    BADYEAR="--badyear" ;; 
       *)    log_action_msg "unknown BADYEAR setting: \"$BADYEAR\""; return 1 ;; 
    esac 
    case "$1" in 
    start) 
        if [ -w /etc ] && [ ! -L /etc/adjtime ] && [ ! -e /etc/adjtime ]; then 
        echo "0.0 0 0.0" > /etc/adjtime 
        fi 
        NOADJ="--noadjfile" 
        if [ "$FIRST" != yes ]; then 
        : 
        fi 
        if [ "$HWCLOCKACCESS" != no ]; then 
        log_action_msg "Setting the system clock" 
        if /sbin/hwclock --hctosys $GMT $HWCLOCKPARS $BADYEAR $NOADJ; then 
            log_action_msg "System Clock set to: `date $UTC`" 
        else 
            log_warning_msg "Unable to set System Clock to: `date $UTC`" 
        fi 
        else 
        verbose_log_action_msg "Not setting System Clock" 
        fi 
        ;; 
    stop|restart|reload|force-reload) 
        if [ "$HWCLOCKACCESS" != no ]; then 
        if [ -f /var/log/shutdown ] 
        then 
            cd /var/log && { 
                chgrp adm shutdown || : 
                savelog -q -p -c 5 shutdown  
            } 
        fi 
        log_action_msg "Saving the system clock" 
        echo "`date` hwclockfirst: Saving the system clock" >> /var/log/shutdown 
        if [ "$GMT" = "-u" ]; then 
            GMT="--utc" 
        fi 
        if /sbin/hwclock --systohc $GMT $HWCLOCKPARS $BADYEAR $NOADJ; then 
            log_action_msg "Hardware Clock updated to `date`" 
            echo "`date` hwclockfirst: Hardware Clock updated to `date`" >> /var/log/shutdown 
        fi 
        else 
        verbose_log_action_msg "Not saving System Clock" 
        fi 
        ;; 
    show) 
        if [ "$HWCLOCKACCESS" != no ]; then 
        /sbin/hwclock --show $GMT $HWCLOCKPARS $BADYEAR 
        fi 
        ;; 
    *) 
        log_success_msg "Usage: hwclock.sh {start|stop|reload|force-reload|show}" 
        log_success_msg "       start sets kernel (system) clock from hardware (RTC) clock" 
        log_success_msg "       stop and reload set hardware (RTC) clock from kernel (system) clock" 
        return 1 
        ;; 
    esac 
} 
hwclocksh "$@"
