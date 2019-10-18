# ZNxt.App

ZNxt.App is an open source, cross platform, highly scalable, micro-services based application development framework.
ZNxt.App designed on .net core 2.2 and can run on standalone on under container. 
Its has build it support of authentication, authorization, micro front end, dynamic module injection. 


## Docker images 


[ZNxtApp](https://cloud.docker.com/u/choudhurykhanin/repository/docker/choudhurykhanin/znxtapp)

## Docker commands

```python
#Gateway Service
sudo docker run -d -p 806:80 -p 4436:443 -e ConnectionString=mongodb://172.31.22.56:27071 -e DataBaseName=ZNxt_QA_gateway -e AppEndpoint=http://gateway.ZNxt.App -e ApiGatewayEndpoint=http://gateway.ZNxt.App -e SSOUrl=http://sso.ZNxt.App -e  AppSecret=MySecret --restart=always --name znxt-gateway-run  choudhurykhanin/znxtapp:latest

#SSO  Service
sudo docker run -d -p 807:80 -p 4437:443 -e ConnectionString=mongodb://172.31.22.56:27071 -e DataBaseName=ZNxt_QA_SSO -e AppEndpoint=http://sso.s2ftecnologies.com -e ApiGatewayEndpoint=http://gateway.ZNxt.App -e IsSSO=true -e  AppSecret=MySecret -e RelyingPartyUrls=http://ZNxt.App,http://www.ZNxt.App,http://admin.ZNxt.App,http://www.admin.ZNxt.App,https://localhost:44373 --restart=always --name znxt-sso-run  choudhurykhanin/znxtapp:latest

#Frontend Service
sudo docker run -d -p 804:80 -p 4434:443 -e ConnectionString=mongodb://172.31.22.56:27071 -e DataBaseName=ZNxt_QA_UI -e AppEndpoint=http://ZNxt.App -e ApiGatewayEndpoint=http://gateway.ZNxt.App -e SSOUrl=http://sso.ZNxt.App -e  AppSecret=MySecret --restart=always --name znxt-frontend-run  choudhurykhanin/znxtapp:latest


#Admin Service
sudo docker run -d -p 805:80 -p 4435:443 -e ConnectionString=mongodb://172.31.22.56:27071 -e DataBaseName=ZNxt_QA_Admin -e AppEndpoint=http://admin.ZNxt.App -e ApiGatewayEndpoint=http://gateway.ZNxt.App -e SSOUrl=http://sso.ZNxt.App -e  AppSecret=MySecret --restart=always --name znxt-admin-run  choudhurykhanin/znxtapp:latest
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)