# MountyPython
**Title:** Monty-Python

**Motto:** Always look on the bright side of the board.

## Task Description:
You are working as a software developer in an agile software team. In the next sprint, you have
picked the task of developing a robust, platform-independent C# application to manage orders in
an SMT (Surface-Mount Technology) manufacturing environment. At the end of the sprint, you will
present your results to the team to facilitate an active discussion about the implementation.
Providing an outline of the modelled classes and their architecture would be beneficial.

## Requirements:
The application should support CRUD operations for **Orders**, **Boards**, and **Components**. The
relationships between the entities are as follows:
- An **Order** can include one or more **Boards**, and a Board can be produced in one or multiple orders.
- A **Board** can contain one or more **Components**, and a Component can be placed on one or more Boards.

The following attributes should be included for each entity:
- **Order:** Name, Description, Order Date
- **Board:** Name, Description, Length, Width
- **Component:** Name, Description, Quantity
 
You may extend these entities with additional properties as needed to accurately represent the
relationships between the objects (e.g., foreign keys, IDs, reference lists).

## Required actions:
- The application should allow the creation, editing, searching, and removal of one or more:
- components, boards and orders.
- The application should allow a download of an order – simulate a download to a production
- line where the order will be produced.

The application should serialize data to **JSON** for interoperability and persist it using a technology
of your choice (e.g., **MS SQL**, **SQLite**, **file storage**).

The solution should include a **logging mechanism** using a commonly adopted framework to
capture relevant actions and errors.

Apply object-oriented programming principles (e.g., **SOLID**, **DRY**), use standard **design patterns**,
and write a **unit test** for at least one representative method to ensure code quality.

## Additional Technical Requirements (Optional):
The following technical requirements are **optional** and can be implemented to demonstrate
advanced knowledge, completeness, and best practices:

- **Version Control**:
Use a **free version control system** such as **GitHub** or **Azure DevOps** to host the project.
All code changes should be committed to the remote repository. Upon completion, make
the repository accessible for **review** (public or restricted access).

- **CI/CD Pipeline**:
Set up a **build and release pipeline** using a free service like **GitHub Actions** or **Azure
DevOps Pipelines**. The pipeline should:
    - Automatically build the application on each commit.
    - Deploy the application to a **free-tier cloud provider** (e.g., Microsoft Azure).

- **Web Interface**:
The application should include a **web-based user interface** accessible via login. The
interface should:
    - Communicate with a **backend Web API** (e.g., ASP.NET Core Web API).
    - Enable CRUD operations for orders, boards, and components.
    - Include authentication (e.g., custom login or integration with a free identity provider
like **Azure AD B2C** or **Firebase Authentication**).

- **Persistence Between Restarts**:
Choose a persistence mechanism to store application data across restarts:
    - **Database** (e.g., Azure SQL, PostgreSQL, SQLite)
    - **Blob/file storage**

- **Dockerization / Containerization**:
Containerize the entire solution (API, database, frontend) using **Docker**. Provide a Dockerfile
and optionally a docker-compose.yml file to simplify deployment and local development.
The containerized app should be runnable both **locally** and in the **cloud**.

## Decisions

### Technology Stack:
**Motivation:** Time-to-Market, slightly more than a prototype, development & maintenance-costs, as well the running costs, licenses
- Technology: Aspire, Docker, RabbitMQ, Keycloak, Azure ACA, GitHub, GitHub Actions
- Usual Libs: Rebus (instead of MassTransit), FluentValidation, AwesomeAssertions (instead of FluentAssertions), Serilog, OpenAPI, Scalar
- Backend: ASP.NET Core Minimal Web API, PostgreSQL, Entity Framework Core, Keycloak (instead of Azure AD B2C), SignalR
- Frontend: Angular Material deployed as a static app on Yarp, NodeJS v24 LTS
- Code-Styling: standard C#/dotnet, TypeScript/Angular

### Architecture & Patterns:
- Onion, Vertical-Slices, Clean-Architecture
- Domain-Driven Design (DDD), Test-Driven Development (TDD), Event-Driven Architecture (EDA)
- Backend: CQRS, Commands & Consumers (instead of MediatR), Minimal API
- Due Aspire: 3 Microservices (2 for Business, 1 for Notifications) instead of 1 Monolith
- Frontend: TypeScript, Angular, Angular Material, Signal-Forms for simplicity and efficiency, NgRx - State Management
- E2E Test: Playwright if time allows
- First steps in reliable services: Aspire.ServiceDefaults add retry policy, fail-fast, circuit-breaker, timeout, logging, metrics, health-checks, etc.
- Self-Written map to DTO instead of AutoMapper due to license costs, and simplicity
- Monitoring: OpenTelemetry, Azure Insights

### Automatic Build & Deployment, Quality Gates:
- GitHub Actions with 2 branches (main & dev-branch), dev-branch deploys on dev, main deploys on prod
- Automatic versioning and tagging during build and deployment
- Automatic tests using Microsoft Test Platform (MTP) and XUnit v3 - MTP v2
- Playwright for E2E tests in pipeline if time allows
- Provisioning of Azure is maintained by Aspire (no manual interventions) - if time allows, a horizontal scalability will be provided

### Operations:
- The User/Customer should be able to see the available boards and components, create or cancel an order, see only his/her orders.
- The Manager should be able to delete or edit a board or component, see all the orders, change the status of an order to 'StartProcessing' or/and 'Completed' or/and 'Cancel'.
- Restrictions (alredy defined in Validators and in the domain model): you can change the order only if not started. You can cancel the order only if not started.

## Plan
1. Define the resources (ACA, PostgreSQL, RabbitMQ, Keycloak, Yarp) in AppHost for the infrastructure.
2. Create project structure, with 3 microservices (2 for Business, 1 for Notifications) and a frontend project.
3. Create shared library for common models, DTOs, and utilities (Logging, Repository, Messaging, Security, etc.).
4. Define the Domain Models, Validators and the Actions of the domain.
5. Implement placeholder for endpoints in the backend services.
6. Create Github repository and define the CI/CD Actions.
7. Add documentation (README.md), add skills, instruct copilot.
8. Implement backend services (maybe without authentication), make the requsts idempotent.
9. Implement frontend (maybe without authentication).
10. Define the Realm in Keycloak and register the test users.
11. Configure Yarp: request limits, CORS, etc.: this should be the first task before publishing.

## Known issues
- HIGH: security, default password for keycloak-admin in code
- HIGH: security, realm with test users in versioning systems
- HIGH: Lots of bugs and functionality breaks
- HIGH: Frontend Container in Azure ACA not reachable
- MEDIUM: inssuficient automatic tests: registering eventsource and consumers
-