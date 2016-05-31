# MBrace.AWS

### Introduction 

AWS PaaS bindings for MBrace

## NuGet [![NuGet Badge](https://buildstats.info/nuget/MBrace.AWS?includePreReleases=true)](https://www.nuget.org/packages/MBrace.AWS)
`Install-Package MBrace.AWS`

### Building & Running Tests

Depending on your platform, you can build and run tests running `build.bat` or `build.cmd`. To successfully run unit tests, you need to have credentials set to your default profile in your local credentials store. Alternative, you could set the following environment variables:
```bash
export AWS_REGION="eu-central-1"                   # defaults to eu-central-1
export AWS_CREDENTIAL_STORE_PROFILE=<profile name> # uses "default" if unset
export AWS_ACCESS_KEY_ID=<your access key>         # your account's access key
export AWS_SECRET_ACCESS_KEY=<your secret key>     # your account's secret key
```

### Deploying an MBrace.AWS Cluster

There are currently 2 ways to deploy an MBrace.AWS cluster
* Standalone Deployments: Use `MBrace.AWS.StandaloneWorker` to manually deploy a cluster on-premises or a cloud VM.
* Elastic Beanstalk: Use the `MBrace.AWS.WebWorker` implementation to deploy a cluster using AWS Elastic Beanstalk.

### Build Status

* .NET/Windows [![Build status](https://ci.appveyor.com/api/projects/status/agctped28mcs1ukk?svg=true)](https://ci.appveyor.com/project/nessos/mbrace-aws)
* Mono/Linux [![Build Status](https://travis-ci.org/mbraceproject/MBrace.AWS.png?branch=master)](https://travis-ci.org/mbraceproject/MBrace.AWS/branches)


### Maintainers

* [@theburningmonk](https://twitter.com/theburningmonk)
* [@eiriktsarpalis](https://twitter.com/eiriktsarpalis)
