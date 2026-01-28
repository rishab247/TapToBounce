#!/bin/bash
CPU=$(top -bn2 -d 1 | grep "Cpu(s)" | tail -n1 | awk -F',' '{for(i=1;i<=NF;i++) if($i ~ /id/) print $i}' | awk '{print 100 - $1"%"}' )
RAM=$(free -h | awk '/^Mem:/ {print $3 "/" $2}')
DISK=$(df -h / | awk 'NR==2 {print $5}')
echo -e "📊 **System Stats**\nCPU: $CPU\nRAM: $RAM\nDisk: $DISK"
