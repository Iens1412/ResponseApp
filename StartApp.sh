#!/bin/bash

# Get local IP address (assumes en0 or fallback to first inet)
IP=$(ipconfig getifaddr en0 2>/dev/null || ifconfig | grep 'inet ' | grep -v 127.0.0.1 | awk '{ print $2 }' | head -n 1)
URL="http://$IP:5000"

echo "Starting the app at $URL"

# Start the app in background
./ResponseApp --urls "http://0.0.0.0:5000" &

# Give it a second to start
sleep 2

# Open in browser
open "$URL"