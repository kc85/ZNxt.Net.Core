# ZNxt.App

ZNxt.App is an open source, cross platform, highly scalable, micro-services based application development framework.
ZNxt.App designed on .net core 2.2 and can run on standalone on under container. 
Its has build it support of authentication, authorization, micro front end, dynamic module injection. 


## Docker images 


[ZNxtApp](https://cloud.docker.com/u/choudhurykhanin/repository/docker/choudhurykhanin/znxtapp)

## Docker commands

```python
#Gateway Service
sudo docker run -d -p 806:80 -p 4436:443  --restart=always --name znxt-app-run  choudhurykhanin/znxtapp:latest
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)
