# The Completed Architecture Roadmap


```
1.Creation of Model-Controller based API (Direct database queries & business logic inside the controller). DONE
```
##
```
2.Introduce services and take load off the controller (Extracting business logic into service classes).  DONE
```
##
```
3.Introduce DTOs (Data Transfer Objects) to the architecture (Decoupling database models from public API request/response contracts for validation and security).
```
##
```
4.Introduce Repositories (Abstracting data access queries away from services into a dedicated data access layer).
```
##
```
5.Make interfaces for the service and repositories (Applying the Dependency Inversion Principle, enabling loose coupling).
```
##
```
6.Introduce Global Exception Handling & Validation Pipelines (Centralizing error catching, custom domain exceptions, and request payload validation so controllers stay completely clean of try/catch noise).
```
##
```
7.Configure Dependency Injection (DI) & IoC Container (Wiring up interface abstractions to their concrete implementations so the framework automatically resolves dependencies).
```
##
```
8.Implement Unit & Integration Testing (Leveraging repository and service interfaces with mocking frameworks to test business logic independently of the database or HTTP layer).
```




building the project using the red green refactor approach


i try to write the test for test cases that doesnt exist yet and whats the test fail


then i try to match my test and make my code valid


and clean up the code 