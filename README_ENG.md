# Ornamental plants learning app
**The server does stop after a period of time if not in use. At the first start it takes time for the backbone to wake up to be able to respond to the HTTP calls created by the mobile application**
**The sourcecode of the webAPI and the documentation of the server side are also available**

# Goal
The goal of this project is to help me learn ornamental plants, and to create a portfolio for the future.
The application helps with the learning of latin names of plants through a quiz. The app is also capable of using a third party API that can determine the name of a plant from a picture. There exists a list of all the available plants and some description about them. Authentication is also implemented, the user can create a list of his/her own plants, attach pictures and add description to them.

# Server
The server side of the application is created using Microsoft Azure. Refer to *serverdocumentation*.

# Client
The client's main purpose is to manage the UI based on data provided by the WebAPI. It is capable of sendig HTTP requests to the server, and display the results.

## How the app works
The main view is a list of all the available plants in the database. They can be loaded by a button press. *The server does stop after a period of time if not in use. At the first start it takes time for the backbone to wake up to be able to respond to the HTTP calls created by the mobile application.* After the initial request of data, more plants automatically load on scroll until all of them are loaded.
Selecting a plant, the app takes the user to a description page, where more pictures and details are provided.
A quiz page is implemented to help the user in learning the latin names of the plants. After a button press, the WebAPI sends the client a picture, and four latin names, one of which is the correct. Appropriate response is given to the user after an answer is given.
Plant identification is another function of the app. Navigating to this view, the user can capture a photo of a plant and send it to a third party API, which identifies the plant, and responds with a result, which is displayed by the app.
The fourth view is for authentication. The user is redirected to a registration/login page in a browser, where he/she can register/login. After a successful login, the user's own list is loaded - if there exists one -, and the user has an option to create new list items. While creating a new item, the user must give it a name, and there is an option to capture a photo or choose from the galery, and also to write a description about the plant.

# Future ideas
- Full CRUD operations on user list
- Personalized Quiz weighing
- Plant tipps on user list view