# Cloud Databases Assignment

## Info

Date: 29/10/2021  
Name: Sjors Grooff  
Student number: 634293  
Email: 634293@gmail.com  
Swagger url: https://cloud-databases.azurewebsites.net/api/swagger/ui  

### Mortgage request flow
1. A mortgage requst can be created by sending a post request to https://cloud-databases.azurewebsites.net/api/mortgage with the following example body:  
```
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "income": 0,
  "loans": 0
}
```
2. When the API is triggered its send to the mortgage request queue and stores the data in a Cosmos Database. Finally an HTTP OK response is returned.
3. At midnight a timer trigger is run which moves all messages in the request queueo the handle queue to be handled asyncronous.
4. A trigger is set on the handle queue which calculates the mortgage offer and creates a PDF which is stored in Azure Blob Storage.
As soon as the pdf is created a Sas Token will be created as well which wil be added to an email that will be send with SendGrid 7 hours later.

### Houses API
The houses api can list houses, add houses, remove houses and add images to houses.
These images are added to the blob storage which is publicly available.
The image needs to be send to /api/houses/{houseId}/images with the image file attached in a form-data body.

### Used resources
![image](https://user-images.githubusercontent.com/33691096/139484773-90d5941c-1e24-4287-89b3-5db324d8b69e.png)
