# DashiToon - Online Reading Platform 🎨📚

DashiToon is an innovative platform that allows users to read comics, novels, and other forms of serialized content online. It provides a seamless reading experience for users, while offering authors tools to monetize their content through subscriptions and individual purchases. DashiToon also integrates advanced content moderation features to maintain a safe and inclusive environment for all users.

---

## 🚀 Features

- **User-friendly Reading Experience**: Enjoy serialized content in a clean and simple interface.
- **Subscription-based Monetization**: Authors can offer their content through subscriptions or pay-per-chapter models.
- **Commenting and Reviews**: Users can engage with content through comments and reviews.
- **Content Moderation**: Built-in moderation tools powered by AI to ensure a safe and respectful community.
- **Secure Payment Gateway**: Integrated with PayPal for secure transactions.

---

## 📑 Table of Contents

1. [Features](#features)
2. [Installation Guides](#installation-guides)
   - [System Requirements](#system-requirements)
   - [Installation Instructions](#installation-instructions)
3. [External Services](#external-services)
4. [License](#license)

---

## 2. Installation Guides

### 2.1 System Requirements

#### Hardware Requirements:
- **Processor**: 2 GHz or faster
- **Memory**: 4 GB RAM minimum
- **Storage**: 20 GB of available space

#### Software Requirements:
- **Host Operating System**: Ubuntu 22.04 or later
- **Node.js**: Version 21.x or later
- **ASP.NET Core Runtime**: Version 8.0.x or later
- **npm (Node Package Manager)**: Version 8.x or later
- **Postgres**: Version 16 or later
- **Web browser**: Chrome 131 or later

### 2.2 Installation Instructions

#### Step 1: Set Up a Production Environment

1. **Choose a Cloud Service Provider**:  
   Select a provider such as AWS, Google Cloud, Azure, or DigitalOcean.

2. **Set Up a Virtual Machine (VM)**:  
   Provision a virtual machine with Docker or use a container service (e.g., ECS, Fargate).

3. **Set Up Networking and Domain**:  
   Configure networking for the VM and set up domain names for the public-facing website and API service. You will need two domains (or subdomains):  
   - One for the website (e.g., `https://www.dashitoon.com`)
   - One for the API service (e.g., `https://api-dashitoon.com`)

---

#### Step 2: Set Up External Services

1. **Google OAuth 2.0**:  
   Follow [this guide](https://support.google.com/cloud/answer/6158849) to create OAuth 2.0 credentials. You will need the `client_id` and `client_secret`.

2. **Currency API**:  
   Create an account at [Currency API](https://app.currencyapi.com/dashboard) to obtain the API key for currency conversions.

3. **Postmark API**:  
   - Sign up for an account at [Postmark](https://postmarkapp.com/).
   - Under the "Servers" tab, create a new server.
   - Copy the Server API Token for later use.

4. **AWS Simple Storage Service (S3)**:  
   - Sign in to AWS and navigate to S3.
   - Create a new S3 bucket for media storage.
   - Create a new IAM user with programmatic access and attach the `AmazonS3FullAccess` policy.
   - Save the **Access Key ID** and **Secret Access Key**.

5. **OpenAI API**:  
   - Create an account at [OpenAI](https://platform.openai.com/signup).
   - Generate an API key at [this link](https://platform.openai.com/api-keys), with access to the Moderation API.

6. **Paypal Setup**:  
   - Sign up for a PayPal business account at [PayPal Developer](https://developer.paypal.com/).
   - Create an app under the REST API section to obtain the **Client ID** and **Secret**.
   - Add a webhook to receive PayPal event notifications, using your deployment URL (e.g., `https://api-dashitoon.com/webhook`). Copy the Webhook ID.

7. **ElasticSearch**:  
   - Sign up for an account at [Elastic Cloud](https://www.elastic.co/cloud).
   - Create a deployment and copy the **CloudID**.
   - Navigate to Kibana > Management > Security > API Keys and create an API key with index privileges for "dashi-toon-series-index" (read, write, create, delete).

---

#### Step 3: Build and Deploy the Software

1. **Prepare Environment Variables**:  
   - In the root folder of the source code package, open the `docker-compose.yml` file.
   - Replace `NEXT_PUBLIC_BACKEND_HOST` with the domain for the API service (e.g., `https://api-dashitoon.com`).
   - Replace `NEXT_PUBLIC_PAYPAL_CLIENT_ID` with the PayPal **Client ID** from the previous step.

   - Open `appsettings.json` in the `DashiToonAPI.Web` project and update the following:
     - `FrontEndHost`: Set to the public-facing website domain (e.g., `https://www.dashitoon.com`).
     - `Authentication.Google.*`: Add the Google OAuth credentials.
     - `PostMarkToken`: Add the Postmark API token.
     - `AWS.*`: Add the AWS S3 credentials (Access Key ID, Secret Access Key).
     - `Paypal.*`: Add the PayPal **Client ID** and **Secret**.
     - `ElasticSearch.*`: Add the ElasticSearch Cloud ID and API Key.
     - `CurrencyApi.ApiKey`: Add the Currency API key.
     - `OpenAi.APIKey`: Add the OpenAI API key for moderation.

2. **Deploy the Software**:  
   Run the following command from the root directory of the source code package:

   ```bash
   docker compose up -d --build

---

## 3. External Services

DashiToon relies on several third-party services to enable various platform features. These services include:

- **Google OAuth 2.0**: For user authentication.
- **Currency API**: For currency exchange rate conversions.
- **Postmark**: For email notifications and transactional emails.
- **AWS S3**: For media storage (e.g., images, comics, novels).
- **OpenAI API**: For content moderation.
- **PayPal**: To handle subscriptions and in-app purchases.
- **ElasticSearch**: For content search functionality, improving user experience in finding novels and comics.

---

## 4. License

DashiToon is licensed under the [MIT License](https://opensource.org/licenses/MIT). Feel free to contribute or modify the platform as per the terms of the license. All contributions must comply with the project's code of conduct and licensing terms.

