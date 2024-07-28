This is a console application designed to interact with the sandbox Allegro API to retrieve billing entries and save them to a database. The application uses environment variables for sensitive information and configuration files for database connection strings so you need to setup your ones.


Set environment variables: Ensure the following environment variables are set:
•	ALLEGRO_CLIENT_ID
•	ALLEGRO_CLIENT_SECRET


Configure the database connection: Update the appsettings.json file with your database connection string:
{
    "ConnectionStrings": {
      "ConnectionS1": "YourConnectionStringHere"
  }
}
    
