This is an MVC project that aims to use Mobilt Bankid to enable Swedish citizens to make opinion polls on local politics. The project was made for an LU course in advanced database technologies. Visual Studio and Microsoft SSMS was utilized. Made by me and Dev Jakob. It showcases among other things the use of stored procedures, triggers, transaction management, user administration etc. All business logic is handled on the DB side tru stored procedures (T-SQL) (with the exception of salting/ hasing the BankIDs), and the controller logic/ presentation was made in Visual Studio(C#). The db-server was (locally) hosted on an Azure VM and the db had been configured for remote/ online access. We strived to keep the project as "modular" as possible, using an divison of sub projects for each of the core componentes contained in the same solution. Please note that since SSMS is not version controlled with the T-SQL/ configurations it contains that part of the project is not visuable here but the db structure can be seen in the Utilities folder, the project is complete and functional and recived a passing grade with praises for an intressting application.
The usage of a mockup Mobilt Bankid shows the usage of salted hashing for secure storage of sensitive user data. ChatGPT 4 was utilised during development as support and many of the methods are co-written and have been revised with and without AI support multiple times, but all of the architecture/ structure and design of the program/ db was made by us.


ACID
