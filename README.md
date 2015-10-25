# Sitecore Azure Diagnostics

Sitecore Azure Diagnostics is Sitecore CMS extension to output Sitecore Log statements to Microsoft Azure Storage Service using blobs. It includes two components. One component for Sitecore platform, and the other one for Sitecore Azure module.   

**Features:**
+ Append regular, WebDAV, Search, Crawling and Publishing Sitecore diagnostics
+ Based on [Azure Storage Services](https://msdn.microsoft.com/en-us/library/azure/gg433040.aspx)
+ Block Blobs are used to store diagnostics as a text data
+ Text data is compatible with [Sitecore Log Analyzer](https://marketplace.sitecore.net/Modules/Sitecore_Log_Analyzer.aspx) tool
+ Support multiple WebRole Instances and Deployments 
+ Reuse the same Azure Storage Service that the [Sitecore Azure](https://dev.sitecore.net/en/Downloads/Sitecore_Azure/80/Sitecore_Azure_80.aspx) module automatically creates during deployment
+ Clean up out-of-date blobs using Sitecore Agent
+ Support Sitecore Log Viewer application to open, download and deleted blobs.
