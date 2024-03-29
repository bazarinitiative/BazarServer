
# Install Step

## Prepare virtual machine

* Get a new virtual machine. See also <https://www.vultr.com/> . At least 2GB memory. Ubuntu 20.04 64bit prefered.

```shell
# add linux user
adduser bazar
# add user bazar to sudo list.
vi /etc/sudoers
# switch to user bazar
su bazar
```

* Install mongodb. MongoDB 4.4 or above. See also <https://docs.mongodb.com/manual/installation/> . You need to setup user name and password for mongodb <https://www.mongodb.com/docs/manual/core/authentication/> .

```shell
sudo apt install mongodb
```

```shell
#create mongodb user
mongosh
#modify bindIP to 0.0.0.0, port to a specific port AAAA (don't open default port to internet for security reason)
#enable security authentication
sudo vi /etc/mongodb.cfg
#restart mongodb
sudo systemctl restart mongod
#open port for internet access
sudo ufw allow AAAA
```

* Install dotnet6. See also <https://docs.microsoft.com/en-us/dotnet/core/install/linux>

```shell
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

```shell
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-6.0
```

* Install npm. See also <https://nodejs.org/en/download/package-manager/>

```shell
sudo apt install npm
sudo npm install -g yarn
```

* Enable port 80 and 443

```shell
sudo ufw allow 80
sudo ufw allow 443
```

## Prepare domain name

See also <https://dash.cloudflare.com/>

For example your domain name is `bazar.social` (In the following document, replace `bazar.social` to your own domain name)

Modify DNS:

* Create A record of `bazar.social` to the IP of your virtual machine

* Create A record of `api` to the IP of your virtual machine

* Create A record of `www` to the IP of your virtual machine

## Prepare source code

```shell
cd ~
mkdir work -p
cd work
git clone https://github.com/bazarinitiative/BazarServer.git
git clone https://github.com/bazarinitiative/BazarHtml.git
```

```shell
vi ~/work/BazarHtml/src/bazar-config.tsx
```

modify apihost value to `https://api.bazar.social`

## Prepare npm

```shell
cd ~/work/BazarHtml/
npm i
```

## Build souce code

```shell
cd ~/work/BazarServer/
sh build.sh
cd ~/work/BazarHtml/
sh build.sh
```

## Prepare environment variables

```shell
cd ~
vi .bashrc
```

Then add following content to the end of file:

```shell
export BazarBaseUrl="https://api.bazar.social/"
export BazarMongodb="mongodb://localhost:27017/"
# we use google API to detect PostConent language. see also <https://cloud.google.com/docs/authentication/api-keys>
export BazarTranslateKey="yourApiKey"
```

and replace the variables to proper value.

## Start BazarServer

```shell
cd ~
cd work/BazarServer
sh run.sh
```

## Setup nginx

See also <https://www.nginx.com/resources/wiki/start/topics/tutorials/install/>

Then config nginx:

```shell
cd /etc/nginx/conf.d
sudo vi bazar.conf
```

For example

```shell
upstream bazarapi {
        server 127.0.0.1:5000;

}
server {
        listen 80;
        server_name api.bazar.social;

        location / {
        # X-Forwarded-For for user real IP
        proxy_next_upstream error timeout invalid_header http_500 http_502 http_503 http_504;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header Host $http_host;
        proxy_set_header X_Nginx_Proxy true;
        proxy_pass http://bazarapi;
        proxy_redirect off;
        }
}

server {
        listen 80;
        server_name bazar.social www.bazar.social;
        root /home/bazar/run/BazarHtml/;

        location / {
        try_files $uri $uri/ @fallback;
        index index.html;
        proxy_set_header Host      $host;
        proxy_set_header X-Real-IP    $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        }
        location @fallback {
        rewrite ^.*$ /index.html break;
        }
}
```

## Handy script

* Update source code from github and rebuild all

```shell
vi ~/svrupdate.sh
vi ~/htmlupdate.sh
```

Content

```shell
echo ====================BazarServer========================
cd ~/work/BazarServer
git reset HEAD
git checkout .
git pull
sh build.sh
#killall dotnet
ps -ef | grep BazarServer.Api.dll | grep -v grep |awk '{print "kill "$2}'|sh
nohup sh run.sh &
sleep 1
```

```
echo
echo =====================BazarHtml=========================
cd ~/work/BazarHtml
git reset HEAD
git checkout .
git pull
sh build.sh
```

Usage

```shell
cd ~
sh svrupdate.sh
sh htmlupdate.sh
```
