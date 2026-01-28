#!/bin/bash

TARGET="943567826"
DURATION=15
INTERVAL=30
ITERATIONS=30

echo "Starting monitoring for $DURATION minutes ($ITERATIONS iterations)..."

for ((i=1; i<=ITERATIONS; i++)); do
    # CPU: 100 - idle from mpstat
    CPU=$(mpstat 1 1 | awk '/Average/ {printf "%.1f", 100 - $12}')
    
    # RAM: Used/Total from free
    RAM_USED=$(free -h | grep Mem | awk '{print $3}')
    RAM_TOTAL=$(free -h | grep Mem | awk '{print $2}')
    
    # Disk: Usage of /
    DISK=$(df -h / | tail -1 | awk '{print $5}')
    
    # Format Message
    MESSAGE="📊 **System Stats**
CPU: ${CPU}%
RAM: ${RAM_USED}/${RAM_TOTAL}
Disk: ${DISK}"

    echo "Sending iteration $i/$ITERATIONS..."
    clawdbot message send --channel telegram --target "$TARGET" --message "$MESSAGE"
    
    if [ $i -lt $ITERATIONS ]; then
        sleep $INTERVAL
    fi
done

echo "Monitoring complete."
