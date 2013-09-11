#!/bin/bash 

 

 

echo 1 > /proc/sys/net/ipv4/tcp_retries2 

 

 

BPC_HOME=/home/Bpc 

 

 

# init console 

export LANG="ru_RU.koi8r" 

export LC_CTYPE="ru_RU.UTF-8" 

cd $BPC_HOME 

 

 

if ! test -e Log; then 

	mkdir Log 

fi 

 

 

# init sound 

amixer sset Master 100 unmute > /dev/nul 

amixer sset PCM 100 unmute > /dev/nul 

amixer sset Line mute > /dev/nul 

 

 

exitCode=1 

 

 

while test "$exitCode" -eq "1"; do 

	# clear spool 

	/usr/bin/lprm 

 

 

	# start application 

	./Croc.Bpc.Application.exe 1>`date +Log/console_%Y%m%d_%H%M.log` 2>&1 

 

 

	exitCode=$? 

 

 

	# check need to restart driver 

	if test "$exitCode" -eq "4"; then 

		/etc/init.d/gs2mgr stop 

		/etc/init.d/gs2 stop 

		/etc/init.d/gs2 start 

		/etc/init.d/gs2mgr start 

		exitCode="1" 

	fi 

done 


 
 

if test "$exitCode" -eq "2"; then 

	reboot 

fi 

 

 

if test "$exitCode" -eq "3"; then 

	poweroff 

fi


