# ZNxt

ZNxt is an open source, cross platform, highly scalable, micro-services based application development framework.
ZNxt designed on .net core 2.2 and can run on standalone on under container. 
Its has build it support of authentication, authorization, micro front end, dynamic module injection. 


## Docker images 


[ZNxtApp](https://cloud.docker.com/u/choudhurykhanin/repository/docker/choudhurykhanin/znxtapp)

[ZNxtSSO](https://cloud.docker.com/u/choudhurykhanin/repository/docker/choudhurykhanin/znxtsso)



## Docker commands

```python
sudo docker run -d -p 80:80 -p 434:443 -v /ZNxtApp/ZNxt_s2f:/app/ZNxtApp/ZNxt_s2f -e ASPNETCORE_URLS="https://+443;http://+80" -e ASPNETCORE_HTTPS_PORT=443 -e ASPNETCORE_Kestrel__Certificates__Default__Password="passs" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx -v /home/ubuntu/.aspnet/https:/https/ -e ConnectionString=mongodb://172.31.2.1:27071 -e DataBaseName=ZNxt  --restart=always --name s2fapp-run  znxtapp:latest
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)