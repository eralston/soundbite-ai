# soundbite-ai
Short-form video platform built on Microsoft Azure - TikTok for the Enterprise. Originally a micro-SaaS product, name Soundbite.AI, that was created and marketed from April 2020 to April 2025. Achieved six-figure Annual Recurring Revenue (ARR) until being decommissioned due to flat growth.

## Key Features
The platform was initially designed to manage secure audio content (MP3 audio) to service as an enterprise-ready podcast solution, growing into a video-centric hosting platform. The core of the application features include:
- Uploading, Encoding, and Publishing audio and video content
- Reactions, comment, and transcription for posted content
- Smart text content from video/audrio transcripts, including conversion to social media-style posts, blogs, and news
- E-Mail and SMS notifications
- Organization and group-based permission models
- Microsoft Teams app, including publishing into the MS Teams marketplace
- M365 SharePoint/Vive widgets

<img width="1920" height="1080" alt="Workflows and Security 2022Q3-917df75d-5ee0-44bc-ab1f-95bc2d6ac1b2" src="https://github.com/user-attachments/assets/64caa7c7-add7-4abf-9ae4-088821b21093" />

The enterprise-ready features includes:
- Reporting on audience behavior and platform activity
- SSO and directory data sync (users and groups) with Microsoft Entra (Azure Active Directory) and Okta
- Support for custom branding (icons and colors in the UI)
- Documents in the "Supporting Docs" folder that tell the story of our change management, security best practices, cost controls, and other non-code artifacts one needs to evaluate and deploy SaaS to enterprise and B2B clients

# Technology

The platformn was built in C# using .Net 6.0 and intended to be hosted exclusively on Microsoft Azure. This includes such highlights as:

## Applications
As far as distinct facets of the platform one can interact with:
- Front-end SPA Web Application, Miscrosoft Teams App, and SharePoint Add-In in Typescript with React
- Monolithic back-end API application in C#
- Azure Function microservices for event-driven async back-end tasking in C#, including processing notifications (e-mail, SMS, and MS Teams) and media

The top-level application projects then depend on "Soundbite.\*" libraries which implement various aspects of the product-specific code, breaking down into a core "Soundbite" library that is the primary common library of the application (contains all interfaces and DTOs), all of which depend on the various "Masticore.\*" modules, with the "Masticore" project being the final common foundation.

## Architecture
The following is a simplified diagram of the chosen architecture hosted in Microsoft Azure, including:
- Azure Web Apps for hosting the API, including use of a Web Application Firewall (WAF) and CDN via Azure Front Door
- Azure SQL Database for scalable data hosting, including the platform-level and organization-level data (the code is notably structured for multi-tenant architecture)
- Azure Storage for managing content, including the intial upload and processing of content, plus supporting content like company logos and user profile images
- Azure Queues for aynschronous back-end processing, such as connecting API actions to asynchronous notifications
- Azure Communication Services (ACS) for e-mail and SMS (originally using SendGrid and Twilio) 
- MS Graph for integration with Office 365 actions like user profile data, users and groups, and more

<img width="1920" height="1080" alt="Soundbite Platform Architecture 2022Q3" src="https://github.com/user-attachments/assets/de18a9a4-aa06-4f66-afd2-58323891968b" />

## Masticore
The solution can be thought of as having two levels: The top-level "Application-Level" of Soundbite that implements a bespoke behavior using multiple underlying components and the "Foundation-Level" that implements functioinality primarily as wrappers for third-party tools. This foundation level is referred to as "Masticore" and is part of an ongoing framework that has had many iterations going back to 2016. Masticore is structured such that:
- Reusable design patterns for common scenarios, such as an OOP queue pattern that could be synchronous or asynchronous, in-process or Azure queue-based based on setup in the application without changing client cod
- Abstractions for third-parties such that application-level code never knows the concrete implementation
- Provide code generation that converts .Net APIs into Typescript SDKs using reflection to generation interfaces and HTTP clients that match
- Foundations for database access and schema, including the lifecycle of in-memory unit test data vs local dev vs cloud-based production databases

All projects with the prefix "Masticore" will depend only on each other while the projects with the "Soundbite" prefix can depend on each other or Masticore libraries as needed, maintaining a solution-level approach to enforcing modularity and dependency management.
