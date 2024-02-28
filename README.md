# JigsawMaker
This API is for a community of bird watchers. The idea is to photograph birds and upload photos with some basic details, like date, location, and species.

It appears on GitHub with the name "JigsawMaker," but it has nothing to do with puzzles anymore because I initially started working on an API for cutting images into jigsaw puzzle pieces. As I ran out of time, I had to switch to something simpler and limit the development to its essential functionalities. 

Far from being complete, it can already be used 

![Screenshot from React Front End](/img/screenshot.png)

The following features are currently implemented:
* it is working/functional API for a community of bird watchers
* uploads and stores files and data
    * data stored on Azure (MS SQL database)
    * uploaded images are stored in an Azure Storage Account as blobs
    * application secrets are stored in Azure Key Vault
* partial implementation of some logic for JWT-based authentication

Developed using: C#, .Net 8, Azure SQL Database, Azure Storage/Blobs and Azure Key Vault.

