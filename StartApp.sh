#!/bin/bash

# Get LAN IP address (preferring 192.168.*.*)
IP=$(ifconfig | grep 'inet ' | grep '192\.168\.' | awk '{ print $2 }' | head -n 1)

if [ -z "$IP" ]; then
  echo " Could not determine LAN IP address (192.168.x.x)"
  exit 1
fi

URL="http://$IP:5000"

echo " Starting the app at $URL"

# Start the app in background
./ResponseApp --urls "http://0.0.0.0:5000" &

# Wait briefly to allow app startup
sleep 2

# Open in default browser
open "$URL"