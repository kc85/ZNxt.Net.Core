# ZNxt.App

ZNxt.App is an open source, cross platform, highly scalable, micro-services based application development framework.
ZNxt.App designed on .net core 3.1 and can run on standalone on under container. 
Its has build it support of authentication, authorization, micro front end, dynamic module injection. 


## Docker images 


[ZNxtApp](https://cloud.docker.com/u/choudhurykhanin/repository/docker/choudhurykhanin/znxtapp)

## Docker commands

```python
#Basic setup 
sudo docker run -d -p 806:80 -p 4436:443  --restart=always --name znxt-app-run  choudhurykhanin/znxtapp:latest

#Mapped static content on windows ## c:\myapp\wwwroot is the folder from your application static content
sudo docker run -d -p 806:80 -p 4436:443 -v c:\myapp\wwwroot:/app/wwwroot --restart=always --name znxt-app-run  choudhurykhanin/znxtapp:latest


#Mapped static content on linux ## /myapp/wwwroot:/app/wwwroot is the folder from your application static content
sudo docker run -d -p 806:80 -p 4436:443 -v c:\myapp\wwwroot:/app/wwwroot --restart=always --name znxt-app-run  choudhurykhanin/znxtapp:latest

```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## Feedback 
Please email to znxtapp@gmail.com for any feedback/issue/support

## License
[MIT](https://choosealicense.com/licenses/mit/)
