ps -ef | grep BazarServer.Api.dll | grep -v grep |awk '{print "kill "$2}'|sh
